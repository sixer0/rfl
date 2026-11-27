#nullable enable
namespace RFL.Sim
{
    /// <summary>
    /// One player's input for one tick (Tech Spec, Input). Humans and the CPU produce the same frames,
    /// so the simulation cannot tell them apart. Directions are screen-relative; the simulation turns
    /// them into forward and back from each robot's facing.
    /// </summary>
    public struct InputFrame
    {
        // Limb buttons (Xbox names): X left punch, Y right punch, A left kick, B right kick.
        public const byte LeftPunch = 1;
        public const byte RightPunch = 2;
        public const byte LeftKick = 4;
        public const byte RightKick = 8;

        public const byte Left = 1;
        public const byte Right = 2;
        public const byte Up = 4;
        public const byte Down = 8;

        public byte Buttons;
        public byte Dirs;

        public InputFrame(byte buttons, byte dirs)
        {
            Buttons = buttons;
            Dirs = dirs;
        }

        public bool Has(byte button) => (Buttons & button) != 0;

        public bool Holds(byte dir) => (Dirs & dir) != 0;
    }
}
