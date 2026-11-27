#nullable enable
namespace RFL.Sim
{
    /// <summary>Controls and movement: buttons, strings, walk, crouch, dash, backdash, jump, stamina.</summary>
    public sealed partial class Simulation
    {
        public const int DashCost = 50; // stamina ▲ (PF sheet #51: movement costs, normal attacks are free)
        public const int BackdashCost = 50;
        public const int JumpCost = 60;
        public const int BackWalkPermille = 800;
        public const int WakeUpFrames = 20;
        public const int KnockdownFrames = 40;
        public const int LaunchFrames = 44;
        public const int LaunchPeakMm = 1200;
        public const int OverloadQuietFrames = 60;
        public const int OverloadDrainPerTick = 4;

        /// <summary>Edge-detects buttons and direction taps. A press that continues the current string is queued
        /// at once; any other press waits in the 8-frame buffer. Two forward (back) taps within 12 frames request
        /// a dash (backdash).</summary>
        void ReadInput(ref MatchState s, ref RobotState r, InputFrame input)
        {
            byte pressed = (byte)(input.Buttons & ~r.PrevButtons);
            r.PrevButtons = input.Buttons;
            if (pressed != 0)
            {
                byte b =
                    (pressed & InputFrame.LeftPunch) != 0 ? InputFrame.LeftPunch
                    : (pressed & InputFrame.RightPunch) != 0 ? InputFrame.RightPunch
                    : (pressed & InputFrame.LeftKick) != 0 ? InputFrame.LeftKick
                    : InputFrame.RightKick;
                bool queued = false;
                if (r.Mode == RobotMode.Attack && !r.LinkQueued)
                {
                    var m = _moves[(int)r.Move];
                    if (
                        m.LinkMove != MoveId.None
                        && b == m.LinkButton
                        && r.MoveFrame <= m.LinkUntil
                    )
                        queued = r.LinkQueued = true;
                }
                if (!queued)
                {
                    r.BufButton = b;
                    r.BufFrames = BufferFrames;
                }
            }

            byte fwd = r.Facing > 0 ? InputFrame.Right : InputFrame.Left;
            byte back = r.Facing > 0 ? InputFrame.Left : InputFrame.Right;
            byte tapped = (byte)(input.Dirs & ~r.PrevDirs);
            r.PrevDirs = input.Dirs;
            if ((tapped & fwd) != 0)
            {
                if (s.Frame - r.FwdTapFrame <= DoubleTapFrames)
                {
                    r.PendingDash = 1;
                    r.FwdTapFrame = -1000;
                }
                else
                {
                    r.FwdTapFrame = s.Frame;
                }
            }
            if ((tapped & back) != 0)
            {
                if (s.Frame - r.BackTapFrame <= DoubleTapFrames)
                {
                    r.PendingDash = -1;
                    r.BackTapFrame = -1000;
                }
                else
                {
                    r.BackTapFrame = s.Frame;
                }
            }
            r.UpHeld = input.Holds(InputFrame.Up) ? r.UpHeld + 1 : 0;
        }

        /// <summary>Parabola height in metres at frame f of n, peaking at peakMm.</summary>
        static Fix Arc(int peakMm, int f, int n) =>
            f <= 0 || f >= n ? Fix.Zero : Fix.FromRatio(4L * peakMm * f * (n - f), 1000L * n * n);

        void Act(
            ref MatchState s,
            ref RobotState r,
            InputFrame input,
            in FighterDef body,
            in StatusMods mods,
            int side
        )
        {
            if (r.SlideFrames > 0)
            {
                r.X = ClampToRing(r.X + r.SlidePerTick, body);
                r.SlideFrames--;
            }

            switch (r.Mode)
            {
                case RobotMode.Ko:
                    return;
                case RobotMode.Hitstun:
                case RobotMode.Blockstun:
                case RobotMode.Groggy:
                    if (--r.Stun > 0)
                        return;
                    EndStun(ref r);
                    break;
                case RobotMode.Overload:
                    if (--r.Stun > 0)
                        return;
                    r.Overload = 0; // the stall empties the gauge
                    EndStun(ref r);
                    break;
                case RobotMode.Knockdown:
                    if (--r.Stun > 0)
                        return;
                    if (r.Hp <= 0)
                    {
                        r.Mode = RobotMode.Ko; // a KO'd robot stays down
                        return;
                    }
                    r.Mode = RobotMode.WakeUp;
                    r.Stun = WakeUpFrames;
                    r.ModeLength = WakeUpFrames;
                    return;
                case RobotMode.WakeUp:
                    if (--r.Stun > 0)
                        return;
                    EndStun(ref r);
                    break;
                case RobotMode.Launched:
                    r.ModeFrame++;
                    r.Y = Arc(LaunchPeakMm, r.ModeFrame, r.ModeLength);
                    if (r.ModeFrame < r.ModeLength)
                        return;
                    r.Y = Fix.Zero;
                    r.Mode = r.Hp <= 0 ? RobotMode.Ko : RobotMode.Knockdown;
                    r.Stun = KnockdownFrames;
                    return;
                case RobotMode.Dash:
                case RobotMode.Backdash:
                    r.ModeFrame++;
                    r.X = ClampToRing(r.FromX + r.Travel * r.ModeFrame / r.ModeLength, body);
                    if (r.ModeFrame < r.ModeLength)
                        return;
                    r.Mode = RobotMode.Idle;
                    break;
                case RobotMode.Jump:
                    r.ModeFrame++;
                    if (r.Phase == 0)
                    {
                        if (r.ModeFrame < body.PreJumpFrames)
                            return;
                        r.Phase = 1;
                        r.ModeFrame = 0;
                        r.FromX = r.X;
                        return;
                    }
                    if (r.Phase == 1)
                    {
                        r.Y = Arc(body.JumpPeakMm, r.ModeFrame, body.AirFrames);
                        r.X = ClampToRing(r.FromX + r.Travel * r.ModeFrame / body.AirFrames, body);
                        if (r.ModeFrame < body.AirFrames)
                            return;
                        r.Phase = 2;
                        r.ModeFrame = 0;
                        r.Y = Fix.Zero;
                        s.AddEvent(new SimEvent { Type = SimEventType.Land, Side = (byte)side });
                        return;
                    }
                    if (r.ModeFrame < body.LandingFrames)
                        return;
                    r.Mode = RobotMode.Idle;
                    r.Phase = 0;
                    break;
                case RobotMode.Attack:
                {
                    r.MoveFrame++;
                    var m = _moves[(int)r.Move];
                    if (
                        r.BufFrames > 0
                        && m.LinkMove != MoveId.None
                        && r.BufButton == m.LinkButton
                        && r.MoveFrame <= m.LinkUntil
                    )
                    {
                        r.LinkQueued = true;
                        r.BufFrames = 0;
                    }
                    if (r.LinkQueued && r.MoveFrame >= m.LinkStart)
                    {
                        Start(ref r, m.LinkMove);
                        return;
                    }
                    // A broken limb adds whole recovery frames, shown as a hold; startup and active never change.
                    if (r.MoveFrame <= m.Total + mods.RecoveryAdd(m))
                        return;
                    r.Mode = RobotMode.Idle;
                    r.Move = MoveId.None;
                    break;
                }
            }

            Neutral(ref s, ref r, input, body, mods, side);
        }

        void Neutral(
            ref MatchState s,
            ref RobotState r,
            InputFrame input,
            in FighterDef body,
            in StatusMods mods,
            int side
        )
        {
            if (r.BufFrames > 0)
            {
                var id = StarterFor(r.BufButton);
                if (id != MoveId.None)
                {
                    r.BufFrames = 0;
                    Start(ref r, id);
                    return;
                }
            }

            byte forward = r.Facing > 0 ? InputFrame.Right : InputFrame.Left;
            byte back = r.Facing > 0 ? InputFrame.Left : InputFrame.Right;

            if (r.PendingDash != 0)
            {
                bool dash = r.PendingDash > 0;
                int cost = dash ? DashCost : BackdashCost;
                if (r.Stamina >= cost)
                {
                    r.Stamina -= cost;
                    int mm = (dash ? body.DashMm : -body.BackdashMm) * mods.DashPermille / 1000;
                    r.Mode = dash ? RobotMode.Dash : RobotMode.Backdash;
                    r.ModeFrame = 0;
                    r.ModeLength = dash ? body.DashFrames : body.BackdashFrames;
                    r.FromX = r.X;
                    r.Travel = Fix.FromMilli(mm) * r.Facing;
                    r.Blocking = false;
                    r.Crouching = false;
                    s.AddEvent(new SimEvent { Type = SimEventType.Dash, Side = (byte)side });
                    return;
                }
            }

            if (r.UpHeld > TapWindowFrames && mods.CanJump && r.Stamina >= JumpCost)
            {
                r.Stamina -= JumpCost;
                r.UpHeld = 0;
                r.MoveDir = (sbyte)(
                    input.Holds(forward) ? 1
                    : input.Holds(back) ? -1
                    : 0
                );
                r.Mode = RobotMode.Jump;
                r.Phase = 0;
                r.ModeFrame = 0;
                r.Travel = Fix.FromMilli(body.JumpMm) * (r.MoveDir * r.Facing);
                r.Blocking = false;
                r.Crouching = false;
                s.AddEvent(new SimEvent { Type = SimEventType.Jump, Side = (byte)side });
                return;
            }

            r.Blocking = input.Holds(back);
            r.Crouching = input.Holds(InputFrame.Down);
            if (r.Crouching)
            {
                r.Mode = RobotMode.Idle; // crouching robots do not walk
            }
            else if (input.Holds(forward))
            {
                r.Mode = RobotMode.Walk;
                r.X +=
                    Fix.FromRatio(
                        (long)body.WalkMmPerSecond * mods.WalkPermille,
                        TicksPerSecond * 1000L * 1000L
                    ) * r.Facing;
            }
            else if (input.Holds(back))
            {
                r.Mode = RobotMode.Walk;
                r.X -=
                    Fix.FromRatio(
                        (long)body.WalkMmPerSecond * mods.WalkPermille * BackWalkPermille,
                        TicksPerSecond * 1000L * 1000L * 1000L
                    ) * r.Facing;
            }
            else
            {
                r.Mode = RobotMode.Idle;
            }
            r.X = ClampToRing(r.X, body);
        }

        MoveId StarterFor(byte button)
        {
            for (int i = 1; i < _moves.Length; i++)
            {
                if (_moves[i].StartButton == button)
                    return _moves[i].Id;
            }
            return MoveId.None;
        }

        static void Start(ref RobotState r, MoveId id)
        {
            r.Mode = RobotMode.Attack;
            r.Move = id;
            r.MoveFrame = 1;
            r.MoveConnected = false;
            r.LinkQueued = false;
            r.Blocking = false;
            r.Crouching = false;
        }

        static void EndStun(ref RobotState r)
        {
            r.Mode = RobotMode.Idle;
            r.Stun = 0;
            r.Y = Fix.Zero;
            r.ComboHits = 0;
            r.ComboDamage = 0;
            r.JuggleHits = 0;
            r.JuggleDamage = 0;
            r.JuggleCap = 0;
        }

        /// <summary>End of tick: stamina regenerates (faster while guarding), OVERLOAD drains when quiet, buffers age.</summary>
        static void Recover(ref RobotState r, in FighterDef body, in StatusMods mods)
        {
            r.PendingDash = 0;
            if (r.BufFrames > 0)
                r.BufFrames--;
            bool moving =
                r.Mode == RobotMode.Dash
                || r.Mode == RobotMode.Backdash
                || r.Mode == RobotMode.Jump;
            if (!moving && r.Mode != RobotMode.Ko)
            {
                int regen = System.Math.Max(1, body.MaxStamina / 300);
                if (r.Blocking || r.Mode == RobotMode.Blockstun)
                    regen *= 2;
                r.Stamina = System.Math.Min(body.MaxStamina, r.Stamina + regen);
            }
            if (r.Mode != RobotMode.Overload && r.Overload > 0)
            {
                if (++r.OverloadQuiet > OverloadQuietFrames)
                    r.Overload = System.Math.Max(
                        0,
                        r.Overload
                            - System.Math.Max(
                                1,
                                OverloadDrainPerTick * mods.OverloadDrainPermille / 1000
                            )
                    );
            }
        }
    }
}
