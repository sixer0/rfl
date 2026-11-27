#nullable enable
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RFL.Data;
using RFL.Sim;

namespace RFL.Tests
{
    /// <summary>The combat stack: frame data, the X, Y, X string, block, determinism and the float ban.</summary>
    public class SimulationTests
    {
        static readonly InputFrame None = default;
        static readonly InputFrame X = new InputFrame(InputFrame.LeftPunch, 0);
        static readonly InputFrame Y = new InputFrame(InputFrame.RightPunch, 0);

        static Simulation NewSim(
            StarterType p1 = StarterType.Standard,
            StarterType p2 = StarterType.Standard
        ) =>
            new Simulation(
                StarterMoves.Table(),
                StarterMoves.Fighter(StarterTypes.Get(p1)),
                StarterMoves.Fighter(StarterTypes.Get(p2))
            );

        static MatchState Close(
            Simulation sim,
            MatchMode mode = MatchMode.VersusCpu,
            int distanceMm = 1300
        )
        {
            var s = sim.NewMatch(
                new MatchConfig
                {
                    Mode = mode,
                    P1 = StarterType.Standard,
                    P2 = StarterType.Standard,
                    Seed = 1,
                }
            );
            s.P1.X = Fix.FromMilli(-distanceMm / 2);
            s.P2.X = Fix.FromMilli(distanceMm / 2);
            s.Phase = MatchPhase.Fight; // skip the round intro
            s.PhaseFrames = 0;
            return s;
        }

        /// <summary>Runs ticks; P1 input from the script (index = tick), P2 from <paramref name="p2"/>.</summary>
        static void Run(
            Simulation sim,
            ref MatchState s,
            InputFrame[] p1,
            int ticks,
            InputFrame p2 = default,
            System.Action<int, MatchState>? each = null
        )
        {
            for (int t = 0; t < ticks; t++)
            {
                sim.Tick(ref s, t < p1.Length ? p1[t] : None, p2);
                each?.Invoke(t, s);
            }
        }

        static InputFrame[] Script(int ticks, params (int at, InputFrame f)[] presses)
        {
            var frames = new InputFrame[ticks];
            foreach (var (at, f) in presses)
                frames[at] = f;
            return frames;
        }

        [Test]
        public void FixMathIsExactAndCultureFree()
        {
            Assert.AreEqual(Fix.FromInt(3), Fix.FromInt(6) / Fix.FromInt(2));
            Assert.AreEqual(Fix.FromRatio(3, 2), Fix.FromMilli(1500));
            Assert.AreEqual(-1500, (Fix.FromMilli(-1000) * Fix.FromRatio(3, 2)).ToMilli());
            Assert.AreEqual(2, Fix.FromMilli(1500).RoundToInt());
            var saved = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture =
                    new System.Globalization.CultureInfo("id-ID");
                Assert.AreEqual(
                    "1.500",
                    Fix.FromMilli(1500).ToString(),
                    "dot decimal under any culture"
                );
                Assert.AreEqual("-0.250", Fix.FromMilli(-250).ToString());
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = saved;
            }
        }

        [Test]
        public void MatchStateIsOneCopyableValue()
        {
            Assert.IsFalse(
                System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<MatchState>(),
                "MatchState must stay unmanaged (REQ-072)"
            );
        }

        [Test]
        public void JabHitsOnItsEighthFrame()
        {
            var sim = NewSim();
            var s = Close(sim);
            int hitTick = -1;
            Run(sim, ref s, Script(30, (0, X)), 30, each: (t, st) =>
            {
                if (hitTick < 0 && st.EventCount > 0 && st.E0.Type == SimEventType.Hit)
                    hitTick = t;
            });
            Assert.AreEqual(
                7,
                hitTick,
                "pressed on tick 0, the jab's first active frame is its 8th"
            );
            Assert.AreEqual(1550 - 33, s.P2.Hp, "jab 30 × STANDARD power 1.1");
        }

        [Test]
        public void JabWhiffsOutOfRange()
        {
            var sim = NewSim();
            var s = Close(sim, distanceMm: 2000);
            Run(sim, ref s, Script(40, (0, X)), 40);
            Assert.AreEqual(1550, s.P2.Hp);
        }

        [Test]
        public void XyxIsAThreeHitTrueCombo()
        {
            var sim = NewSim();
            var s = Close(sim);
            int maxCombo = 0;
            bool dropped = false;
            var p1 = Script(90, (0, X), (6, Y), (14, X));
            Run(sim, ref s, p1, 90, each: (t, st) =>
            {
                for (int i = 0; i < st.EventCount; i++)
                {
                    var e = st.Event(i);
                    if (e.Type == SimEventType.Hit)
                        maxCombo = System.Math.Max(maxCombo, e.ComboHits);
                }
                if (
                    maxCombo > 0
                    && maxCombo < 3
                    && (st.P2.Mode == RobotMode.Idle || st.P2.Mode == RobotMode.Walk)
                )
                    dropped = true;
            });
            Assert.AreEqual(3, maxCombo, "jab, cross, uppercut all connect");
            Assert.IsFalse(dropped, "the defender never recovers between hits");
            // Each step floors: 30×1.1 = 33; 42×1.1 = 46 ×0.85 = 39; 65×1.1 = 71 ×0.70 = 49
            Assert.AreEqual(1550 - 33 - 39 - 49, s.P2.Hp);
        }

        [Test]
        public void YAfterTheWindowIsAStraightNotACross()
        {
            var sim = NewSim();
            var s = Close(sim);
            MoveId second = MoveId.None;
            Run(sim, ref s, Script(80, (0, X), (40, Y)), 80, each: (t, st) =>
            {
                if (t > 25 && st.P1.Mode == RobotMode.Attack)
                    second = st.P1.Move;
            });
            Assert.AreEqual(MoveId.Straight, second);
        }

        [Test]
        public void XyxEnteredEarlyStillComes()
        {
            var sim = NewSim();
            var s = Close(sim);
            int hits = 0;
            // All three presses inside the jab's startup: links queue and fire in order.
            Run(sim, ref s, Script(90, (0, X), (2, Y), (12, X)), 90, each: (t, st) =>
            {
                for (int i = 0; i < st.EventCount; i++)
                    if (st.Event(i).Type == SimEventType.Hit)
                        hits++;
            });
            Assert.AreEqual(3, hits);
        }

        [Test]
        public void HoldingBackBlocksTheCombo()
        {
            var sim = NewSim();
            var s = Close(sim);
            var back = new InputFrame(0, InputFrame.Right); // P2 faces left, so right is back
            int blocks = 0;
            Run(sim, ref s, Script(90, (0, X), (6, Y), (14, X)), 90, back, (t, st) =>
            {
                for (int i = 0; i < st.EventCount; i++)
                    if (st.Event(i).Type == SimEventType.Block)
                        blocks++;
            });
            Assert.AreEqual(1550, s.P2.Hp, "blocked hits do no damage");
            Assert.GreaterOrEqual(blocks, 1);
        }

        static readonly InputFrame A = new InputFrame(InputFrame.LeftKick, 0);
        static readonly InputFrame B = new InputFrame(InputFrame.RightKick, 0);
        static readonly InputFrame P2Back = new InputFrame(0, InputFrame.Right);
        static readonly InputFrame P2DownBack = new InputFrame(
            0,
            InputFrame.Right | InputFrame.Down
        );

        static (int hits, int blocks, int firstHitTick) Count(
            Simulation sim,
            ref MatchState s,
            InputFrame[] p1,
            int ticks,
            InputFrame p2
        )
        {
            int hits = 0,
                blocks = 0,
                first = -1;
            Run(sim, ref s, p1, ticks, p2, (t, st) =>
            {
                for (int i = 0; i < st.EventCount; i++)
                {
                    var e = st.Event(i);
                    if (e.Type == SimEventType.Hit)
                    {
                        hits++;
                        if (first < 0)
                            first = t;
                    }
                    if (e.Type == SimEventType.Block)
                        blocks++;
                }
            });
            return (hits, blocks, first);
        }

        [Test]
        public void KicksAreSlowerThanTheFastestPunchPlusThree()
        {
            var t = StarterMoves.Table();
            int fastestPunch = System.Math.Min(
                t[(int)MoveId.Jab].Startup,
                t[(int)MoveId.Straight].Startup
            );
            Assert.GreaterOrEqual(t[(int)MoveId.LowKick].Startup, fastestPunch + 3);
            Assert.GreaterOrEqual(t[(int)MoveId.BodyKick].Startup, fastestPunch + 3);
        }

        [Test]
        public void ALowKickHitsOnFrame15AndBeatsAStandingBlock()
        {
            var sim = NewSim();
            var s = Close(sim);
            var r = Count(sim, ref s, Script(60, (0, A)), 60, P2Back);
            Assert.AreEqual(1, r.hits, "a low goes under a standing guard");
            Assert.AreEqual(14, r.firstHitTick, "low kick hits on its 15th frame");
            Assert.AreEqual(1550 - 44, s.P2.Hp, "40 × 1.1");
        }

        [Test]
        public void CrouchingGuardStopsTheLowKick()
        {
            var sim = NewSim();
            var s = Close(sim);
            var r = Count(sim, ref s, Script(60, (0, A)), 60, P2DownBack);
            Assert.AreEqual(0, r.hits);
            Assert.AreEqual(1, r.blocks);
            Assert.AreEqual(1550, s.P2.Hp);
        }

        [Test]
        public void MidsHitACrouchingGuard()
        {
            var sim = NewSim();
            var s = Close(sim);
            var r = Count(sim, ref s, Script(60, (0, B)), 60, P2DownBack);
            Assert.AreEqual(1, r.hits, "body kick is mid");
            Assert.AreEqual(16, r.firstHitTick, "body kick hits on its 17th frame");

            s = Close(sim);
            r = Count(sim, ref s, Script(60, (0, X)), 60, P2DownBack);
            Assert.AreEqual(1, r.hits, "punches are mid");
        }

        [Test]
        public void StandingGuardStopsTheBodyKick()
        {
            var sim = NewSim();
            var s = Close(sim);
            var r = Count(sim, ref s, Script(60, (0, B)), 60, P2Back);
            Assert.AreEqual(0, r.hits);
            Assert.AreEqual(1, r.blocks);
        }

        [Test]
        public void TrainingUsesTheRealRules()
        {
            var sim = NewSim();
            var s = Close(sim, MatchMode.Training);
            s.P2.Hp = 20;
            Run(sim, ref s, Script(30, (0, X)), 30);
            Assert.AreEqual(0, s.P2.Hp, "the training robot can be knocked out");
            Assert.AreEqual(MatchPhase.RoundOver, s.Phase);
        }

        [Test]
        public void SameInputsGiveTheSameHashAndRestoreReplays()
        {
            var sim = NewSim(StarterType.Power, StarterType.Speed);
            var cfg = new MatchConfig
            {
                Mode = MatchMode.VersusPlayer,
                P1 = StarterType.Power,
                P2 = StarterType.Speed,
                Seed = 7,
            };
            uint rng = 12345;
            InputFrame Next()
            {
                rng = rng * 1664525u + 1013904223u;
                return new InputFrame((byte)((rng >> 24) & 3), (byte)((rng >> 16) & 3));
            }
            var a1 = new InputFrame[1200];
            var a2 = new InputFrame[1200];
            for (int i = 0; i < a1.Length; i++)
            {
                a1[i] = Next();
                a2[i] = Next();
            }

            ulong RunAll(out MatchState snapshotAt600, out ulong hashAt600)
            {
                var s = sim.NewMatch(cfg);
                snapshotAt600 = default;
                hashAt600 = 0;
                for (int i = 0; i < a1.Length; i++)
                {
                    if (i == 600)
                    {
                        snapshotAt600 = s;
                        hashAt600 = s.Hash();
                    }
                    sim.Tick(ref s, a1[i], a2[i]);
                }
                return s.Hash();
            }

            ulong h1 = RunAll(out var snap, out var mid);
            ulong h2 = RunAll(out _, out _);
            Assert.AreEqual(h1, h2, "same seed and inputs, same result");

            var restored = snap; // PF-REQ-072: capture is a copy
            Assert.AreEqual(mid, restored.Hash());
            for (int i = 600; i < a1.Length; i++)
                sim.Tick(ref restored, a1[i], a2[i]);
            Assert.AreEqual(h1, restored.Hash(), "restore and re-run reproduces the match");
        }

        static string StripComment(string line)
        {
            int c = line.IndexOf("//", System.StringComparison.Ordinal);
            return c >= 0 ? line.Substring(0, c) : line;
        }

        [Test]
        public void SimulationCodeHasNoFloatsOrUnity()
        {
            var banned = new Regex(@"\b(float|double|UnityEngine|System\.Random|Mathf)\b");
            var offenders = Directory
                .GetFiles("Assets/RFL/Sim", "*.cs")
                .SelectMany(f =>
                    File.ReadAllLines(f).Select((line, i) => (f, line: StripComment(line), i))
                )
                .Where(x =>
                    !x.line.TrimStart().StartsWith("//")
                    && !x.line.TrimStart().StartsWith("///")
                    && banned.IsMatch(x.line)
                )
                .Select(x => $"{Path.GetFileName(x.f)}:{x.i + 1}: {x.line.Trim()}")
                .ToList();
            Assert.IsEmpty(offenders, string.Join("\n", offenders));
        }
    }
}
