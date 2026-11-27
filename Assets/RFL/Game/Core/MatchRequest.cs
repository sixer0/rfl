#nullable enable
using RFL.Sim;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RFL.Game.Core
{
    /// <summary>Hand-over between the menu scene and the fight scene.</summary>
    public static class MatchRequest
    {
        public static MatchConfig Config = MatchConfig.Default;
        public static InputDevice? P1Device;
        public static InputDevice? P2Device;
        public static ReturnTarget ReturnTo = ReturnTarget.Title;
        public static int LastP1Index;
        public static int LastP2Index;
        public static CpuLevel LastCpu = CpuLevel.Normal;
        public static DummyAction Dummy = DummyAction.Stand;
        public static bool ShowFrames = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Config = MatchConfig.Default;
            P1Device = null;
            P2Device = null;
            ReturnTo = ReturnTarget.Title;
            LastP1Index = 0;
            LastP2Index = 0;
            LastCpu = CpuLevel.Normal;
            Dummy = DummyAction.Stand;
            ShowFrames = true;
        }
    }
}
