#nullable enable
using RFL.Game.Audio;
using RFL.Game.Input;
using RFL.Game.Profile;
using UnityEngine;

namespace RFL.Game.Core
{
    /// <summary>
    /// Creates the services that live for the whole session, before the first scene loads,
    /// so any scene can be played directly in the editor.
    /// </summary>
    public static class GameServices
    {
        public static SettingsService Settings { get; private set; } = null!;
        public static MenuInput Input => MenuInput.Instance;
        public static AudioService Audio => AudioService.Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            var go = new GameObject("RFL Services");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<MenuInput>();
            go.AddComponent<AudioService>();
            UseProfilePath(ProfileStore.DefaultPath);
            DevTools.ShotRunner.StartIfRequested(go);
        }

        /// <summary>Loads the profile at <paramref name="path"/> and applies it. Tests use a temporary path.</summary>
        public static void UseProfilePath(string path)
        {
            Settings = new SettingsService(path);
            Settings.ApplyAudio();
            Settings.ApplyDisplay(atBoot: true);
        }
    }
}
