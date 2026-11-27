#nullable enable
using RFL.Sim;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RFL.Game.Core
{
    /// <summary>What the idle Training opponent does. It never attacks (week goal 2026-W48).</summary>
    public enum DummyAction
    {
        Stand = 0,
        Block = 1,
        CrouchBlock = 2,
    }
}
