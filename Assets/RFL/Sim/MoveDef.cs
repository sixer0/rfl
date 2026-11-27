#nullable enable
namespace RFL.Sim
{
    /// <summary>
    /// Frame data and effects for one move (Tech Spec, Frame data). Active frames are Startup+1 .. Startup+Active,
    /// so the clip is exactly Startup + Active + Recovery frames. A string continues with <see cref="LinkMove"/>
    /// when <see cref="LinkButton"/> is pressed before <see cref="LinkUntil"/>. Integers only (PF-REQ-071).
    /// </summary>
    public struct MoveDef
    {
        public MoveId Id;
        public string NameKey;
        public byte StartButton;
        public HitLevel Level;
        public int Startup;
        public int Active;
        public int Recovery;
        public int Damage;
        public int ReachMm;
        public int Hitstun;
        public int Blockstun;
        public int Hitstop;
        public int PushbackMm;
        public Reaction Reaction;
        public Reaction CounterReaction; // None = same as Reaction
        public Part TargetPart;
        public int OverloadFill; // gauge points (0–1000) before the defender's resist
        public int JuggleCap; // launchers only: most damage the follow-up juggle may add
        public MoveId LinkMove;
        public byte LinkButton;
        public int LinkUntil;

        public int Total => Startup + Active + Recovery;
        public int LinkStart => Startup + Active + 1;

        public bool IsActive(int frame) => frame > Startup && frame <= Startup + Active;

        public bool InStartup(int frame) => frame <= Startup;

        public bool IsKick =>
            StartButton == InputFrame.LeftKick || StartButton == InputFrame.RightKick;
        public bool UsesLeftArm => Id == MoveId.Jab || Id == MoveId.Uppercut;
        public bool UsesRightArm => Id == MoveId.Cross || Id == MoveId.Straight;
    }
}
