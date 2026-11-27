#nullable enable
using System;
using RFL.Game.Input;
using UnityEngine.UIElements;

namespace RFL.Game.UI
{
    [Flags]
    public enum PromptSet
    {
        None = 0,
        Confirm = 1,
        Back = 2,
        Change = 4,
        Tabs = 8,
        Pause = 16,
        Skip = 32,
    }
}
