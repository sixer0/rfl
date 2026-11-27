#nullable enable
using System;

namespace RFL.Sim
{
    /// <summary>Hits and contact: hurt state by posture, block, reactions, counter-hit, OVERLOAD, ropes.</summary>
    public sealed partial class Simulation
    {
        public const int BlockHitstop = 4;
        public const int MaxJuggleHits = 2; // PF sheet #57 short juggles ▲
        public const int JugglePopFrames = 6; // each juggle hit lifts the robot back up a little
        public const int GroggyFrames = 40; // ▲
        public const int OverloadFrames = 90; // about 1.5 s (PF GDD)
        public const int HeavySlideMm = 300; // heavy stagger stumble ▲
        public const int HeavySlideFrames = 8;
        public const int RopeStunFrames = 4; // extra stun when pinned on the ropes ▲
        public const int BlockChipPermille = 150; // part damage to the blocking limb ▲
        public const int MaxBroken = 2; // PF-REQ-060

        /// <summary>Does the attacker's active move touch the defender this tick?</summary>
        bool Connects(in RobotState a, in RobotState d, in FighterDef defBody)
        {
            if (a.Mode != RobotMode.Attack || a.MoveConnected || d.Down)
                return false;
            var m = _moves[(int)a.Move];
            if (!m.IsActive(a.MoveFrame))
                return false;
            if (m.Level == HitLevel.High && d.Crouching && d.Grounded)
                return false; // highs whiff on a crouching robot
            if (m.Level == HitLevel.Low && d.Airborne)
                return false; // lows whiff on an airborne robot
            if (d.Mode == RobotMode.Launched && d.JuggleHits >= MaxJuggleHits)
                return false;
            Fix ahead = (d.X - a.X) * a.Facing; // distance to the defender's centre, in front
            Fix nearEdge = ahead - Fix.FromMilli(defBody.RadiusMm);
            return ahead > Fix.Zero && nearEdge <= Fix.FromMilli(m.ReachMm);
        }

        void Resolve(
            ref MatchState s,
            ref RobotState a,
            ref RobotState d,
            in FighterDef atkBody,
            in FighterDef defBody,
            in StatusMods atkMods,
            byte side
        )
        {
            var m = _moves[(int)a.Move];
            a.MoveConnected = true;

            bool guarding =
                d.Blocking
                && d.Grounded
                && (
                    d.Mode == RobotMode.Idle
                    || d.Mode == RobotMode.Walk
                    || d.Mode == RobotMode.Blockstun
                );
            // Standing guard stops highs and mids; crouching guard (down-back) stops lows only.
            if (guarding && (m.Level == HitLevel.Low ? d.Crouching : !d.Crouching))
            {
                d.Mode = RobotMode.Blockstun;
                d.Stun = m.Blockstun;
                // Blocking costs the blocking limb a little (PF GDD): the arm on the side the blow comes from, or the legs.
                var limb =
                    m.Level == HitLevel.Low ? Part.Legs
                    : m.UsesLeftArm ? Part.ArmR
                    : Part.ArmL;
                int chip = Math.Max(
                    1,
                    ComputeDamage(m, atkBody, atkMods, d, counter: false, comboIndex: 0)
                        * BlockChipPermille
                        / 1000
                );
                DamagePart(ref s, ref d, limb, chip, side);
                AddOverload(ref s, ref d, m.OverloadFill / 2, defBody, side);
                Push(ref a, ref d, atkBody, defBody, m.PushbackMm * 3 / 2);
                s.Hitstop = Math.Max(s.Hitstop, BlockHitstop);
                s.AddEvent(
                    new SimEvent
                    {
                        Type = SimEventType.Block,
                        Side = side,
                        Move = m.Id,
                        Part = limb,
                    }
                );
                return;
            }

            bool counter = d.Mode == RobotMode.Attack && _moves[(int)d.Move].InStartup(d.MoveFrame);
            int damage = ComputeDamage(m, atkBody, atkMods, d, counter, d.ComboHits);
            d.Hp = Math.Max(0, d.Hp - damage);
            d.ComboHits++;
            d.ComboDamage += damage;
            if (d.Mode == RobotMode.Launched)
            {
                d.JuggleHits++;
                d.JuggleDamage += damage;
            }
            DamagePart(ref s, ref d, m.TargetPart, damage, side);

            var reaction =
                counter && m.CounterReaction != Reaction.None ? m.CounterReaction : m.Reaction;
            bool wasAirborneJump = d.Mode == RobotMode.Jump && d.Phase == 1;
            bool stalled = d.Mode == RobotMode.Overload;
            d.Move = MoveId.None;
            d.MoveFrame = 0;
            d.LinkQueued = false;
            d.Blocking = false;
            d.Crouching = false;
            d.BufFrames = 0;

            if (stalled)
            {
                // OVERLOAD is a stall: hits land with extra damage, the robot stays stuck until it ends.
            }
            else if (d.Mode == RobotMode.Launched)
            {
                d.ModeFrame = Math.Max(0, d.ModeFrame - JugglePopFrames);
            }
            else if (wasAirborneJump)
            {
                d.Mode = RobotMode.Knockdown; // hit out of a jump: falls
                d.Phase = 0;
                d.Y = Fix.Zero;
                d.Stun = KnockdownFrames;
            }
            else
            {
                ApplyReaction(ref d, reaction, m, a.Facing);
            }

            Push(ref a, ref d, atkBody, defBody, m.PushbackMm);
            if (
                AtRopes(d.X, defBody) && (d.Mode == RobotMode.Hitstun || d.Mode == RobotMode.Groggy)
            )
                d.Stun += RopeStunFrames;

            s.Hitstop = Math.Max(s.Hitstop, m.Hitstop);
            s.AddEvent(
                new SimEvent
                {
                    Type = SimEventType.Hit,
                    Side = side,
                    Move = m.Id,
                    Damage = damage,
                    Counter = counter,
                    ComboHits = d.ComboHits,
                    ComboDamage = d.ComboDamage,
                    Part = m.TargetPart,
                }
            );
            if (!stalled && reaction == Reaction.Groggy && d.Mode == RobotMode.Groggy)
                s.AddEvent(new SimEvent { Type = SimEventType.Groggy, Side = side });

            AddOverload(ref s, ref d, m.OverloadFill, defBody, side);
        }

        static void ApplyReaction(
            ref RobotState d,
            Reaction reaction,
            in MoveDef m,
            sbyte attackerFacing
        )
        {
            switch (reaction)
            {
                case Reaction.Heavy:
                    d.Mode = RobotMode.Hitstun;
                    d.Stun = m.Hitstun;
                    d.SlidePerTick =
                        Fix.FromMilli(HeavySlideMm) / HeavySlideFrames * attackerFacing;
                    d.SlideFrames = HeavySlideFrames;
                    break;
                case Reaction.Knockdown:
                    d.Mode = RobotMode.Knockdown;
                    d.Stun = m.Hitstun;
                    break;
                case Reaction.Launch:
                    d.Mode = RobotMode.Launched;
                    d.ModeFrame = 0;
                    d.ModeLength = LaunchFrames;
                    d.JuggleHits = 0;
                    d.JuggleDamage = 0;
                    d.JuggleCap = m.JuggleCap;
                    break;
                case Reaction.Groggy:
                    d.Mode = RobotMode.Groggy;
                    d.Stun = GroggyFrames;
                    break;
                default:
                    d.Mode = RobotMode.Hitstun;
                    d.Stun = m.Hitstun;
                    break;
            }
        }

        /// <summary>OVERLOAD resist 1–5 scales the fill: resist 3 takes 85 %, resist 5 takes 55 % ▲.</summary>
        public static int ResistPermille(int resist) =>
            1300 - 150 * Math.Max(1, Math.Min(5, resist));

        static void AddOverload(
            ref MatchState s,
            ref RobotState d,
            int amount,
            in FighterDef defBody,
            byte side
        )
        {
            if (
                amount <= 0
                || d.Mode == RobotMode.Overload
                || d.Down
                || d.Mode == RobotMode.Launched
            )
                return;
            d.Overload = Math.Min(
                1000,
                d.Overload + amount * ResistPermille(defBody.OverloadResist) / 1000
            );
            d.OverloadQuiet = 0;
            if (d.Overload < 1000)
                return;
            d.Mode = RobotMode.Overload;
            d.Stun = OverloadFrames;
            d.SlideFrames = 0;
            s.AddEvent(new SimEvent { Type = SimEventType.Overload, Side = side });
        }

        /// <summary>Pushes the defender away; at the ropes the attacker takes the rest of the push.</summary>
        static void Push(
            ref RobotState a,
            ref RobotState d,
            in FighterDef atkBody,
            in FighterDef defBody,
            int pushMm
        )
        {
            Fix want = d.X + Fix.FromMilli(pushMm) * a.Facing;
            Fix got = ClampToRing(want, defBody);
            d.X = got;
            a.X = ClampToRing(a.X - (want - got), atkBody);
        }
    }
}
