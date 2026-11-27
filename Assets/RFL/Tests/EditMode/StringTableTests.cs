#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RFL.Editor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace RFL.Tests
{
    /// <summary>PF-REQ-020: every string exists in both languages; no player text is written outside the tables.</summary>
    public class StringTableTests
    {
        const string Root = "Assets/RFL";

        static HashSet<string> CsvKeys()
        {
            var rows = StringTableImporter.Load(out var errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
            return new HashSet<string>(rows.Select(r => r.Key));
        }

        [Test]
        public void CsvIsValid() => CsvKeys();

        [Test]
        public void EveryKeyUsedInUxmlExists()
        {
            var keys = CsvKeys();
            var missing = new List<string>();
            foreach (var file in Directory.GetFiles(Root, "*.uxml", SearchOption.AllDirectories))
            foreach (Match m in Regex.Matches(File.ReadAllText(file), "key=\"([^\"]+)\""))
                if (!keys.Contains(m.Groups[1].Value))
                    missing.Add($"{Path.GetFileName(file)}: {m.Groups[1].Value}");
            Assert.IsEmpty(missing, string.Join("\n", missing));
        }

        [Test]
        public void EveryKeyLiteralInCodeExists()
        {
            var keys = CsvKeys();
            var missing = new List<string>();
            var literal = new Regex("\"((?:ui|robot|move)\\.[a-z0-9_.]+)\"");
            foreach (
                var file in new[] { "Sim", "Data", "Game", "Editor" }.SelectMany(d =>
                    Directory.GetFiles(Root + "/" + d, "*.cs", SearchOption.AllDirectories)
                )
            )
            foreach (Match m in literal.Matches(File.ReadAllText(file)))
            {
                if (file.Replace('\\', '/').Contains("/Audio/"))
                    continue; // cue keys (PF-REQ-073) are not string keys
                var k = m.Groups[1].Value;
                if (k.EndsWith("."))
                    continue; // prefix used to build a key, e.g. "ui.settings.lang." + code
                if (!keys.Contains(k))
                    missing.Add($"{Path.GetFileName(file)}: {k}");
            }
            Assert.IsEmpty(missing, string.Join("\n", missing));
        }

        [Test]
        public void UxmlHasNoLiteralText()
        {
            var found = new List<string>();
            foreach (var file in Directory.GetFiles(Root, "*.uxml", SearchOption.AllDirectories))
            foreach (Match m in Regex.Matches(File.ReadAllText(file), "\\stext=\"([^\"]+)\""))
                found.Add($"{Path.GetFileName(file)}: {m.Groups[1].Value}");
            Assert.IsEmpty(
                found,
                "Player text must come from string keys:\n" + string.Join("\n", found)
            );
        }

        [Test]
        public void ImportedTablesMatchTheCsv()
        {
            var rows = StringTableImporter.Load(out _);
            var coll = LocalizationEditorSettings.GetStringTableCollection(
                StringTableImporter.TableName
            );
            Assert.IsNotNull(coll, "Run RFL/Setup: the UI string table collection is missing.");
            foreach (var code in StringTableImporter.Codes)
            {
                var table = (StringTable)coll.GetTable(new LocaleIdentifier(code));
                Assert.IsNotNull(table, code);
                foreach (var r in rows)
                {
                    var entry = table.GetEntry(r.Key);
                    Assert.IsNotNull(entry, $"{code}: {r.Key} missing; re-import the strings");
                    Assert.AreEqual(code == "id" ? r.Id : r.En, entry.Value, $"{code}: {r.Key}");
                }
            }
        }

        [Test]
        public void IndonesianNumbersUseDotThousands()
        {
            Assert.AreEqual(
                "1.550",
                1550.ToString("N0", new System.Globalization.CultureInfo("id"))
            );
            Assert.AreEqual(
                "1,550",
                1550.ToString("N0", new System.Globalization.CultureInfo("en"))
            );
        }
    }
}
