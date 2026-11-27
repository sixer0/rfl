#nullable enable
namespace RFL.Sim
{
    public struct RobotState
    {
        public Fix X;
        public Fix Y;
        public sbyte Facing;
        public int Hp;
        public int Stamina;
        public int Overload;
        public int OverloadQuiet; // ticks since the gauge last rose
        public RobotMode Mode;
        public MoveId Move;
        public int MoveFrame;
        public bool MoveConnected;
        public bool LinkQueued;
        public int Stun; // frames left in a timed mode
        public int ModeFrame; // frames spent in the current timed mode
        public int ModeLength;
        public int Phase; // jump: 0 pre-jump, 1 air, 2 landing
        public sbyte MoveDir; // dash or jump direction: +1 forward, -1 back, 0 straight up
        public Fix FromX;
        public Fix Travel; // signed distance covered over ModeLength (dash, jump)
        public Fix SlidePerTick; // heavy-stagger stumble
        public int SlideFrames;
        public bool Blocking;
        public bool Crouching;
        public byte PrevButtons;
        public byte PrevDirs;
        public byte BufButton;
        public int BufFrames;
        public int FwdTapFrame;
        public int BackTapFrame;
        public int UpHeld;
        public sbyte PendingDash; // +1 dash, -1 backdash requested by a double tap this tick
        public int ComboHits;
        public int ComboDamage;
        public int JuggleHits;
        public int JuggleDamage;
        public int JuggleCap;
        public bool KodThisRound;
        public int PartHead;
        public int PartTorso;
        public int PartArmL;
        public int PartArmR;
        public int PartLegs;

        public bool Airborne =>
            (Mode == RobotMode.Jump && Phase == 1) || Mode == RobotMode.Launched;
        public bool Grounded => !Airborne;
        public bool Down =>
            Mode == RobotMode.Knockdown || Mode == RobotMode.WakeUp || Mode == RobotMode.Ko;

        public int GetPart(Part p) =>
            p switch
            {
                Part.Head => PartHead,
                Part.Torso => PartTorso,
                Part.ArmL => PartArmL,
                Part.ArmR => PartArmR,
                _ => PartLegs,
            };

        public void SetPart(Part p, int hp)
        {
            switch (p)
            {
                case Part.Head:
                    PartHead = hp;
                    break;
                case Part.Torso:
                    PartTorso = hp;
                    break;
                case Part.ArmL:
                    PartArmL = hp;
                    break;
                case Part.ArmR:
                    PartArmR = hp;
                    break;
                default:
                    PartLegs = hp;
                    break;
            }
        }
    }
}
