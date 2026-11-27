#nullable enable
namespace RFL.Sim
{
    /// <summary>
    /// Everything the simulation needs to start a match. Plain value type so it can sit inside MatchState
    /// and be recorded with a replay (PF-REQ-072).
    /// </summary>
    public struct MatchConfig
    {
        public MatchMode Mode;
        public StarterType P1;
        public StarterType P2;
        public CpuLevel Cpu;
        public uint Seed;

        public static MatchConfig Default =>
            new MatchConfig
            {
                Mode = MatchMode.VersusCpu,
                P1 = StarterType.Standard,
                P2 = StarterType.Standard,
                Cpu = CpuLevel.Normal,
                Seed = 1,
            };
    }
}
