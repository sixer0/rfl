#nullable enable
using System;

namespace RFL.Game.Profile
{
    // Field names are the profile save-file keys (persisted, synced by Steam Cloud), so they
    // keep the file's spelling rather than PascalCase. Renaming one is a save-format change.
    /// <summary>All settings are integers or booleans, so the file never depends on a culture (PF-REQ-071).</summary>
    [Serializable]
    public sealed class SettingsData
    {
        /// <summary>"id" or "en"; empty means "follow the system language".</summary>
        public string language = "";

        public int master = 8;
        public int music = 7;
        public int effects = 8;
        public int ui = 7;

        /// <summary>0 borderless fullscreen, 1 windowed, 2 exclusive fullscreen.</summary>
        public int windowMode = 0;

        /// <summary>0 means the display's native resolution.</summary>
        public int resolutionW = 0;
        public int resolutionH = 0;
        public bool vsync = true;

        public const int VolumeMax = 10;
    }
}
