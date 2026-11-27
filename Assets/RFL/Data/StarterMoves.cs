#nullable enable
using RFL.Sim;

namespace RFL.Data
{
    /// <summary>
    /// Demo move table ▲ (test values for Step 2 tuning, until the importer reads the Starter Move List tab).
    /// X = Jab, Y = Straight, A = Low Kick, B = Body Kick. String X, Y, X = Jab → Cross → Uppercut (launcher);
    /// each link is a true combo on hit. Jab and Body Kick put a counter-hit robot into Groggy.
    /// </summary>
    public static class StarterMoves
    {
        public static MoveDef[] Table()
        {
            var t = new MoveDef[(int)MoveId.Count];
            t[(int)MoveId.Jab] = new MoveDef
            {
                Id = MoveId.Jab,
                NameKey = "move.jab",
                StartButton = InputFrame.LeftPunch,
                Level = HitLevel.Mid,
                Startup = 7,
                Active = 2,
                Recovery = 12,
                Damage = 30,
                ReachMm = 1250,
                Hitstun = 17,
                Blockstun = 10,
                Hitstop = 6,
                PushbackMm = 80,
                Reaction = Reaction.Light,
                CounterReaction = Reaction.Groggy,
                TargetPart = Part.Head,
                OverloadFill = 60,
                LinkMove = MoveId.Cross,
                LinkButton = InputFrame.RightPunch,
                LinkUntil = 21,
            };
            t[(int)MoveId.Cross] = new MoveDef
            {
                Id = MoveId.Cross,
                NameKey = "move.cross",
                StartButton = 0,
                Level = HitLevel.Mid,
                Startup = 9,
                Active = 3,
                Recovery = 16,
                Damage = 42,
                ReachMm = 1350,
                Hitstun = 19,
                Blockstun = 12,
                Hitstop = 7,
                PushbackMm = 100,
                Reaction = Reaction.Light,
                TargetPart = Part.Head,
                OverloadFill = 80,
                LinkMove = MoveId.Uppercut,
                LinkButton = InputFrame.LeftPunch,
                LinkUntil = 28,
            };
            t[(int)MoveId.Uppercut] = new MoveDef
            {
                Id = MoveId.Uppercut,
                NameKey = "move.uppercut",
                StartButton = 0,
                Level = HitLevel.Mid,
                Startup = 13,
                Active = 3,
                Recovery = 24,
                Damage = 65,
                ReachMm = 1100,
                Hitstun = 0,
                Blockstun = 14,
                Hitstop = 10,
                PushbackMm = 150,
                Reaction = Reaction.Launch,
                TargetPart = Part.Head,
                OverloadFill = 120,
                JuggleCap = 90,
            };
            t[(int)MoveId.Straight] = new MoveDef
            {
                Id = MoveId.Straight,
                NameKey = "move.straight",
                StartButton = InputFrame.RightPunch,
                Level = HitLevel.Mid,
                Startup = 10,
                Active = 3,
                Recovery = 18,
                Damage = 45,
                ReachMm = 1400,
                Hitstun = 22,
                Blockstun = 12,
                Hitstop = 7,
                PushbackMm = 120,
                Reaction = Reaction.Heavy,
                TargetPart = Part.Torso,
                OverloadFill = 100,
            };
            // Kicks are slower and heavier than punches (PF sheet #57; PF-REQ-049: startup at least fastest punch + 3).
            t[(int)MoveId.LowKick] = new MoveDef
            {
                Id = MoveId.LowKick,
                NameKey = "move.low_kick",
                StartButton = InputFrame.LeftKick,
                Level = HitLevel.Low,
                Startup = 14,
                Active = 3,
                Recovery = 20,
                Damage = 40,
                ReachMm = 1300,
                Hitstun = 16,
                Blockstun = 12,
                Hitstop = 7,
                PushbackMm = 120,
                Reaction = Reaction.Light,
                CounterReaction = Reaction.Knockdown,
                TargetPart = Part.Legs,
                OverloadFill = 80,
            };
            t[(int)MoveId.BodyKick] = new MoveDef
            {
                Id = MoveId.BodyKick,
                NameKey = "move.body_kick",
                StartButton = InputFrame.RightKick,
                Level = HitLevel.Mid,
                Startup = 16,
                Active = 3,
                Recovery = 22,
                Damage = 55,
                ReachMm = 1450,
                Hitstun = 24,
                Blockstun = 14,
                Hitstop = 8,
                PushbackMm = 200,
                Reaction = Reaction.Heavy,
                CounterReaction = Reaction.Groggy,
                TargetPart = Part.Torso,
                OverloadFill = 150,
            };
            return t;
        }

        /// <summary>Body numbers by weight class ▲ (week plan §3: walk, dash, backdash, jump, push radius).</summary>
        public static FighterDef Fighter(StarterTypeDef def)
        {
            var f = new FighterDef
            {
                MaxHp = def.Hp,
                MaxStamina = def.Stamina,
                PowerPermille = 800 + 100 * def.Power,
                OverloadResist = def.OverloadResist,
            };
            switch (def.Class)
            {
                case WeightClass.Light:
                    f.WalkMmPerSecond = 1600;
                    f.RadiusMm = 450;
                    f.DashMm = 1800;
                    f.DashFrames = 14;
                    f.BackdashMm = 1400;
                    f.BackdashFrames = 16;
                    f.PreJumpFrames = 6;
                    f.AirFrames = 36;
                    f.JumpPeakMm = 900;
                    f.JumpMm = 1600;
                    f.LandingFrames = 6;
                    break;
                case WeightClass.Heavy:
                    f.WalkMmPerSecond = 1000;
                    f.RadiusMm = 600;
                    f.DashMm = 1200;
                    f.DashFrames = 18;
                    f.BackdashMm = 1000;
                    f.BackdashFrames = 20;
                    f.PreJumpFrames = 10;
                    f.AirFrames = 44;
                    f.JumpPeakMm = 500;
                    f.JumpMm = 1000;
                    f.LandingFrames = 12;
                    break;
                default:
                    f.WalkMmPerSecond = 1300;
                    f.RadiusMm = 500;
                    f.DashMm = 1500;
                    f.DashFrames = 16;
                    f.BackdashMm = 1200;
                    f.BackdashFrames = 18;
                    f.PreJumpFrames = 8;
                    f.AirFrames = 40;
                    f.JumpPeakMm = 700;
                    f.JumpMm = 1300;
                    f.LandingFrames = 9;
                    break;
            }
            return f;
        }
    }
}
