#nullable enable
namespace RFL.Sim
{
    public enum SimEventType : byte
    {
        None = 0,
        Hit = 1,
        Block = 2,
        Ko = 3,
        PartBroken = 4,
        Overload = 5,
        Groggy = 6,
        Dash = 7,
        Jump = 8,
        Land = 9,
        RoundOver = 10,
        MatchOver = 11,
        Fight = 12,
    }
}
