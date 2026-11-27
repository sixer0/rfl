#nullable enable
using System;
using System.Globalization;

namespace RFL.Sim
{
    /// <summary>
    /// Q48.16 fixed-point number: a signed 64-bit integer whose lowest 16 bits hold the
    /// fraction (step 1/65536, about 0.015 mm when one unit is one metre). The only numeric
    /// type in the simulation core; floats are banned there so every machine computes the
    /// same bits.
    /// </summary>
    /// <remarks>
    /// Range: products stay exact while both factors are below about 32,768 in magnitude, and
    /// quotients while the dividend is below about 2^31; far beyond a 9.0 m ring. Overflow
    /// wraps (unchecked), which is still deterministic. Multiplication floors toward negative
    /// infinity (arithmetic shift); division truncates toward zero. Division by zero throws
    /// <see cref="DivideByZeroException"/>.
    /// </remarks>
    public readonly struct Fix : IEquatable<Fix>, IComparable<Fix>
    {
        public const int FractionBits = 16;
        public const long OneRaw = 1L << FractionBits;

        public static readonly Fix Zero = new Fix(0);
        public static readonly Fix One = new Fix(OneRaw);

        public readonly long Raw;

        private Fix(long raw)
        {
            Raw = raw;
        }

        public static Fix FromInt(int value) => new Fix((long)value << FractionBits);

        public static Fix FromRaw(long raw) => new Fix(raw);

        public static Fix operator +(Fix a, Fix b) => new Fix(a.Raw + b.Raw);

        public static Fix operator -(Fix a, Fix b) => new Fix(a.Raw - b.Raw);

        public static Fix operator -(Fix a) => new Fix(-a.Raw);

        public static Fix operator *(Fix a, Fix b) => new Fix((a.Raw * b.Raw) >> FractionBits);

        public static Fix operator /(Fix a, Fix b) => new Fix((a.Raw << FractionBits) / b.Raw);

        public static bool operator ==(Fix a, Fix b) => a.Raw == b.Raw;

        public static bool operator !=(Fix a, Fix b) => a.Raw != b.Raw;

        public static bool operator <(Fix a, Fix b) => a.Raw < b.Raw;

        public static bool operator >(Fix a, Fix b) => a.Raw > b.Raw;

        public static bool operator <=(Fix a, Fix b) => a.Raw <= b.Raw;

        public static bool operator >=(Fix a, Fix b) => a.Raw >= b.Raw;

        public bool Equals(Fix other) => Raw == other.Raw;

        public override bool Equals(object? obj) => obj is Fix other && other.Raw == Raw;

        public override int GetHashCode() => Raw.GetHashCode();

        public int CompareTo(Fix other) => Raw.CompareTo(other.Raw);

        /// <summary>
        /// Culture-free text with exactly three decimals, rounded half away from zero, built
        /// with integer maths only so the debug overlay and logs read the same everywhere.
        /// </summary>
        public override string ToString()
        {
            long half = OneRaw >> 1;
            long milli = Math.Abs((Raw * 1000 + (Raw >= 0 ? half : -half)) / OneRaw);
            string sign = Raw < 0 && milli != 0 ? "-" : "";
            return sign
                + (milli / 1000).ToString(CultureInfo.InvariantCulture)
                + "."
                + (milli % 1000).ToString("000", CultureInfo.InvariantCulture);
        }
    }
}
