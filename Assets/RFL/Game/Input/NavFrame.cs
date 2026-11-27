#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace RFL.Game.Input
{
    /// <summary>One device's menu input for one frame. X and Y are pulses (with key repeat), up is +1.</summary>
    public struct NavFrame
    {
        public InputDevice Device;
        public int X;
        public int Y;
        public bool Submit;
        public bool Cancel;
        public bool TabPrev;
        public bool TabNext;
        public bool Start;
        public bool Any;

        public bool HasAction =>
            X != 0 || Y != 0 || Submit || Cancel || TabPrev || TabNext || Start;
    }
}
