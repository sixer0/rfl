#nullable enable
namespace RFL.Sim
{
    /// <summary>What a clean hit does to the defender.</summary>
    public enum Reaction : byte
    {
        None = 0,
        Light = 1, // short hitstun, small push
        Heavy = 2, // long hitstun, stumble back
        Knockdown = 3, // falls, lies down, wakes up
        Launch = 4, // thrown into the air; up to 2 juggle hits, then knockdown
        Groggy = 5, // counter-hit stun: cannot act, free to hit (PF GDD)
    }
}
