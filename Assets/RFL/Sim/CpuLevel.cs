#nullable enable
namespace RFL.Sim
{
    /// <summary>CPU levels change reaction time, blocking and combo length, never damage or HP (PF-REQ-016).</summary>
    public enum CpuLevel : byte
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
    }
}
