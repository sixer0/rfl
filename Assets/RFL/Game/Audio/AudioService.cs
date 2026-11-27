#nullable enable
using System.Collections.Generic;
using RFL.Game.Core;
using RFL.Game.Profile;
using UnityEngine;

namespace RFL.Game.Audio
{
    /// <summary>Plays cues by ID. The mixer (Tech Spec, Audio) replaces the per-category volumes later.</summary>
    public sealed class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; } = null!;
        readonly Dictionary<ushort, AudioClip> _clips = new Dictionary<ushort, AudioClip>();
        AudioSource _ui = null!;

        void Awake()
        {
            Instance = this;
            _ui = gameObject.AddComponent<AudioSource>();
            _ui.playOnAwake = false;
            _ui.spatialBlend = 0f;
            foreach (var cue in UiCues.All)
                _clips[cue.Id] = UiCues.Build(cue);
            foreach (var cue in UiCues.Fight)
                _clips[cue.Id] = UiCues.Build(cue);
        }

        public static void Play(ushort cueId) => PlayAt(cueId, s => s.ui);

        /// <summary>Fight sounds follow the Effects volume.</summary>
        public static void PlayFight(ushort cueId) => PlayAt(cueId, s => s.effects);

        static void PlayAt(ushort cueId, System.Func<SettingsData, int> volume)
        {
            if (Instance == null || !Instance._clips.TryGetValue(cueId, out var clip))
                return;
            var settings = GameServices.Settings;
            float v = settings != null ? SettingsService.Volume01(volume(settings.S)) : 0.7f;
            Instance._ui.PlayOneShot(clip, v);
        }
    }
}
