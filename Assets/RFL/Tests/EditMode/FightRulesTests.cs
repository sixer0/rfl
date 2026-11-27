#nullable enable
using System;
using NUnit.Framework;
using RFL.Data;
using RFL.Sim;

namespace RFL.Tests
{
    /// <summary>One or more tests per fight rule: movement, contact, reactions, damage, parts, OVERLOAD, rounds.</summary>
    public class FightRulesTests
    {
        static readonly InputFrame None = default;
        static readonly InputFrame X = new InputFrame(InputFrame.LeftPunch, 0);
        static readonly InputFrame Y = new InputFrame(InputFrame.RightPunch, 0);
        static readonly InputFrame A = new InputFrame(InputFrame.LeftKick, 0);
        static readonly InputFrame Right = new InputFrame(0, InputFrame.Right);
        static readonly InputFrame Left = new InputFrame(0, InputFrame.Left);
        static readonly InputFrame Up = new InputFrame(0, InputFrame.Up);

        static Simulation Sim(
            StarterType a = StarterType.Standard,
            StarterType b = StarterType.Standard
        ) =>
            new Simulation(
                StarterMoves.Table(),
                StarterMoves.Fighter(StarterTypes.Get(a)),
                StarterMoves.Fighter(StarterTypes.Get(b))
            );

        static MatchState Fight(
            Simulation sim,
            int p1Mm = -650,
            int p2Mm = 650,
            MatchMode mode = MatchMode.Training
        )
        {
            var s = sim.NewMatch(new MatchConfig { Mode = mode, Seed = 1 });
            s.P1.X = Fix.FromMilli(p1Mm);
            s.P2.X = Fix.FromMilli(p2Mm);
            s.Phase = MatchPhase.Fight;
            s.PhaseFrames = 0;
            return s;
        }

        static InputFrame[] Script(int ticks, params (int at, InputFrame f)[] presses)
        {
            var frames = new InputFrame[ticks];
            foreach (var (at, f) in presses)
                frames[at] = f;
            return frames;
        }

        static void Run(
            Simulation sim,
            ref MatchState s,
            InputFrame[] p1,
            InputFrame[]? p2 = null,
            Action<int, MatchState>? each = null
        )
        {
            int n = Math.Max(p1.Length, p2?.Length ?? 0);
            for (int t = 0; t < n; t++)
            {
                sim.Tick(
                    ref s,
                    t < p1.Length ? p1[t] : None,
                    p2 != null && t < p2.Length ? p2[t] : None
                );
                each?.Invoke(t, s);
            }
        }

        static bool Saw(MatchState s, SimEventType type)
        {
            for (int i = 0; i < s.EventCount; i++)
                if (s.Event(i).Type == type)
                    return true;
            return false;
        }

        // ---------------------------------------------------------------- 1. controls and movement

        [Test]
        public void DoubleTapWithin12FramesDashes()
        {
            var sim = Sim();
            var s = Fight(sim, -2000, 2000);
            bool dashed = false;
            Run(sim, ref s, Script(40, (0, Right), (12, Right)), each: (t, st) =>
                dashed |= st.P1.Mode == RobotMode.Dash
            );
            Assert.IsTrue(dashed);
            int moved = s.P1.X.ToMilli() + 2000;
            Assert.That(
                moved,
                Is.InRange(1500, 1560),
                "MIDDLE dash is 1,500 mm plus two tapped walk frames"
            );
            Assert.That(
                s.P1.Stamina,
                Is.InRange(1000 - Simulation.DashCost, 999),
                "stamina paid, regenerating since"
            );
        }

        [Test]
        public void SecondTapAt13FramesDoesNotDash()
        {
            var sim = Sim();
            var s = Fight(sim, -2000, 2000);
            bool dashed = false;
            Run(sim, ref s, Script(40, (0, Right), (13, Right)), each: (t, st) =>
                dashed |= st.P1.Mode == RobotMode.Dash
            );
            Assert.IsFalse(dashed);
        }

        [Test]
        public void DoubleTapBackBackdashes()
        {
            var sim = Sim();
            var s = Fight(sim, -500, 2500);
            bool backdashed = false;
            Run(sim, ref s, Script(40, (0, Left), (6, Left)), each: (t, st) =>
                backdashed |= st.P1.Mode == RobotMode.Backdash
            );
            Assert.IsTrue(backdashed);
            Assert.That(s.P1.X.ToMilli(), Is.InRange(-1760, -1690), "MIDDLE backdash is 1,200 mm");
        }

        [Test]
        public void DashNeedsStamina()
        {
            var sim = Sim();
            var s = Fight(sim, -2000, 2000);
            s.P1.Stamina = Simulation.DashCost - 20; // regenerates 3 a tick: still short two ticks later
            bool dashed = false;
            Run(sim, ref s, Script(30, (0, Right), (2, Right)), each: (t, st) =>
                dashed |= st.P1.Mode == RobotMode.Dash
            );
            Assert.IsFalse(dashed);
        }

        [Test]
        public void HoldingUpJumpsAndLowsWhiff()
        {
            var sim = Sim();
            var s = Fight(sim);
            var p1 = new InputFrame[80];
            for (int i = 0; i < 12; i++)
                p1[i] = Up;
            float peak = 0;
            int jumpTick = -1;
            Run(sim, ref s, p1, Script(80, (10, A)), (t, st) =>
            {
                if (jumpTick < 0 && st.P1.Mode == RobotMode.Jump)
                    jumpTick = t;
                peak = Math.Max(peak, st.P1.Y.ToMilli());
            });
            Assert.AreEqual(
                Simulation.TapWindowFrames,
                jumpTick,
                "up held past the 6-frame tap window"
            );
            Assert.That(peak, Is.InRange(690, 700), "MIDDLE jump peaks at 700 mm");
            Assert.AreEqual(1550, s.P1.Hp, "the low kick passes under the jump");
            Assert.AreEqual(RobotMode.Idle, s.P1.Mode, "landed and recovered");
        }

        [Test]
        public void TapUpDoesNotJump()
        {
            var sim = Sim();
            var s = Fight(sim);
            bool jumped = false;
            var p1 = new InputFrame[30];
            for (int i = 0; i < 4; i++)
                p1[i] = Up;
            Run(sim, ref s, p1, each: (t, st) => jumped |= st.P1.Mode == RobotMode.Jump);
            Assert.IsFalse(jumped, "a tap is reserved for sidestep");
        }

        // ---------------------------------------------------------------- 2–3. contact and reactions

        [Test]
        public void CounterHitDoesMoreAndMakesGroggy()
        {
            var sim = Sim();
            var s = Fight(sim);
            bool groggy = false,
                counter = false;
            Run(sim, ref s, Script(12, (0, X)), Script(12, (0, Y)), (t, st) =>
            {
                for (int i = 0; i < st.EventCount; i++)
                    if (st.Event(i).Type == SimEventType.Hit && st.Event(i).Side == 1)
                        counter |= st.Event(i).Counter;
                groggy |= st.P2.Mode == RobotMode.Groggy;
            });
            Assert.IsTrue(counter, "jab lands during the straight's startup");
            Assert.IsTrue(groggy);
            Assert.AreEqual(1550 - 39, s.P2.Hp, "33 × 1.2 = 39");
            Assert.AreEqual(1550, s.P1.Hp, "the straight was cancelled");
        }

        [Test]
        public void StraightIsAHeavyStaggerThatSlides()
        {
            var sim = Sim();
            var s = Fight(sim);
            int before = s.P2.X.ToMilli();
            Run(sim, ref s, Script(40, (0, Y)));
            Assert.Greater(s.P2.X.ToMilli() - before, 120 + 250, "pushback plus the stumble");
        }

        [Test]
        public void UppercutLaunchesAndJugglesStayShort()
        {
            var sim = Sim();
            var s = Fight(sim);
            var p1 = Script(160, (0, X), (6, Y), (14, X));
            for (int t = 40; t < 120; t += 9)
                p1[t] = X;
            bool launched = false;
            int maxJuggle = 0,
                juggleDamage = 0;
            Run(sim, ref s, p1, each: (t, st) =>
            {
                launched |= st.P2.Mode == RobotMode.Launched;
                maxJuggle = Math.Max(maxJuggle, st.P2.JuggleHits);
                juggleDamage = Math.Max(juggleDamage, st.P2.JuggleDamage);
            });
            Assert.IsTrue(launched);
            Assert.That(maxJuggle, Is.InRange(1, Simulation.MaxJuggleHits));
            Assert.LessOrEqual(juggleDamage, 90, "juggle damage cap of the uppercut");
            Assert.AreNotEqual(RobotMode.Launched, s.P2.Mode, "fell, lay down, woke up");
        }

        [Test]
        public void HitOnTheRopesAddsStun()
        {
            var sim = Sim();
            var s = Fight(sim, 1950, 2950); // P2 against the ropes (limit 3,000 mm)
            s.P1.Facing = 1;
            s.P2.Facing = -1;
            int stun = 0;
            Run(sim, ref s, Script(12, (0, X)), each: (t, st) =>
            {
                if (Saw(st, SimEventType.Hit))
                    stun = st.P2.Stun;
            });
            Assert.AreEqual(17 + Simulation.RopeStunFrames, stun);
        }

        [Test]
        public void OverloadStallsTakesMoreAndEmpties()
        {
            var sim = Sim();
            var s = Fight(sim);
            s.P2.Overload = 980;
            bool stalled = false;
            Run(sim, ref s, Script(30, (0, X)), each: (t, st) =>
                stalled |= st.P2.Mode == RobotMode.Overload
            );
            Assert.IsTrue(stalled);
            int hp = s.P2.Hp;
            Run(sim, ref s, Script(30, (0, Y)));
            // straight: 45 × 1.1 = 49; second hit of the combo × 0.85 = 41; OVERLOAD taken × 1.25 = 51
            Assert.AreEqual(51, hp - s.P2.Hp);
            Run(sim, ref s, Script(Simulation.OverloadFrames, (0, None)));
            Assert.AreNotEqual(RobotMode.Overload, s.P2.Mode);
            Assert.AreEqual(0, s.P2.Overload, "the stall empties the gauge");
        }

        // ---------------------------------------------------------------- 4. damage and parts

        [Test]
        public void DamageFormulaWorkedExamples()
        {
            var t = StarterMoves.Table();
            var standard = StarterMoves.Fighter(StarterTypes.Get(StarterType.Standard));
            var power = StarterMoves.Fighter(StarterTypes.Get(StarterType.Power));
            var ok = StatusRules.For(
                new RobotState
                {
                    PartArmL = 1,
                    PartArmR = 1,
                    PartLegs = 1,
                    PartHead = 1,
                    Stamina = 1000,
                },
                standard
            );
            var idle = new RobotState();
            Assert.AreEqual(
                33,
                Simulation.ComputeDamage(t[1], standard, ok, idle, false, 0),
                "jab, STANDARD"
            );
            Assert.AreEqual(
                39,
                Simulation.ComputeDamage(t[1], power, ok, idle, false, 0),
                "jab, POWER 1.3"
            );
            Assert.AreEqual(
                39,
                Simulation.ComputeDamage(t[1], standard, ok, idle, true, 0),
                "counter × 1.2"
            );
            Assert.AreEqual(
                18,
                Simulation.ComputeDamage(t[1], standard, ok, idle, false, 3),
                "fourth hit × 0.55"
            );
            var stalled = new RobotState { Mode = RobotMode.Overload };
            Assert.AreEqual(
                41,
                Simulation.ComputeDamage(t[1], standard, ok, stalled, false, 0),
                "OVERLOAD taken × 1.25"
            );
            var brokenArm = StatusRules.For(
                new RobotState
                {
                    PartArmL = 0,
                    PartArmR = 1,
                    PartLegs = 1,
                    PartHead = 1,
                    Stamina = 1000,
                },
                standard
            );
            Assert.AreEqual(
                24,
                Simulation.ComputeDamage(t[1], standard, brokenArm, idle, false, 0),
                "broken left arm × 0.75"
            );
            Assert.AreEqual(
                49,
                Simulation.ComputeDamage(t[4], standard, brokenArm, idle, false, 0),
                "straight uses the right arm: untouched"
            );
            var juggled = new RobotState
            {
                Mode = RobotMode.Launched,
                JuggleCap = 90,
                JuggleDamage = 85,
            };
            Assert.AreEqual(
                5,
                Simulation.ComputeDamage(t[1], standard, ok, juggled, false, 0),
                "capped by the launcher"
            );
        }

        [Test]
        public void PartsTakeHitsAndAtMostTwoBreak()
        {
            var sim = Sim();
            var s = Fight(sim);
            s.P2.PartHead = 1;
            s.P2.PartTorso = 1;
            s.P2.PartLegs = 1;
            int broken = 0;
            var p1 = Script(200, (0, X), (60, Y), (130, A));
            for (int i = 100; i < 112; i++)
                p1[i] = Right; // close the distance the stumble opened
            Run(sim, ref s, p1, each: (t, st) =>
            {
                for (int i = 0; i < st.EventCount; i++)
                    if (st.Event(i).Type == SimEventType.PartBroken)
                        broken++;
            });
            Assert.AreEqual(0, s.P2.PartHead, "jab breaks the head");
            Assert.AreEqual(0, s.P2.PartTorso, "straight breaks the torso");
            Assert.AreEqual(1, s.P2.PartLegs, "a third part stops one step short (REQ-060)");
            Assert.AreEqual(2, broken);
            Assert.Greater(s.P2.Hp, 0, "parts never knock out a robot (REQ-061)");
        }

        [Test]
        public void BlockingCostsTheBlockingArm()
        {
            var sim = Sim();
            var s = Fight(sim);
            int arm = s.P2.PartArmR;
            var back = new InputFrame(0, InputFrame.Right);
            var p2 = new InputFrame[30];
            for (int i = 0; i < p2.Length; i++)
                p2[i] = back;
            Run(sim, ref s, Script(30, (0, X)), p2);
            Assert.AreEqual(1550, s.P2.Hp, "no HP chip");
            Assert.AreEqual(
                arm - 4,
                s.P2.PartArmR,
                "a left jab is blocked with the right arm: 33 × 15 % = 4"
            );
        }

        [Test]
        public void BrokenArmAddsRecoveryButNotStartup()
        {
            var sim = Sim();
            var s = Fight(sim);
            s.P1.PartArmL = 0;
            int hitTick = -1,
                idleTick = -1;
            Run(sim, ref s, Script(40, (0, X)), each: (t, st) =>
            {
                if (hitTick < 0 && Saw(st, SimEventType.Hit))
                    hitTick = t;
                if (idleTick < 0 && t > 0 && st.P1.Mode == RobotMode.Idle)
                    idleTick = t;
            });
            Assert.AreEqual(7, hitTick, "startup unchanged");
            int hitstop = StarterMoves.Table()[1].Hitstop;
            Assert.AreEqual(21 + 2 + hitstop, idleTick, "21 frames + 2 recovery + hitstop");
        }

        // ---------------------------------------------------------------- 5. rounds and winners

        static void FinishRound(Simulation sim, ref MatchState s)
        {
            for (
                int i = 0;
                i < Simulation.RoundOverFrames + 5 && s.Phase == MatchPhase.RoundOver;
                i++
            )
                sim.Tick(ref s, None, None);
        }

        [Test]
        public void KoWinsTheRoundAndCornerBreakHeals()
        {
            var sim = Sim();
            var s = Fight(sim);
            s.P2.Hp = 10;
            s.P2.Overload = 400;
            bool ko = false;
            Run(sim, ref s, Script(12, (0, X)), each: (t, st) => ko |= Saw(st, SimEventType.Ko));
            Assert.IsTrue(ko);
            Assert.AreEqual(MatchPhase.RoundOver, s.Phase);
            Assert.AreEqual(
                (1, 0, (byte)1, RoundEnd.Ko),
                (s.WinsP1, s.WinsP2, s.RoundWinner, s.RoundEndReason)
            );
            FinishRound(sim, ref s);
            Assert.AreEqual(MatchPhase.CornerBreak, s.Phase);

            int p1Hp = s.P1.Hp;
            sim.ApplyCornerBreak(
                ref s,
                Part.Head,
                Simulation.MostDamagedPart(s.P2, sim.Fighter(2))
            );
            Assert.AreEqual(2, s.Round);
            Assert.AreEqual(MatchPhase.Intro, s.Phase);
            Assert.AreEqual(387, s.P2.Hp, "KO'd: 10 % back is 155, floor is 25 % of 1,550");
            Assert.AreEqual(p1Hp, s.P1.Hp, "P1 lost nothing, so nothing returns");
            Assert.AreEqual(0, s.P2.Overload);
        }

        [Test]
        public void TimeOutGoesToTheLargerHpShare()
        {
            var sim = Sim(StarterType.Standard, StarterType.Power);
            var s = Fight(sim);
            s.P1.Hp = 800; // 51.6 % of 1,550
            s.P2.Hp = 850; // 49.8 % of 1,708
            s.TimerTicks = 2;
            Run(sim, ref s, Script(3));
            Assert.AreEqual((byte)1, s.RoundWinner, "share, not raw HP");
            Assert.AreEqual(RoundEnd.Time, s.RoundEndReason);
        }

        [Test]
        public void EqualSharesFallBackToBrokenPartsThenStamina()
        {
            var sim = Sim();
            var s = Fight(sim);
            s.P1.Hp = s.P2.Hp = 700;
            s.P2.PartLegs = 0;
            s.TimerTicks = 1;
            Run(sim, ref s, Script(2));
            Assert.AreEqual((byte)1, s.RoundWinner, "fewer broken parts");

            s = Fight(sim);
            s.P1.Hp = s.P2.Hp = 700;
            s.P1.Stamina = 300;
            s.TimerTicks = 1;
            Run(sim, ref s, Script(2));
            Assert.AreEqual((byte)2, s.RoundWinner, "more stamina");
        }

        [Test]
        public void FirstToThreeWinsTheMatch()
        {
            var sim = Sim();
            var s = Fight(sim);
            for (int round = 1; round <= 3; round++)
            {
                s.P2.Hp = 5;
                s.P1.X = Fix.FromMilli(-650);
                s.P2.X = Fix.FromMilli(650);
                s.Phase = MatchPhase.Fight;
                Run(sim, ref s, Script(12, (0, X)));
                FinishRound(sim, ref s);
                if (round < 3)
                {
                    Assert.AreEqual(MatchPhase.CornerBreak, s.Phase);
                    sim.ApplyCornerBreak(ref s, Part.Head, Part.Head);
                }
            }
            Assert.AreEqual(MatchPhase.MatchOver, s.Phase);
            Assert.AreEqual((byte)1, s.MatchWinner);
            Assert.AreEqual(3, s.WinsP1);
        }

        [Test]
        public void HundredCornerBreaksFollowTheRules()
        {
            var sim = Sim();
            var body = sim.Fighter(1);
            uint rng = 99;
            int Next(int max)
            {
                rng = rng * 1664525u + 1013904223u;
                return (int)((rng >> 8) % (uint)max);
            }
            for (int i = 0; i < 100; i++)
            {
                var s = Fight(sim);
                s.Phase = MatchPhase.CornerBreak;
                s.P1.Hp = Next(body.MaxHp + 1);
                s.P1.KodThisRound = s.P1.Hp == 0;
                s.P1.Overload = Next(1001);
                var part = (Part)Next(5);
                int partHp = Next(body.PartMax + 1);
                s.P1.SetPart(part, partHp);
                int hp = s.P1.Hp;
                sim.ApplyCornerBreak(ref s, part, Part.Head);

                int expect = Math.Min(body.MaxHp, hp + (body.MaxHp - hp) * 100 / 1000);
                if (hp == 0)
                    expect = Math.Max(expect, body.MaxHp * 250 / 1000);
                Assert.AreEqual(expect, s.P1.Hp, $"break {i}");
                Assert.AreEqual(0, s.P1.Overload);
                var before = StatusRules.StateOf(partHp, body.PartMax);
                var after = StatusRules.StateOf(s.P1.GetPart(part), body.PartMax);
                Assert.AreEqual(
                    before == PartState.Ok ? PartState.Ok : (PartState)((int)before - 1),
                    after,
                    $"break {i}: one step"
                );
            }
        }
    }
}
