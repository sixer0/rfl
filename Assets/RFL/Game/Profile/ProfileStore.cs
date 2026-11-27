#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace RFL.Game.Profile
{
    /// <summary>
    /// Atomic, versioned profile save (PF-REQ-022): write to a temporary file, then replace.
    /// JSON holds only integers, booleans and an ISO-8601 time in the invariant culture (PF-REQ-071).
    /// </summary>
    public static class ProfileStore
    {
        public const int SchemaVersion = 1;

        public static string DefaultPath =>
            Path.Combine(Application.persistentDataPath, "profile.json");

        public static ProfileData Load(string path)
        {
            if (!File.Exists(path))
                return new ProfileData();
            try
            {
                var json = File.ReadAllText(path, Encoding.UTF8);
                var data = JsonUtility.FromJson<ProfileData>(json);
                if (data == null || data.settings == null)
                    throw new InvalidDataException("empty profile");
                return Migrate(data);
            }
            catch (Exception e)
            {
                // Keep the unreadable file for support, start from defaults.
                var bad = path + ".bad";
                try
                {
                    File.Copy(path, bad, overwrite: true);
                }
                catch (IOException) { }
                UnityEngine.Debug.LogWarning(
                    $"Profile at {path} could not be read ({e.Message}); defaults used, copy kept at {bad}."
                );
                return new ProfileData();
            }
        }

        public static void Save(ProfileData data, string path)
        {
            data.schema_version = SchemaVersion;
            data.saved_at = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            if (File.Exists(path))
                File.Replace(tmp, path, destinationBackupFileName: null);
            else
                File.Move(tmp, path);
        }

        /// <summary>Each schema change adds one step here; saves from every earlier version must load (PF-REQ-122).</summary>
        static ProfileData Migrate(ProfileData data)
        {
            if (data.schema_version > SchemaVersion)
                UnityEngine.Debug.LogWarning(
                    $"Profile schema {data.schema_version} is newer than this build ({SchemaVersion})."
                );
            data.schema_version = SchemaVersion;
            return data;
        }
    }
}
