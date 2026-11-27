#nullable enable
using System;

namespace RFL.Sim
{
    /// <summary>
    /// Damage and part damage. Every step is an integer per-mille multiply, floored, in this order:
    /// base × Power × combo scale × counter × OVERLOAD taken × attacker's limb status × juggle, capped by the
    /// launcher's juggle cap. Parts take the same amount; at most two parts can be broken (PF-REQ-060), and parts
    /// never end a round on their own (PF-REQ-061).
    /// </summary>
    public sealed partial class Simulation
    {
        /// <summary>Damage of the n-th hit in a combo, per mille (PF-REQ-010) ▲.</summary>
        static readonly int[] ComboScale = { 1000, 850, 700, 550, 400 };

        public const int CounterPermille = 1200; // ▲
        public const int OverloadTakenPermille = 1250; // ▲
        public const int JugglePermille = 700; // ▲

        public static int ScaleFor(int comboIndex) =>
            ComboScale[Math.Min(comboIndex, ComboScale.Length - 1)];

        public static int ComputeDamage(
            in MoveDef m,
            in FighterDef attacker,
            in StatusMods attackerMods,
            in RobotState defender,
            bool counter,
            int comboIndex
        )
        {
            long v = m.Damage;
            v = v * attacker.PowerPermille / 1000;
            v = v * ScaleFor(comboIndex) / 1000;
            if (counter)
                v = v * CounterPermille / 1000;
            if (defender.Mode == RobotMode.Overload)
                v = v * OverloadTakenPermille / 1000;
            v = v * attackerMods.DamagePermille(m) / 1000;
            if (defender.Mode == RobotMode.Launched)
            {
                v = v * JugglePermille / 1000;
                v = Math.Min(v, Math.Max(0, defender.JuggleCap - defender.JuggleDamage));
            }
            return (int)Math.Max(0, v);
        }

        static void DamagePart(
            ref MatchState s,
            ref RobotState d,
            Part part,
            int amount,
            byte attackerSide
        )
        {
            int hp = d.GetPart(part);
            if (hp <= 0 || amount <= 0)
                return;
            int next = hp - amount;
            if (next <= 0)
            {
                if (StatusRules.BrokenCount(d) >= MaxBroken)
                {
                    next = 1; // a third part stops one step short of broken
                }
                else
                {
                    next = 0;
                    s.AddEvent(
                        new SimEvent
                        {
                            Type = SimEventType.PartBroken,
                            Side = attackerSide,
                            Part = part,
                        }
                    );
                }
            }
            d.SetPart(part, next);
        }
    }
}
