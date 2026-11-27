#nullable enable
using RFL.Sim;

namespace RFL.Data
{
    /// <summary>
    /// Starter types as typed from the sheet's Starter Types tab (PF GDD table, 2026-11-24).
    /// Replaced by the sheet importer in WP-D13; until then this table is the single copy in code.
    /// </summary>
    public static class StarterTypes
    {
        public const int StatPipMax = 5;

        public static readonly StarterTypeDef[] All =
        {
            new StarterTypeDef
            {
                Type = StarterType.Standard,
                NameKey = "robot.standard",
                Class = WeightClass.Middle,
                Hp = 1550,
                Stamina = 1000,
                Agility = 3,
                Power = 3,
                OverloadResist = 3,
                TechMoveKeys = new[]
                {
                    "move.body_blow_combo",
                    "move.uppercut_combo",
                    "move.ducking_counter",
                    "move.rushing_straight",
                },
            },
            new StarterTypeDef
            {
                Type = StarterType.Power,
                NameKey = "robot.power",
                Class = WeightClass.Heavy,
                Hp = 1708,
                Stamina = 800,
                Agility = 2,
                Power = 5,
                OverloadResist = 4,
                TechMoveKeys = new[]
                {
                    "move.rushing_straight",
                    "move.haymaker",
                    "move.low_kick",
                    "move.clinch_throw",
                },
            },
            new StarterTypeDef
            {
                Type = StarterType.Speed,
                NameKey = "robot.speed",
                Class = WeightClass.Light,
                Hp = 1386,
                Stamina = 1250,
                Agility = 5,
                Power = 2,
                OverloadResist = 2,
                TechMoveKeys = new[]
                {
                    "move.hook_flurry",
                    "move.uppercut_combo",
                    "move.low_kick",
                    "move.ducking_counter",
                },
            },
        };

        public static StarterTypeDef Get(StarterType type) => All[(int)type];

        public static string ClassKey(WeightClass c) =>
            c switch
            {
                WeightClass.Light => "ui.class.light",
                WeightClass.Heavy => "ui.class.heavy",
                _ => "ui.class.middle",
            };
    }
}
