#nullable enable
using System.Collections.Generic;
using System.Linq;
using RFL.Game.UI;
using UnityEngine;

namespace RFL.Game.Profile
{
    /// <summary>Owns the loaded profile and applies its settings to the engine.</summary>
    public sealed class SettingsService
    {
        readonly string _path;

        public SettingsService(string path)
        {
            _path = path;
            Profile = ProfileStore.Load(path);
        }

        public ProfileData Profile { get; private set; }
        public SettingsData S => Profile.settings;
        public string Path => _path;

        public static readonly string[] Languages = { "id", "en" };

        public string LanguageCode =>
            Languages.Contains(S.language) ? S.language
            : Application.systemLanguage == SystemLanguage.Indonesian ? "id"
            : "en";

        public void Save() => ProfileStore.Save(Profile, _path);

        public void ApplyLanguage() => Loc.SetLanguage(LanguageCode);

        public void ApplyAudio() => AudioListener.volume = S.master / (float)SettingsData.VolumeMax;

        public static float Volume01(int steps) =>
            Mathf.Clamp01(steps / (float)SettingsData.VolumeMax);

        public void ApplyDisplay(bool atBoot = false)
        {
            QualitySettings.vSyncCount = S.vsync ? 1 : 0;
            Application.targetFrameRate = S.vsync ? -1 : 120;
            if (Application.isEditor)
                return;
            if (atBoot && S.resolutionW <= 0 && S.windowMode == 0)
                return; // first launch: keep the player defaults
            var native = Screen.currentResolution;
            int w = S.resolutionW > 0 ? S.resolutionW : native.width;
            int h = S.resolutionH > 0 ? S.resolutionH : native.height;
            Screen.SetResolution(w, h, ModeOf(S.windowMode));
        }

        public static FullScreenMode ModeOf(int i) =>
            i switch
            {
                1 => FullScreenMode.Windowed,
                2 => FullScreenMode.ExclusiveFullScreen,
                _ => FullScreenMode.FullScreenWindow,
            };

        /// <summary>Distinct resolutions, smallest first; always at least the current one.</summary>
        public static List<Vector2Int> Resolutions()
        {
            var list = Screen
                .resolutions.Select(r => new Vector2Int(r.width, r.height))
                .Distinct()
                .OrderBy(v => v.x * 100000 + v.y)
                .ToList();
            var cur = new Vector2Int(Screen.width, Screen.height);
            if (list.Count == 0 || !list.Contains(cur))
                list.Add(cur);
            return list;
        }

        public void ResetTab(SettingsTab tab)
        {
            var d = new SettingsData();
            switch (tab)
            {
                case SettingsTab.Game:
                    S.language = d.language;
                    ApplyLanguage();
                    break;
                case SettingsTab.Audio:
                    S.master = d.master;
                    S.music = d.music;
                    S.effects = d.effects;
                    S.ui = d.ui;
                    ApplyAudio();
                    break;
                case SettingsTab.Display:
                    S.windowMode = d.windowMode;
                    S.resolutionW = d.resolutionW;
                    S.resolutionH = d.resolutionH;
                    S.vsync = d.vsync;
                    ApplyDisplay();
                    break;
            }
        }
    }
}
