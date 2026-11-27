#nullable enable
namespace RFL.Sim
{
    /// <summary>High attacks miss a crouching robot; lows must be blocked crouching (hold down-back) and
    /// miss an airborne robot; mids hit a crouching blocker.</summary>
    public enum HitLevel : byte
    {
        High = 0,
        Mid = 1,
        Low = 2,
    }
}
