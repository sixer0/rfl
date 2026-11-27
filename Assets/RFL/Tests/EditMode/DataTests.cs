#nullable enable
using System.Linq;
using NUnit.Framework;
using RFL.Data;
using RFL.Game.Audio;
using RFL.Sim;

namespace RFL.Tests
{
    public class DataTests
    {
        [Test]
        public void StarterTypesMatchTheGdd()
        {
            var s = StarterTypes.Get(StarterType.Standard);
            Assert.AreEqual(
                (WeightClass.Middle, 1550, 1000, 3, 3, 3),
                (s.Class, s.Hp, s.Stamina, s.Agility, s.Power, s.OverloadResist)
            );
            var p = StarterTypes.Get(StarterType.Power);
            Assert.AreEqual(
                (WeightClass.Heavy, 1708, 800, 2, 5, 4),
                (p.Class, p.Hp, p.Stamina, p.Agility, p.Power, p.OverloadResist)
            );
            var f = StarterTypes.Get(StarterType.Speed);
            Assert.AreEqual(
                (WeightClass.Light, 1386, 1250, 5, 2, 2),
                (f.Class, f.Hp, f.Stamina, f.Agility, f.Power, f.OverloadResist)
            );
            Assert.IsTrue(StarterTypes.All.All(t => t.TechMoveKeys.Length == 4));
        }

        [Test]
        public void EverySceneUiDocumentHasPanelSettings()
        {
            foreach (
                var scene in new[]
                {
                    "Assets/RFL/Scenes/Menu.unity",
                    "Assets/RFL/Scenes/GreyBox.unity",
                }
            )
            {
                var text = System.IO.File.ReadAllText(scene);
                StringAssert.Contains(
                    "m_PanelSettings: {fileID: 11400000",
                    text,
                    scene + ": UIDocument without PanelSettings renders nothing"
                );
                StringAssert.DoesNotContain("m_PanelSettings: {fileID: 0}", text, scene);
            }
        }

        [Test]
        public void CueIdsAndKeysAreUniqueAndStructured()
        {
            var all = UiCues.All.Concat(UiCues.Fight).ToArray();
            Assert.AreEqual(all.Length, all.Select(c => c.Id).Distinct().Count());
            Assert.AreEqual(all.Length, all.Select(c => c.Key).Distinct().Count());
            foreach (var c in UiCues.Fight)
                Assert.That(c.Id, Is.InRange(1000, 1999), c.Key);
            foreach (var c in UiCues.All)
            {
                StringAssert.IsMatch(@"^[a-z]+(\.[a-z0-9_]+){2,}$", c.Key);
                Assert.That(c.Id, Is.InRange(9000, 9999), c.Key);
            }
        }
    }
}
