#nullable enable
using NUnit.Framework;
using RFL.Sim;

namespace RFL.Tests
{
    public class StateHasherTests
    {
        // Golden values from a reference FNV-1a 64 run over little-endian bytes.
        [Test]
        public void Empty_IsTheOffsetBasis()
        {
            Assert.AreEqual(StateHasher.OffsetBasis, new StateHasher().Value);
            Assert.AreEqual(14695981039346656037UL, default(StateHasher).Value);
        }

        [Test]
        public void Add_MatchesGoldenValues()
        {
            var zero = new StateHasher();
            zero.Add(0L);
            Assert.AreEqual(12161962213042174405UL, zero.Value);

            var minusOne = new StateHasher();
            minusOne.Add(-1L);
            Assert.AreEqual(10157053723145373757UL, minusOne.Value);
        }

        [Test]
        public void Add_IsOrderSensitive()
        {
            var oneTwo = new StateHasher();
            oneTwo.Add(1L);
            oneTwo.Add(2L);

            var twoOne = new StateHasher();
            twoOne.Add(2L);
            twoOne.Add(1L);

            Assert.AreEqual(8581494755304202342UL, oneTwo.Value);
            Assert.AreEqual(513837244993915590UL, twoOne.Value);
        }

        [Test]
        public void Overloads_HashLikeTheirLongValue()
        {
            var asInt = new StateHasher();
            asInt.Add(5);
            var asTrue = new StateHasher();
            asTrue.Add(true);
            var asFalse = new StateHasher();
            asFalse.Add(false);
            var asFix = new StateHasher();
            asFix.Add(Fix.One);

            Assert.AreEqual(HashOfLong(5L), asInt.Value);
            Assert.AreEqual(HashOfLong(1L), asTrue.Value);
            Assert.AreEqual(HashOfLong(0L), asFalse.Value);
            Assert.AreEqual(HashOfLong(Fix.OneRaw), asFix.Value);
        }

        private static ulong HashOfLong(long value)
        {
            var hasher = new StateHasher();
            hasher.Add(value);
            return hasher.Value;
        }
    }
}
