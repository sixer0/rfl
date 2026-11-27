#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace RFL.Editor
{
    /// <summary>
    /// ui_strings.csv (key, id, en) → the "UI" string table collection. The CSV is the source;
    /// the tables are generated. Import fails with every problem listed.
    /// </summary>
    public static class StringTableImporter
    {
        public const string CsvPath = "Assets/RFL/Data/Strings/ui_strings.csv";
        public const string TableName = "UI";
        public static readonly string[] Codes = { "id", "en" };
        static readonly Regex KeyFormat = new Regex(@"^[a-z0-9_]+(\.[a-z0-9_]+)+$");

        public static List<StringRow> Load(out List<string> errors)
        {
            errors = new List<string>();
            var rows = new List<StringRow>();
            var table = CsvTable.Read(File.ReadAllText(CsvPath));
            if (table.Count == 0 || string.Join(",", table[0]) != "key,id,en")
            {
                errors.Add($"{CsvPath}: header must be exactly key,id,en");
                return rows;
            }
            var seen = new HashSet<string>();
            for (int i = 1; i < table.Count; i++)
            {
                var r = table[i];
                int line = i + 1;
                if (r.Length == 1 && string.IsNullOrWhiteSpace(r[0]))
                    continue;
                if (r.Length != 3)
                {
                    errors.Add($"line {line}: expected 3 fields, found {r.Length}");
                    continue;
                }
                string key = r[0].Trim();
                if (!KeyFormat.IsMatch(key))
                    errors.Add(
                        $"line {line}: key '{key}' is not lower-case dotted (for example ui.menu.versus)"
                    );
                if (!seen.Add(key))
                    errors.Add($"line {line}: key '{key}' appears twice");
                if (string.IsNullOrWhiteSpace(r[1]))
                    errors.Add($"line {line}: '{key}' has no Bahasa Indonesia text");
                if (string.IsNullOrWhiteSpace(r[2]))
                    errors.Add($"line {line}: '{key}' has no English text");
                rows.Add(new StringRow(line, key, r[1], r[2]));
            }
            return rows;
        }

        [MenuItem("RFL/Import UI Strings")]
        public static void Import()
        {
            var rows = Load(out var errors);
            if (errors.Count > 0)
                throw new System.InvalidOperationException(
                    "UI strings not imported:\n" + string.Join("\n", errors)
                );

            var coll = LocalizationEditorSettings.GetStringTableCollection(TableName)
                ?? LocalizationEditorSettings.CreateStringTableCollection(
                    TableName,
                    ProjectSetup.LocDir + "/Tables"
                );
            var keys = new HashSet<string>();
            foreach (var row in rows)
            {
                keys.Add(row.Key);
                if (!coll.SharedData.Contains(row.Key))
                    coll.SharedData.AddKey(row.Key);
                Set(coll, "id", row.Key, row.Id);
                Set(coll, "en", row.Key, row.En);
            }
            foreach (var key in coll.SharedData.Entries.Select(e => e.Key).ToList())
            {
                if (!keys.Contains(key))
                    coll.RemoveEntry(key);
            }
            EditorUtility.SetDirty(coll.SharedData);
            foreach (var t in coll.StringTables)
                EditorUtility.SetDirty(t);
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(
                $"UI strings imported: {rows.Count} keys in {Codes.Length} languages."
            );
        }

        static void Set(StringTableCollection coll, string code, string key, string value)
        {
            var table = (StringTable)coll.GetTable(new LocaleIdentifier(code));
            if (table == null)
                throw new System.InvalidOperationException(
                    $"No '{code}' table in {TableName}; run RFL/Setup first."
                );
            var entry = table.GetEntry(key);
            if (entry != null)
                entry.Value = value;
            else
                table.AddEntry(key, value);
        }
    }
}
