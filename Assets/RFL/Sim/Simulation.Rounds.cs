#nullable enable
using System;

namespace RFL.Sim
{
    /// <summary>
    /// Rounds and winners (PF sheet PF sheet #48, PF sheet #50; PF-REQ-011/012/063/064): best of 5, 90-second rounds, KO or the larger
    /// HP share at time-out. HP, parts and stamina carry over; between rounds a corner break empties OVERLOAD,
    /// returns 10 % of lost HP, repairs one part by one step, and lifts a KO'd robot to at least 25 % HP.
    /// </summary>
    public sealed partial class Simulation
    {
        public const int RoundSeconds = 90;
        public const int WinsNeeded = 3;
        public const int MaxRounds = 9; // drawn rounds are replayed; this is a safety stop
        public const int IntroFrames = 96; // "ROUND n" 1.0 s, then "FIGHT!" 0.6 s
        public const int FightBannerFrames = 36;
        public const int RoundOverFrames = 150;
        public const int CornerBreakHpPermille = 100; // 10 % of lost HP returns
        public const int KoFloorPermille = 250; // at least 25 % ▲

        void StartRound(ref MatchState s)
        {
            int half = StartDistanceMm / 2;
            ResetForRound(ref s.P1, Fix.FromMilli(-half), 1);
            ResetForRound(ref s.P2, Fix.FromMilli(half), -1);
            s.Phase = MatchPhase.Intro;
            s.PhaseFrames = IntroFrames;
            s.TimerTicks = RoundSeconds * TicksPerSecond;
            s.RoundWinner = 0;
            s.RoundEndReason = RoundEnd.None;
            s.Hitstop = 0;
        }

        static void ResetForRound(ref RobotState r, Fix x, sbyte facing)
        {
            var keep = r;
            r = new RobotState
            {
                X = x,
                Facing = facing,
                Hp = keep.Hp,
                Stamina = keep.Stamina,
                Overload = keep.Overload,
                PartHead = keep.PartHead,
                PartTorso = keep.PartTorso,
                PartArmL = keep.PartArmL,
                PartArmR = keep.PartArmR,
                PartLegs = keep.PartLegs,
                KodThisRound = keep.KodThisRound,
                FwdTapFrame = -1000,
                BackTapFrame = -1000,
            };
        }

        void TickLocked(ref MatchState s)
        {
            switch (s.Phase)
            {
                case MatchPhase.Intro:
                    if (--s.PhaseFrames > 0)
                        return;
                    s.Phase = MatchPhase.Fight;
                    s.AddEvent(new SimEvent { Type = SimEventType.Fight });
                    return;
                case MatchPhase.RoundOver:
                {
                    // Let knocked-down robots finish falling; nobody can act.
                    var m1 = StatusRules.For(s.P1, _f1);
                    var m2 = StatusRules.For(s.P2, _f2);
                    if (s.P1.Mode != RobotMode.Ko && s.P1.Mode != RobotMode.Idle)
                        Act(ref s, ref s.P1, default, _f1, m1, 1);
                    if (s.P2.Mode != RobotMode.Ko && s.P2.Mode != RobotMode.Idle)
                        Act(ref s, ref s.P2, default, _f2, m2, 2);
                    if (--s.PhaseFrames > 0)
                        return;
                    if (s.WinsP1 >= WinsNeeded || s.WinsP2 >= WinsNeeded || s.Round >= MaxRounds)
                    {
                        s.Phase = MatchPhase.MatchOver;
                        s.MatchWinner = (byte)(
                            s.WinsP1 > s.WinsP2 ? 1
                            : s.WinsP2 > s.WinsP1 ? 2
                            : 3
                        );
                        s.AddEvent(
                            new SimEvent { Type = SimEventType.MatchOver, Side = s.MatchWinner }
                        );
                    }
                    else
                    {
                        s.Phase = MatchPhase.CornerBreak;
                    }
                    return;
                }
            }
        }

        void CheckKo(ref MatchState s)
        {
            bool ko1 = s.P1.Hp <= 0;
            bool ko2 = s.P2.Hp <= 0;
            if (!ko1 && !ko2)
                return;
            if (ko1)
                KnockOut(ref s.P1);
            if (ko2)
                KnockOut(ref s.P2);
            byte winner = (byte)(
                ko1 && ko2 ? 3
                : ko2 ? 1
                : 2
            );
            s.AddEvent(new SimEvent { Type = SimEventType.Ko, Side = winner });
            EndRound(ref s, winner, RoundEnd.Ko);
        }

        static void KnockOut(ref RobotState r)
        {
            r.Hp = 0;
            r.KodThisRound = true;
            if (r.Mode != RobotMode.Launched)
            {
                r.Mode = RobotMode.Ko;
                r.Y = Fix.Zero;
            }
        }

        /// <summary>
        /// Time-out: the larger share of max HP wins. Equal shares use PF-DEC-10 option A (provisional): fewer broken
        /// parts, then the larger stamina share, then a drawn round that is replayed.
        /// </summary>
        void EndRoundOnTime(ref MatchState s)
        {
            long hp1 = (long)s.P1.Hp * _f2.MaxHp;
            long hp2 = (long)s.P2.Hp * _f1.MaxHp;
            byte winner;
            if (hp1 != hp2)
                winner = (byte)(hp1 > hp2 ? 1 : 2);
            else
            {
                int b1 = StatusRules.BrokenCount(s.P1),
                    b2 = StatusRules.BrokenCount(s.P2);
                long st1 = (long)s.P1.Stamina * _f2.MaxStamina,
                    st2 = (long)s.P2.Stamina * _f1.MaxStamina;
                winner =
                    b1 != b2 ? (byte)(b1 < b2 ? 1 : 2)
                    : st1 != st2 ? (byte)(st1 > st2 ? 1 : 2)
                    : (byte)3;
            }
            EndRound(ref s, winner, RoundEnd.Time);
        }

        static void EndRound(ref MatchState s, byte winner, RoundEnd reason)
        {
            s.Phase = MatchPhase.RoundOver;
            s.PhaseFrames = RoundOverFrames;
            s.RoundWinner = winner;
            s.RoundEndReason = reason;
            s.Hitstop = 0;
            if (winner == 1)
                s.WinsP1++;
            if (winner == 2)
                s.WinsP2++;
            s.AddEvent(
                new SimEvent
                {
                    Type = SimEventType.RoundOver,
                    Side = winner,
                    Reason = reason,
                }
            );
        }

        /// <summary>
        /// The between-round command. Each side repairs one part by one step (broken → damaged → OK); the game
        /// records the choice with the replay, so the match stays deterministic.
        /// </summary>
        public void ApplyCornerBreak(ref MatchState s, Part p1Repair, Part p2Repair)
        {
            if (s.Phase != MatchPhase.CornerBreak)
                throw new InvalidOperationException("Corner break only between rounds.");
            Break(ref s.P1, _f1, p1Repair);
            Break(ref s.P2, _f2, p2Repair);
            s.Round++;
            StartRound(ref s);
        }

        static void Break(ref RobotState r, in FighterDef f, Part repair)
        {
            r.Overload = 0;
            r.OverloadQuiet = 0;
            r.Hp = Math.Min(f.MaxHp, r.Hp + (f.MaxHp - r.Hp) * CornerBreakHpPermille / 1000);
            if (r.KodThisRound)
                r.Hp = Math.Max(r.Hp, f.MaxHp * KoFloorPermille / 1000);
            r.KodThisRound = false;
            int max = f.PartMax;
            switch (StatusRules.StateOf(r.GetPart(repair), max))
            {
                case PartState.Broken:
                    r.SetPart(repair, max / 2);
                    break;
                case PartState.Damaged:
                    r.SetPart(repair, max);
                    break;
            }
        }

        /// <summary>The idle robot's choice (and the default for the player): the part with the lowest HP.</summary>
        public static Part MostDamagedPart(in RobotState r, in FighterDef f)
        {
            var best = Part.Head;
            int lowest = int.MaxValue;
            for (int i = 0; i < (int)Part.Count; i++)
            {
                int hp = r.GetPart((Part)i);
                if (hp < lowest)
                {
                    lowest = hp;
                    best = (Part)i;
                }
            }
            return best;
        }
    }
}
