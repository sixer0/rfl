#nullable enable
namespace RFL.Sim
{
    /// <summary>
    /// FNV-1a 64-bit hash over little-endian value bytes, for state snapshots: the
    /// determinism guard compares one hash per run, and replays check theirs per tick.
    /// Feed every field in a fixed order; the hash is order-sensitive by design.
    /// </summary>
    public struct StateHasher
    {
        public const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        private ulong _value;
        private bool _started;

        /// <summary>The hash so far; the offset basis when nothing was added.</summary>
        public ulong Value => _started ? _value : OffsetBasis;

        public void Add(long value)
        {
            if (!_started)
            {
                _value = OffsetBasis;
                _started = true;
            }

            for (int i = 0; i < 8; i++)
            {
                _value ^= (byte)(value >> (i * 8));
                _value = unchecked(_value * Prime);
            }
        }

        public void Add(int value) => Add((long)value);

        public void Add(bool value) => Add(value ? 1L : 0L);

        public void Add(Fix value) => Add(value.Raw);
    }
}
