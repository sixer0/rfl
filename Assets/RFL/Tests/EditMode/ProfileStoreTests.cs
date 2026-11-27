#nullable enable
using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using RFL.Game.Profile;

namespace RFL.Tests
{
    /// <summary>AC-081 and PF-REQ-022: the profile is culture-independent and written atomically.</summary>
    public class ProfileStoreTests
    {
        string _dir = null!;
        CultureInfo _saved = null!;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "rfl-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _saved = CultureInfo.CurrentCulture;
        }

        [TearDown]
        public void TearDown()
        {
            CultureInfo.CurrentCulture = _saved;
            Directory.Delete(_dir, recursive: true);
        }

        static ProfileData Sample()
        {
            var p = new ProfileData();
            p.settings.language = "id";
            p.settings.master = 3;
            p.settings.ui = 10;
            p.settings.windowMode = 1;
            p.settings.resolutionW = 1280;
            p.settings.resolutionH = 800;
            p.settings.vsync = false;
            return p;
        }

        [Test]
        public void SavedUnderIndonesianLoadsUnderEnglishAndBack()
        {
            var path = Path.Combine(_dir, "profile.json");
            CultureInfo.CurrentCulture = new CultureInfo("id-ID");
            ProfileStore.Save(Sample(), path);

            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var loaded = ProfileStore.Load(path);
            Assert.AreEqual("id", loaded.settings.language);
            Assert.AreEqual(3, loaded.settings.master);
            Assert.AreEqual(10, loaded.settings.ui);
            Assert.AreEqual(1, loaded.settings.windowMode);
            Assert.AreEqual(1280, loaded.settings.resolutionW);
            Assert.AreEqual(800, loaded.settings.resolutionH);
            Assert.IsFalse(loaded.settings.vsync);
            Assert.AreEqual(ProfileStore.SchemaVersion, loaded.schema_version);

            ProfileStore.Save(loaded, path);
            CultureInfo.CurrentCulture = new CultureInfo("id-ID");
            Assert.AreEqual(1280, ProfileStore.Load(path).settings.resolutionW);
        }

        [Test]
        public void SavedJsonIsIdenticalUnderBothCultures()
        {
            var a = Path.Combine(_dir, "a.json");
            var b = Path.Combine(_dir, "b.json");
            CultureInfo.CurrentCulture = new CultureInfo("id-ID");
            ProfileStore.Save(Sample(), a);
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            ProfileStore.Save(Sample(), b);
            string Strip(string s) =>
                System.Text.RegularExpressions.Regex.Replace(s, "\"saved_at\": \"[^\"]*\"", "");
            Assert.AreEqual(Strip(File.ReadAllText(a)), Strip(File.ReadAllText(b)));
        }

        [Test]
        public void SaveLeavesNoTemporaryFileAndOverwrites()
        {
            var path = Path.Combine(_dir, "profile.json");
            ProfileStore.Save(Sample(), path);
            var p = Sample();
            p.settings.master = 9;
            ProfileStore.Save(p, path);
            Assert.IsFalse(File.Exists(path + ".tmp"));
            Assert.AreEqual(9, ProfileStore.Load(path).settings.master);
        }

        [Test]
        public void CorruptFileGivesDefaultsAndKeepsACopy()
        {
            var path = Path.Combine(_dir, "profile.json");
            File.WriteAllText(path, "{ this is not json");
            var p = ProfileStore.Load(path);
            Assert.AreEqual(new SettingsData().master, p.settings.master);
            Assert.IsTrue(File.Exists(path + ".bad"));
        }

        [Test]
        public void MissingFileGivesDefaults()
        {
            var p = ProfileStore.Load(Path.Combine(_dir, "none.json"));
            Assert.AreEqual(ProfileStore.SchemaVersion, p.schema_version);
            Assert.AreEqual(string.Empty, p.settings.language);
        }
    }
}
