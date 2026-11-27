#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace RFL.Game.Input
{
    /// <summary>
    /// Reads every keyboard and gamepad separately each frame, so robot select can tell players apart,
    /// and merges them for ordinary menus. Menus never rely on UI Toolkit focus navigation.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class MenuInput : MonoBehaviour
    {
        public const float RepeatDelay = 0.35f;
        public const float RepeatRate = 0.11f;

        struct Repeat
        {
            public int X,
                Y;
            public float NextX,
                NextY;
        }

        public static MenuInput Instance { get; private set; } = null!;
        public readonly List<NavFrame> Frames = new List<NavFrame>();
        public DeviceKind LastKind { get; private set; } = DeviceKind.Keyboard;

        readonly Dictionary<int, Repeat> _repeats = new Dictionary<int, Repeat>();
        readonly List<NavFrame> _injected = new List<NavFrame>();

        void Awake() => Instance = this;

        /// <summary>Adds a synthetic frame for the next update (screenshot runner and tests).</summary>
        public void Inject(NavFrame frame) => _injected.Add(frame);

        public NavFrame Merged()
        {
            var m = new NavFrame();
            foreach (var f in Frames)
            {
                if (m.X == 0)
                    m.X = f.X;
                if (m.Y == 0)
                    m.Y = f.Y;
                m.Submit |= f.Submit;
                m.Cancel |= f.Cancel;
                m.TabPrev |= f.TabPrev;
                m.TabNext |= f.TabNext;
                m.Start |= f.Start;
                m.Any |= f.Any;
                if (m.Device == null)
                    m.Device = f.Device;
            }
            return m;
        }

        void Update()
        {
            Frames.Clear();
            float now = Time.unscaledTime;
            var devices = InputSystem.devices;
            for (int i = 0; i < devices.Count; i++)
            {
                var d = devices[i];
                if (!d.enabled || !d.added)
                    continue;
                NavFrame f;
                if (d is Gamepad g)
                    f = ReadPad(g);
                else if (d is Keyboard k)
                    f = ReadKeys(k);
                else
                    continue;
                ApplyRepeat(d.deviceId, ref f, now);
                if (f.HasAction || f.Any)
                {
                    Frames.Add(f);
                    LastKind = KindOf(d);
                }
            }
            var mouse = Mouse.current;
            if (
                mouse != null
                && mouse.added
                && (
                    mouse.leftButton.wasPressedThisFrame
                    || mouse.delta.ReadValue().sqrMagnitude > 9f
                )
            )
                LastKind = DeviceKind.Keyboard;
            if (_injected.Count > 0)
            {
                Frames.AddRange(_injected);
                _injected.Clear();
            }
        }

        public static DeviceKind KindOf(InputDevice d) =>
            d switch
            {
                DualShockGamepad _ => DeviceKind.PlayStation,
                Gamepad _ => DeviceKind.Xbox,
                _ => DeviceKind.Keyboard,
            };

        static int Axis(float v) =>
            v > 0.5f ? 1
            : v < -0.5f ? -1
            : 0;

        static NavFrame ReadPad(Gamepad g)
        {
            var stick = g.leftStick.ReadValue();
            var dpad = g.dpad.ReadValue();
            int x = Axis(dpad.x) != 0 ? Axis(dpad.x) : Axis(stick.x);
            int y = Axis(dpad.y) != 0 ? Axis(dpad.y) : Axis(stick.y);
            return new NavFrame
            {
                Device = g,
                X = x,
                Y = y,
                Submit = g.buttonSouth.wasPressedThisFrame,
                Cancel = g.buttonEast.wasPressedThisFrame,
                TabPrev = g.leftShoulder.wasPressedThisFrame,
                TabNext = g.rightShoulder.wasPressedThisFrame,
                Start = g.startButton.wasPressedThisFrame,
                Any =
                    g.buttonNorth.wasPressedThisFrame
                    || g.buttonWest.wasPressedThisFrame
                    || g.selectButton.wasPressedThisFrame,
            };
        }

        static NavFrame ReadKeys(Keyboard k)
        {
            int right = k.rightArrowKey.isPressed || k.dKey.isPressed ? 1 : 0;
            int left = k.leftArrowKey.isPressed || k.aKey.isPressed ? 1 : 0;
            int up = k.upArrowKey.isPressed || k.wKey.isPressed ? 1 : 0;
            int down = k.downArrowKey.isPressed || k.sKey.isPressed ? 1 : 0;
            return new NavFrame
            {
                Device = k,
                X = right - left,
                Y = up - down,
                Submit =
                    k.enterKey.wasPressedThisFrame
                    || k.numpadEnterKey.wasPressedThisFrame
                    || k.spaceKey.wasPressedThisFrame,
                Cancel = k.escapeKey.wasPressedThisFrame || k.backspaceKey.wasPressedThisFrame,
                TabPrev = k.qKey.wasPressedThisFrame,
                TabNext = k.eKey.wasPressedThisFrame,
                Start = k.escapeKey.wasPressedThisFrame,
                Any = k.anyKey.wasPressedThisFrame,
            };
        }

        void ApplyRepeat(int id, ref NavFrame f, float now)
        {
            _repeats.TryGetValue(id, out var r);
            f.X = Step(f.X, ref r.X, ref r.NextX, now);
            f.Y = Step(f.Y, ref r.Y, ref r.NextY, now);
            _repeats[id] = r;
        }

        static int Step(int raw, ref int held, ref float next, float now)
        {
            if (raw == 0)
            {
                held = 0;
                return 0;
            }
            if (raw != held)
            {
                held = raw;
                next = now + RepeatDelay;
                return raw;
            }
            if (now < next)
                return 0;
            next = Mathf.Max(next + RepeatRate, now);
            return raw;
        }
    }
}
