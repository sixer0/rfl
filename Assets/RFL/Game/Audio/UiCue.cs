#nullable enable
using UnityEngine;

namespace RFL.Game.Audio
{
    /// <summary>A UI sound cue: permanent numeric ID plus structured key (PF-REQ-073; UI range 9000–9999 ▲).</summary>
    public readonly struct UiCue
    {
        public readonly ushort Id;
        public readonly string Key;
        public readonly float StartHz;
        public readonly float EndHz;
        public readonly float Seconds;
        public readonly bool Square;

        public UiCue(ushort id, string key, float startHz, float endHz, float seconds, bool square)
        {
            Id = id;
            Key = key;
            StartHz = startHz;
            EndHz = endHz;
            Seconds = seconds;
            Square = square;
        }
    }
}
