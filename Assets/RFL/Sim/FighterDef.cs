#nullable enable
namespace RFL.Sim
{
    /// <summary>What the simulation needs to know about a robot's body (from the sheet via RFL.Data).</summary>
    public struct FighterDef
    {
        public int MaxHp;
        public int MaxStamina;
        public int PowerPermille;
        public int OverloadResist; // 1–5 stat
        public int WalkMmPerSecond;
        public int RadiusMm;
        public int DashMm;
        public int DashFrames;
        public int BackdashMm;
        public int BackdashFrames;
        public int PreJumpFrames;
        public int AirFrames;
        public int JumpPeakMm;
        public int JumpMm;
        public int LandingFrames;

        /// <summary>Each part's HP: 30 % of max HP ▲.</summary>
        public int PartMax => MaxHp * 300 / 1000;
    }
}
