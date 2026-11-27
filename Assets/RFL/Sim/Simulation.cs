#nullable enable
using System;

namespace RFL.Sim
{
    /// <summary>
    /// The fight rules. Holds only static data (moves and the two robots' bodies); every changing value is in
    /// <see cref="MatchState"/>. <see cref="Tick"/> runs 60 times a second and is fully deterministic.
    /// Split by system: Movement, Combat, Damage and Rounds.
    /// </summary>
    public sealed partial class Simulation
    {
        public const int TicksPerSecond = 60;
        public const int BufferFrames = 8; // Tech Spec, Input ▲
        public const int DoubleTapFrames = 12; // AC-064
        public const int TapWindowFrames = 6; // PF-DEC-30: up held past this jumps ▲
        public const int RingHalfMm = 3500; // 7 × 7 m inside the ropes ▲
        public const int StartDistanceMm = 2000;

        readonly MoveDef[] _moves;
        readonly FighterDef _f1;
        readonly FighterDef _f2;

        public Simulation(MoveDef[] moves, FighterDef p1, FighterDef p2)
        {
            if (moves == null || moves.Length < (int)MoveId.Count)
                throw new ArgumentException(
                    "Move table must have one entry per MoveId.",
                    nameof(moves)
                );
            _moves = moves;
            _f1 = p1;
            _f2 = p2;
        }

        public MoveDef Move(MoveId id) => _moves[(int)id];

        public FighterDef Fighter(int side) => side == 1 ? _f1 : _f2;

        /// <summary>A new match: full robots, round 1 intro.</summary>
        public MatchState NewMatch(MatchConfig config)
        {
            var s = new MatchState
            {
                StateVersion = MatchState.Version,
                Config = config,
                Round = 1,
            };
            s.P1 = FreshRobot(_f1);
            s.P2 = FreshRobot(_f2);
            StartRound(ref s);
            return s;
        }

        static RobotState FreshRobot(in FighterDef f)
        {
            int part = f.PartMax;
            return new RobotState
            {
                Hp = f.MaxHp,
                Stamina = f.MaxStamina,
                PartHead = part,
                PartTorso = part,
                PartArmL = part,
                PartArmR = part,
                PartLegs = part,
            };
        }

        public void Tick(ref MatchState s, InputFrame in1, InputFrame in2)
        {
            s.EventCount = 0;
            s.E0 = s.E1 = s.E2 = s.E3 = default;
            s.Frame++;

            if (s.Phase != MatchPhase.Fight)
            {
                // Inputs are locked: remember them so nothing held fires the moment the fight starts.
                Lock(ref s.P1, in1);
                Lock(ref s.P2, in2);
                TickLocked(ref s);
                return;
            }

            ReadInput(ref s, ref s.P1, in1);
            ReadInput(ref s, ref s.P2, in2);

            if (s.Hitstop > 0)
            {
                s.Hitstop--;
                return;
            }

            if (s.TimerTicks > 0 && --s.TimerTicks == 0)
            {
                EndRoundOnTime(ref s);
                return;
            }

            var m1 = StatusRules.For(s.P1, _f1);
            var m2 = StatusRules.For(s.P2, _f2);
            Act(ref s, ref s.P1, in1, _f1, m1, 1);
            Act(ref s, ref s.P2, in2, _f2, m2, 2);
            Separate(ref s.P1, ref s.P2);
            UpdateFacing(ref s);

            // Both attacks resolve against the same positions, so trades are possible.
            bool p1Hits = Connects(s.P1, s.P2, _f2);
            bool p2Hits = Connects(s.P2, s.P1, _f1);
            if (p1Hits)
                Resolve(ref s, ref s.P1, ref s.P2, _f1, _f2, m1, 1);
            if (p2Hits)
                Resolve(ref s, ref s.P2, ref s.P1, _f2, _f1, m2, 2);

            Recover(ref s.P1, _f1, m1);
            Recover(ref s.P2, _f2, m2);
            CheckKo(ref s);
        }

        static void Lock(ref RobotState r, InputFrame input)
        {
            r.PrevButtons = input.Buttons;
            r.PrevDirs = input.Dirs;
            r.BufFrames = 0;
            r.UpHeld = 0;
        }

        static Fix ClampToRing(Fix x, in FighterDef body)
        {
            Fix limit = Fix.FromMilli(RingHalfMm - body.RadiusMm);
            return Fix.Clamp(x, -limit, limit);
        }

        static bool AtRopes(Fix x, in FighterDef body) =>
            Fix.Abs(x) >= Fix.FromMilli(RingHalfMm - body.RadiusMm);

        void Separate(ref RobotState p1, ref RobotState p2)
        {
            Fix min = Fix.FromMilli(_f1.RadiusMm + _f2.RadiusMm);
            Fix gap = Fix.Abs(p2.X - p1.X);
            if (gap >= min)
                return;
            Fix half = (min - gap) / 2;
            int dir = p1.X <= p2.X ? 1 : -1;
            p1.X = ClampToRing(p1.X - half * dir, _f1);
            p2.X = ClampToRing(p2.X + half * dir, _f2);
            Fix still = min - Fix.Abs(p2.X - p1.X);
            if (still > Fix.Zero)
            {
                // One robot is at the ropes: the other takes the whole separation.
                p1.X = ClampToRing(p1.X - still * dir, _f1);
                p2.X = ClampToRing(p2.X + still * dir, _f2);
            }
        }

        static void UpdateFacing(ref MatchState s)
        {
            sbyte f = (sbyte)(s.P1.X <= s.P2.X ? 1 : -1);
            if (s.P1.Mode == RobotMode.Idle || s.P1.Mode == RobotMode.Walk)
                s.P1.Facing = f;
            if (s.P2.Mode == RobotMode.Idle || s.P2.Mode == RobotMode.Walk)
                s.P2.Facing = (sbyte)-f;
        }
    }
}
