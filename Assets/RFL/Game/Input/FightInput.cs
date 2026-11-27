#nullable enable
using RFL.Sim;
using UnityEngine.InputSystem;

namespace RFL.Game.Input
{
    /// <summary>
    /// Turns one device into one <see cref="InputFrame"/> per simulation tick. Presses are latched every
    /// rendered frame, so a tap shorter than a tick is never lost. Layout (Xbox names): X left punch,
    /// Y right punch, A left kick, B right kick; keyboard U, I, J, K and WASD or arrows.
    /// </summary>
    public sealed class FightInput
    {
        readonly InputDevice? _device;
        byte _latched;

        public FightInput(InputDevice? device) => _device = device;

        public InputDevice? Device => _device;

        /// <summary>Call once per rendered frame.</summary>
        public void Latch()
        {
            if (_device == null || !_device.added)
                return;
            _latched |= Pressed();
        }

        /// <summary>Call once per simulation tick.</summary>
        public InputFrame Sample()
        {
            if (_device == null || !_device.added)
                return default;
            var f = new InputFrame((byte)(Held() | _latched), Dirs());
            _latched = 0;
            return f;
        }

        byte Held()
        {
            switch (_device)
            {
                case Gamepad g:
                    return Bits(
                        g.buttonWest.isPressed,
                        g.buttonNorth.isPressed,
                        g.buttonSouth.isPressed,
                        g.buttonEast.isPressed
                    );
                case Keyboard k:
                    return Bits(
                        k.uKey.isPressed,
                        k.iKey.isPressed,
                        k.jKey.isPressed,
                        k.kKey.isPressed
                    );
                default:
                    return 0;
            }
        }

        byte Pressed()
        {
            switch (_device)
            {
                case Gamepad g:
                    return Bits(
                        g.buttonWest.wasPressedThisFrame,
                        g.buttonNorth.wasPressedThisFrame,
                        g.buttonSouth.wasPressedThisFrame,
                        g.buttonEast.wasPressedThisFrame
                    );
                case Keyboard k:
                    return Bits(
                        k.uKey.wasPressedThisFrame,
                        k.iKey.wasPressedThisFrame,
                        k.jKey.wasPressedThisFrame,
                        k.kKey.wasPressedThisFrame
                    );
                default:
                    return 0;
            }
        }

        byte Dirs()
        {
            bool left,
                right,
                up,
                down;
            switch (_device)
            {
                case Gamepad g:
                {
                    var s = g.leftStick.ReadValue();
                    var d = g.dpad.ReadValue();
                    left = d.x < -0.5f || s.x < -0.5f;
                    right = d.x > 0.5f || s.x > 0.5f;
                    up = d.y > 0.5f || s.y > 0.5f;
                    down = d.y < -0.5f || s.y < -0.5f;
                    break;
                }
                case Keyboard k:
                    left = k.aKey.isPressed || k.leftArrowKey.isPressed;
                    right = k.dKey.isPressed || k.rightArrowKey.isPressed;
                    up = k.wKey.isPressed || k.upArrowKey.isPressed;
                    down = k.sKey.isPressed || k.downArrowKey.isPressed;
                    break;
                default:
                    return 0;
            }
            byte b = 0;
            if (left && !right)
                b |= InputFrame.Left;
            if (right && !left)
                b |= InputFrame.Right;
            if (up && !down)
                b |= InputFrame.Up;
            if (down && !up)
                b |= InputFrame.Down;
            return b;
        }

        static byte Bits(bool lp, bool rp, bool lk, bool rk) =>
            (byte)(
                (lp ? InputFrame.LeftPunch : 0)
                | (rp ? InputFrame.RightPunch : 0)
                | (lk ? InputFrame.LeftKick : 0)
                | (rk ? InputFrame.RightKick : 0)
            );
    }
}
