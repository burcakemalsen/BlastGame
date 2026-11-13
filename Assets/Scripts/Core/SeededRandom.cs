using System;

namespace BlastGame.Core
{
    /// <summary>Deterministic random source (SplitMix64).</summary>
    public sealed class SeededRandom : IRandomSource
    {
        private ulong _state;

        public SeededRandom(int seed)
        {
            _state = unchecked((ulong)(long)seed);
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            return (int)(NextUInt64() % (ulong)maxExclusive);
        }

        private ulong NextUInt64()
        {
            unchecked
            {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}
