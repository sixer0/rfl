#nullable enable
using System;
using System.Globalization;
using NUnit.Framework;
using RFL.Sim;

namespace RFL.Tests
{
    public class FixTests
    {
        [Test]
        public void FromInt_ShiftsIntoTheIntegerBits()
        {
            Assert.AreEqual(3L << 16, Fix.FromInt(3).Raw);
            Assert.AreEqual(-(2L << 16), Fix.FromInt(-2).Raw);
            Assert.AreEqual(Fix.One, Fix.FromInt(1));
            Assert.AreEqual(Fix.Zero, Fix.FromInt(0));
        }

        [Test]
        public void FromRaw_KeepsTheBitsExactly()
        {
            Assert.AreEqual(1L, Fix.FromRaw(1).Raw);
            Assert.AreEqual(Fix.One, Fix.FromRaw(Fix.OneRaw));
            Assert.AreEqual(long.MinValue, Fix.FromRaw(long.MinValue).Raw);
        }

        [Test]
        public void Add_ThreePlusTwo_IsFive()
        {
            Assert.AreEqual(Fix.FromInt(5), Fix.FromInt(3) + Fix.FromInt(2));
            Assert.AreEqual(Fix.FromRaw(3), Fix.FromRaw(1) + Fix.FromRaw(2));
        }

        [Test]
        public void Sub_ThreeMinusFive_IsMinusTwo()
        {
            Assert.AreEqual(Fix.FromInt(-2), Fix.FromInt(3) - Fix.FromInt(5));
        }

        [Test]
        public void Negate_FlipsTheSign()
        {
            Assert.AreEqual(Fix.FromInt(-4), -Fix.FromInt(4));
            Assert.AreEqual(Fix.Zero, -Fix.Zero);
        }

        [Test]
        public void Mul_ThreeTimesTwo_IsSix()
        {
            Fix a = Fix.FromInt(3);
            Fix b = Fix.FromInt(2);
            Assert.AreEqual(Fix.FromInt(6), a * b);
        }

        [Test]
        public void Mul_KeepsFractionsExact()
        {
            Fix oneAndHalf = Fix.FromRaw(Fix.OneRaw * 3 / 2);
            Assert.AreEqual(Fix.FromRaw(Fix.OneRaw * 9 / 4), oneAndHalf * oneAndHalf);
            Assert.AreEqual(Fix.FromInt(-3), Fix.FromInt(-2) * oneAndHalf);
        }

        [Test]
        public void Mul_FloorsTowardNegativeInfinity()
        {
            Assert.AreEqual(0L, (Fix.FromRaw(1) * Fix.FromRaw(1)).Raw);
            Assert.AreEqual(-1L, (Fix.FromRaw(-1) * Fix.FromRaw(1)).Raw);
        }

        [Test]
        public void Div_SevenByTwo_IsThreeAndHalf()
        {
            Assert.AreEqual(Fix.FromRaw(Fix.OneRaw * 7 / 2), Fix.FromInt(7) / Fix.FromInt(2));
            Assert.AreEqual(Fix.FromInt(-3), Fix.FromInt(6) / Fix.FromInt(-2));
        }

        [Test]
        public void Div_TruncatesTowardZero()
        {
            Assert.AreEqual(0L, (Fix.FromRaw(1) / Fix.FromInt(2)).Raw);
            Assert.AreEqual(0L, (Fix.FromRaw(-1) / Fix.FromInt(2)).Raw);
        }

        [Test]
        public void Div_ByZero_Throws()
        {
            Assert.Throws<DivideByZeroException>(() =>
            {
                Fix unused = Fix.One / Fix.Zero;
            });
        }

        [Test]
        public void Compare_OrdersByRawValue()
        {
            Fix low = Fix.FromInt(-1);
            Fix high = Fix.FromRaw(1);

            Assert.IsTrue(low < high);
            Assert.IsTrue(high > low);
            Assert.IsTrue(low <= Fix.FromInt(-1));
            Assert.IsTrue(high >= Fix.FromRaw(1));
            Assert.IsTrue(low <= high);
            Assert.IsFalse(low >= high);
            Assert.IsTrue(low != high);
            Assert.IsFalse(low == high);
            Assert.Less(low.CompareTo(high), 0);
            Assert.Greater(high.CompareTo(low), 0);
            Assert.AreEqual(0, high.CompareTo(Fix.FromRaw(1)));
        }

        [Test]
        public void Equals_MatchesRawAndHashCode()
        {
            Fix a = Fix.FromInt(9);
            object boxed = Fix.FromInt(9);

            Assert.IsTrue(a.Equals(Fix.FromInt(9)));
            Assert.IsTrue(a.Equals(boxed));
            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a.Equals(9));
            Assert.AreEqual(Fix.FromInt(9).GetHashCode(), a.GetHashCode());
        }

        [Test]
        public void ToString_ThreeDecimalsWithDotUnderAnyCulture()
        {
            CultureInfo saved = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("id-ID");
                Assert.AreEqual("1.500", (Fix.FromInt(3) / Fix.FromInt(2)).ToString());
                Assert.AreEqual("-0.250", Fix.FromRaw(-Fix.OneRaw / 4).ToString());
                Assert.AreEqual("-2.000", Fix.FromInt(-2).ToString());
                Assert.AreEqual("1234.000", Fix.FromInt(1234).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = saved;
            }
        }

        [Test]
        public void FromRatio_TruncatesTowardZero()
        {
            Assert.AreEqual(98304L, Fix.FromRatio(3, 2).Raw);
            Assert.AreEqual(21845L, Fix.FromRatio(1, 3).Raw);
            Assert.AreEqual(-21845L, Fix.FromRatio(-1, 3).Raw);
        }

        [Test]
        public void FromMilli_ConvertsMillimetresToMetres()
        {
            Assert.AreEqual(Fix.FromRatio(3, 2), Fix.FromMilli(1500));
            Assert.AreEqual(-16384L, Fix.FromMilli(-250).Raw);
            Assert.AreEqual(65L, Fix.FromMilli(1).Raw);
        }

        [Test]
        public void MulByInt_ScalesExactly()
        {
            Assert.AreEqual(Fix.FromInt(-6), Fix.FromInt(3) * -2);
            Assert.AreEqual(Fix.FromMilli(-250), Fix.FromMilli(250) * -1);
        }

        [Test]
        public void DivByInt_TruncatesTowardZero()
        {
            Assert.AreEqual(229376L, (Fix.FromInt(7) / 2).Raw);
            Assert.AreEqual(0L, (Fix.FromRaw(-1) / 2).Raw);
        }

        [Test]
        public void Abs_DropsTheSign()
        {
            Assert.AreEqual(Fix.FromInt(3), Fix.Abs(Fix.FromInt(-3)));
            Assert.AreEqual(Fix.FromInt(3), Fix.Abs(Fix.FromInt(3)));
            Assert.AreEqual(Fix.Zero, Fix.Abs(Fix.Zero));
        }

        [Test]
        public void Clamp_KeepsTheValueInsideTheRange()
        {
            Fix low = Fix.Zero;
            Fix high = Fix.FromInt(3);
            Assert.AreEqual(high, Fix.Clamp(Fix.FromInt(5), low, high));
            Assert.AreEqual(low, Fix.Clamp(Fix.FromInt(-1), low, high));
            Assert.AreEqual(Fix.FromInt(2), Fix.Clamp(Fix.FromInt(2), low, high));
        }

        [Test]
        public void RoundToInt_RoundsHalfUp()
        {
            Assert.AreEqual(2, Fix.FromMilli(1500).RoundToInt());
            Assert.AreEqual(1, Fix.FromMilli(1499).RoundToInt());
            Assert.AreEqual(3, Fix.FromMilli(2500).RoundToInt());
            Assert.AreEqual(-1, Fix.FromMilli(-1500).RoundToInt());
            Assert.AreEqual(-2, Fix.FromMilli(-1501).RoundToInt());
        }

        [Test]
        public void ToMilli_RoundTripsWholeMillimetres()
        {
            foreach (int milli in new[] { 1500, -250, 1, -1, 123 })
            {
                Assert.AreEqual(milli, Fix.FromMilli(milli).ToMilli(), milli + " mm");
            }
            Assert.AreEqual(0, Fix.FromRaw(1).ToMilli());
            Assert.AreEqual(0, Fix.FromRaw(-1).ToMilli());
        }

        [Test]
        public void ToString_NeverPrintsMinusZero()
        {
            Assert.AreEqual("0.000", Fix.FromRaw(1).ToString());
            Assert.AreEqual("0.000", Fix.FromRaw(-1).ToString());
        }
    }
}
