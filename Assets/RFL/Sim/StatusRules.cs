#nullable enable
namespace RFL.Sim
{
    public static class StatusRules
    {
        public const int BrokenLimbPermille = 750; // broken arm or legs: −25 % ▲
        public const int BrokenArmRecovery = 2; // +2 recovery frames ▲
        public const int BrokenLegsWalkPermille = 800; // −20 % ▲
        public const int BrokenHeadDrainPermille = 500; // OVERLOAD drains at half speed
        public const int LowStaminaPermille = 200; // below 20 % of max ▲
        public const int LowStaminaDashPermille = 750;

        public static PartState StateOf(int hp, int max) =>
            hp <= 0 ? PartState.Broken
            : hp * 2 <= max ? PartState.Damaged
            : PartState.Ok;

        public static PartState StateOf(in RobotState r, Part p, in FighterDef f) =>
            StateOf(r.GetPart(p), f.PartMax);

        public static int BrokenCount(in RobotState r)
        {
            int n = 0;
            if (r.PartHead <= 0)
                n++;
            if (r.PartTorso <= 0)
                n++;
            if (r.PartArmL <= 0)
                n++;
            if (r.PartArmR <= 0)
                n++;
            if (r.PartLegs <= 0)
                n++;
            return n;
        }

        public static bool LowStamina(in RobotState r, in FighterDef f) =>
            r.Stamina * 1000 < f.MaxStamina * LowStaminaPermille;

        public static StatusMods For(in RobotState r, in FighterDef f)
        {
            var m = new StatusMods
            {
                PunchLeftPermille = 1000,
                PunchRightPermille = 1000,
                KickPermille = 1000,
                WalkPermille = 1000,
                DashPermille = 1000,
                OverloadDrainPermille = 1000,
                CanJump = true,
            };
            if (r.PartArmL <= 0)
            {
                m.PunchLeftPermille = BrokenLimbPermille;
                m.RecoveryAddLeft = BrokenArmRecovery;
            }
            if (r.PartArmR <= 0)
            {
                m.PunchRightPermille = BrokenLimbPermille;
                m.RecoveryAddRight = BrokenArmRecovery;
            }
            if (r.PartLegs <= 0)
            {
                m.KickPermille = BrokenLimbPermille;
                m.WalkPermille = BrokenLegsWalkPermille;
                m.DashPermille = BrokenLimbPermille;
            }
            if (r.PartHead <= 0)
                m.OverloadDrainPermille = BrokenHeadDrainPermille;
            if (LowStamina(r, f))
            {
                m.DashPermille = m.DashPermille * LowStaminaDashPermille / 1000;
                m.CanJump = false;
            }
            return m;
        }
    }
}
