#nullable enable
namespace RFL.Sim
{
    /// <summary>
    /// Modifiers derived from a robot's condition (PF-DEC-25, PF-REQ-074/075). Base frame data never changes:
    /// statuses may only change damage, distance, speed, drain and add whole recovery frames.
    /// </summary>
    public struct StatusMods
    {
        public int PunchLeftPermille;
        public int PunchRightPermille;
        public int KickPermille;
        public int WalkPermille;
        public int DashPermille;
        public int OverloadDrainPermille;
        public int RecoveryAddLeft;
        public int RecoveryAddRight;
        public bool CanJump;

        public int DamagePermille(in MoveDef m) =>
            m.IsKick ? KickPermille
            : m.UsesLeftArm ? PunchLeftPermille
            : m.UsesRightArm ? PunchRightPermille
            : 1000;

        public int RecoveryAdd(in MoveDef m) =>
            m.UsesLeftArm ? RecoveryAddLeft
            : m.UsesRightArm ? RecoveryAddRight
            : 0;
    }
}
