#nullable enable
namespace RFL.Sim
{
    /// <summary>
    /// The match's one seeded random generator: PCG32 (XSH-RR, 64-bit state, fixed stream).
    /// A plain value type so it sits inside <c>MatchState</c> and is copied, hashed and
    /// replayed with it. Same seed, same sequence, on every machine.
    /// </summary>
    /// <remarks>
    /// The core uses no engine or framework random source, only this one. Methods mutate
    /// the state, so callers hold the generator by reference (a field of the match state),
    /// never a copy.
    /// </remarks>
    public struct SeededRandom
    {
        private const ulong Multiplier = 6364136223846793005UL;
        private const ulong Increment = 1442695040888963407UL;

        public ulong State;

        public static SeededRandom FromSeed(ulong seed)
        {
            var random = new SeededRandom { State = 0 };
            random.Step();
            random.State += seed;
            random.Step();
            return random;
        }

        /// <summary>Next 32-bit value; uniform over the whole uint range.</summary>
        public uint NextUInt()
        {
            ulong old = State;
            Step();
            uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            int rotation = (int)(old >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }

        /// <summary>
        /// Uniform integer in [minInclusive, maxExclusive), without modulo bias (rejection
        /// below the threshold). Returns minInclusive when the range is empty.
        /// </summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            uint bound = (uint)((long)maxExclusive - minInclusive);
            uint threshold = (uint)(0x1_0000_0000L - bound) % bound;
            uint value;
            do
            {
                value = NextUInt();
            } while (value < threshold);

            return (int)(minInclusive + (long)(value % bound));
        }

        private void Step()
        {
            State = unchecked(State * Multiplier + Increment);
        }
    }
}
