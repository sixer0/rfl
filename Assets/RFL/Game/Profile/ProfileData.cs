#nullable enable
using System;

namespace RFL.Game.Profile
{
    // Field names are the profile save-file keys (persisted, synced by Steam Cloud), so they
    // keep the file's spelling rather than PascalCase. Renaming one is a save-format change.
    /// <summary>One profile per player on this PC (02 §5.2). Field names are the JSON keys.</summary>
    [Serializable]
    public sealed class ProfileData
    {
        public int schema_version = ProfileStore.SchemaVersion;
        public string saved_at = "";
        public SettingsData settings = new SettingsData();
    }
}
