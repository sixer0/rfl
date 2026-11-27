#nullable enable
namespace RFL.Sim
{
    /// <summary>Something the view should show or play this tick. Side: the attacker (or the winner, or the mover).</summary>
    public struct SimEvent
    {
        public SimEventType Type;
        public byte Side;
        public MoveId Move;
        public int Damage;
        public int ComboHits;
        public int ComboDamage;
        public bool Counter;
        public Part Part;
        public RoundEnd Reason;
    }
}
