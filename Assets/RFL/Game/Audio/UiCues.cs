#nullable enable
using UnityEngine;

namespace RFL.Game.Audio
{
    /// <summary>
    /// The UI cues. Placeholder sounds are synthesised here, so they carry no licence; the real
    /// sounds replace the generator later without changing any ID.
    /// </summary>
    public static class UiCues
    {
        public const ushort NavMove = 9001;
        public const ushort NavConfirm = 9002;
        public const ushort NavBack = 9003;
        public const ushort NavError = 9004;
        public const ushort TabSwitch = 9005;

        // Fight cues (placeholder synth) in the 1000 range ▲.
        public const ushort HitLight = 1001;
        public const ushort HitHeavy = 1002;
        public const ushort Block = 1003;
        public const ushort Counter = 1004;
        public const ushort Groggy = 1005;
        public const ushort Overload = 1006;
        public const ushort Bell = 1007;
        public const ushort PartBreak = 1008;

        public static readonly UiCue[] Fight =
        {
            new UiCue(HitLight, "sfx.hit.light", 220f, 90f, 0.08f, true),
            new UiCue(HitHeavy, "sfx.hit.heavy", 160f, 45f, 0.16f, true),
            new UiCue(Block, "sfx.block.light", 900f, 600f, 0.05f, false),
            new UiCue(Counter, "sfx.hit.counter", 420f, 120f, 0.14f, true),
            new UiCue(Groggy, "sfx.status.groggy", 300f, 600f, 0.35f, false),
            new UiCue(Overload, "sfx.status.overload", 120f, 60f, 0.5f, true),
            new UiCue(Bell, "sfx.ring.bell", 1320f, 1300f, 0.6f, false),
            new UiCue(PartBreak, "sfx.part.break", 260f, 80f, 0.25f, true),
        };

        public static readonly UiCue[] All =
        {
            new UiCue(NavMove, "ui.nav.move", 880f, 880f, 0.035f, false),
            new UiCue(NavConfirm, "ui.nav.confirm", 660f, 990f, 0.09f, false),
            new UiCue(NavBack, "ui.nav.back", 560f, 360f, 0.09f, false),
            new UiCue(NavError, "ui.nav.error", 170f, 170f, 0.12f, true),
            new UiCue(TabSwitch, "ui.tab.switch", 740f, 740f, 0.05f, false),
        };

        public static AudioClip Build(UiCue cue)
        {
            const int rate = 48000;
            int n = Mathf.Max(1, (int)(rate * cue.Seconds));
            var data = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float hz = Mathf.Lerp(cue.StartHz, cue.EndHz, t);
                phase += 2.0 * System.Math.PI * hz / rate;
                float s = (float)System.Math.Sin(phase);
                if (cue.Square)
                    s = s >= 0f ? 0.5f : -0.5f;
                float attack = Mathf.Min(1f, i / (rate * 0.004f));
                float decay = (1f - t) * (1f - t);
                data[i] = s * attack * decay * 0.35f;
            }
            var clip = AudioClip.Create(cue.Key, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
