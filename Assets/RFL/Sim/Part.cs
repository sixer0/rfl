#nullable enable
namespace RFL.Sim
{
    /// <summary>The five parts (PF GDD, Part damage). Order matches the art pieces _Head, _Torso, _ArmL, _ArmR, _Legs.</summary>
    public enum Part : byte
    {
        Head = 0,
        Torso = 1,
        ArmL = 2,
        ArmR = 3,
        Legs = 4,
        Count = 5,
    }
}
