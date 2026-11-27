#nullable enable
using NUnit.Framework;
using RFL.Sim;

namespace RFL.Tests
{
    public class SeededRandomTests
    {
        // Golden values from a reference PCG32 run (same algorithm, same fixed stream).
        // They pin the sequence: if these change, saved replays and hashes break.
        [Test]
        public void NextUInt_MatchesGoldenSequence()
        {
            var seedOne = SeededRandom.FromSeed(1);
            Assert.AreEqual(1412771199u, seedOne.NextUInt());
            Assert.AreEqual(1791099446u, seedOne.NextUInt());
            Assert.AreEqual(124312908u, seedOne.NextUInt());
            Assert.AreEqual(1968572995u, seedOne.NextUInt());

            var seedFortyTwo = SeededRandom.FromSeed(42);
            Assert.AreEqual(3270867926u, seedFortyTwo.NextUInt());
            Assert.AreEqual(1795671209u, seedFortyTwo.NextUInt());
        }

        [Test]
        public void SameSeed_GivesSameSequence()
        {
            var a = SeededRandom.FromSeed(2026);
            var b = SeededRandom.FromSeed(2026);
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), "draw " + i);
            }
        }

        [Test]
        public void Copy_IsASnapshotThatDoesNotAdvanceTheOriginal()
        {
            var original = SeededRandom.FromSeed(5);
            var snapshot = original;

            uint first = snapshot.NextUInt();

            Assert.AreNotEqual(original.State, snapshot.State);
            Assert.AreEqual(first, original.NextUInt());
        }

        [Test]
        public void Range_MatchesGoldenSequence()
        {
            var random = SeededRandom.FromSeed(7);
            int[] expected = { 3, 0, 2, 0, 4, 5, 0, 4 };
            foreach (int value in expected)
            {
                Assert.AreEqual(value, random.Range(0, 6));
            }
        }

        [Test]
        public void Range_OffsetShiftsTheSameDraws()
        {
            var zeroBased = SeededRandom.FromSeed(7);
            var tenBased = SeededRandom.FromSeed(7);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(zeroBased.Range(0, 6) + 10, tenBased.Range(10, 16));
            }
        }

        [Test]
        public void Range_StaysInsideBounds()
        {
            var random = SeededRandom.FromSeed(99);
            for (int i = 0; i < 10000; i++)
            {
                int value = random.Range(-3, 3);
                Assert.GreaterOrEqual(value, -3);
                Assert.Less(value, 3);
            }
        }

        [Test]
        public void Range_EmptyReturnsMinimum()
        {
            var random = SeededRandom.FromSeed(1);
            ulong before = random.State;

            Assert.AreEqual(4, random.Range(4, 4));
            Assert.AreEqual(4, random.Range(4, 2));
            Assert.AreEqual(before, random.State, "an empty range draws nothing");
        }
    }
}
