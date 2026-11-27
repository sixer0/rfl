#nullable enable
namespace RFL.Sim
{
    public enum RobotMode : byte
    {
        Idle = 0,
        Walk = 1,
        Attack = 2,
        Hitstun = 3,
        Blockstun = 4,
        Knockdown = 5,
        Ko = 6,
        Dash = 7,
        Backdash = 8,
        Jump = 9,
        Groggy = 10,
        Overload = 11,
        Launched = 12,
        WakeUp = 13,
    }
}
