#nullable enable
using RFL.Sim;

namespace RFL.Data
{
    /// <summary>One demo starter type. Names and moves are string keys, never display text.</summary>
    public sealed class StarterTypeDef
    {
        public StarterType Type;
        public string NameKey = null!;
        public WeightClass Class;
        public int Hp;
        public int Stamina;
        public int Agility;
        public int Power;
        public int OverloadResist;
        public string[] TechMoveKeys = null!;
    }
}
