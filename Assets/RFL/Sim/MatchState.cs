#nullable enable
namespace RFL.Sim
{
    /// <summary>
    /// The whole fight as one unmanaged value type (PF-REQ-072): copying it is a snapshot, assigning it back
    /// is a restore. No classes, arrays or strings inside; static data lives in <see cref="Simulation"/>.
    /// </summary>
    public struct MatchState
    {
        public const int Version = 2;

        public int StateVersion;
        public MatchConfig Config;
        public int Frame;
        public int Hitstop;
        public MatchPhase Phase;
        public int PhaseFrames; // frames left in Intro or RoundOver
        public int Round;
        public int WinsP1;
        public int WinsP2;
        public int TimerTicks;
        public byte RoundWinner; // 0 none, 1, 2, 3 drawn
        public RoundEnd RoundEndReason;
        public byte MatchWinner;
        public RobotState P1;
        public RobotState P2;
        public byte EventCount;
        public SimEvent E0;
        public SimEvent E1;
        public SimEvent E2;
        public SimEvent E3;

        public const int MaxEvents = 4;

        public SimEvent Event(int i) =>
            i switch
            {
                0 => E0,
                1 => E1,
                2 => E2,
                _ => E3,
            };

        public void AddEvent(SimEvent e)
        {
            switch (EventCount)
            {
                case 0:
                    E0 = e;
                    break;
                case 1:
                    E1 = e;
                    break;
                case 2:
                    E2 = e;
                    break;
                case 3:
                    E3 = e;
                    break;
                default:
                    return;
            }
            EventCount++;
        }

        public int TimerSeconds =>
            (TimerTicks + Simulation.TicksPerSecond - 1) / Simulation.TicksPerSecond;

        /// <summary>FNV-1a over every field, for determinism checks and replays.</summary>
        public ulong Hash()
        {
            var h = new StateHasher();
            h.Add(StateVersion);
            h.Add((int)Config.Mode);
            h.Add((int)Config.P1);
            h.Add((int)Config.P2);
            h.Add((int)Config.Cpu);
            h.Add((long)Config.Seed);
            h.Add(Frame);
            h.Add(Hitstop);
            h.Add((int)Phase);
            h.Add(PhaseFrames);
            h.Add(Round);
            h.Add(WinsP1);
            h.Add(WinsP2);
            h.Add(TimerTicks);
            h.Add(RoundWinner);
            h.Add((int)RoundEndReason);
            h.Add(MatchWinner);
            Add(ref h, P1);
            Add(ref h, P2);
            return h.Value;
        }

        static void Add(ref StateHasher h, RobotState r)
        {
            h.Add(r.X.Raw);
            h.Add(r.Y.Raw);
            h.Add(r.Facing);
            h.Add(r.Hp);
            h.Add(r.Stamina);
            h.Add(r.Overload);
            h.Add(r.OverloadQuiet);
            h.Add((int)r.Mode);
            h.Add((int)r.Move);
            h.Add(r.MoveFrame);
            h.Add(r.MoveConnected ? 1 : 0);
            h.Add(r.LinkQueued ? 1 : 0);
            h.Add(r.Stun);
            h.Add(r.ModeFrame);
            h.Add(r.ModeLength);
            h.Add(r.Phase);
            h.Add(r.MoveDir);
            h.Add(r.FromX.Raw);
            h.Add(r.Travel.Raw);
            h.Add(r.SlidePerTick.Raw);
            h.Add(r.SlideFrames);
            h.Add(r.Blocking ? 1 : 0);
            h.Add(r.Crouching ? 1 : 0);
            h.Add(r.PrevButtons);
            h.Add(r.PrevDirs);
            h.Add(r.BufButton);
            h.Add(r.BufFrames);
            h.Add(r.FwdTapFrame);
            h.Add(r.BackTapFrame);
            h.Add(r.UpHeld);
            h.Add(r.PendingDash);
            h.Add(r.ComboHits);
            h.Add(r.ComboDamage);
            h.Add(r.JuggleHits);
            h.Add(r.JuggleDamage);
            h.Add(r.JuggleCap);
            h.Add(r.KodThisRound ? 1 : 0);
            h.Add(r.PartHead);
            h.Add(r.PartTorso);
            h.Add(r.PartArmL);
            h.Add(r.PartArmR);
            h.Add(r.PartLegs);
        }
    }
}
