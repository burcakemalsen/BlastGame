using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class SeededRandomTests
    {
        // ---- Construction ----

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(42)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void Constructor_AnyIntSeed_DoesNotThrowAndCanDraw(int seed)
        {
            var ex = Record.Exception(() =>
            {
                var random = new SeededRandom(seed);
                random.Next(10);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void SeededRandom_ImplementsIRandomSource()
        {
            IRandomSource source = new SeededRandom(42);

            int value = source.Next(6);

            Assert.InRange(value, 0, 5);
        }

        // ---- Determinism ----

        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void Next_SameSeed_SameFirst100Values(int seed)
        {
            var a = new SeededRandom(seed);
            var b = new SeededRandom(seed);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(a.Next(int.MaxValue), b.Next(int.MaxValue));
            }
        }

        [Fact]
        public void Next_SameSeedWithSmallMax_SameFirst100Values()
        {
            var a = new SeededRandom(7);
            var b = new SeededRandom(7);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(a.Next(6), b.Next(6));
            }
        }

        [Fact]
        public void Next_Seeds1And2_First10ValuesDiffer()
        {
            List<int> first = Draw(new SeededRandom(1), 10, int.MaxValue);
            List<int> second = Draw(new SeededRandom(2), 10, int.MaxValue);

            Assert.NotEqual(first, second);
        }

        // ---- Range ----

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Next_MaxZeroOrNegative_ThrowsArgumentOutOfRange(int maxExclusive)
        {
            var random = new SeededRandom(42);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.Next(maxExclusive));
        }

        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Next_One_AlwaysZero(int seed)
        {
            var random = new SeededRandom(seed);

            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(0, random.Next(1));
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void Next_ManyDraws_StayWithinZeroAndMaxExclusive(int maxExclusive)
        {
            var random = new SeededRandom(12345);

            for (int i = 0; i < 2000; i++)
            {
                int value = random.Next(maxExclusive);
                Assert.True(value >= 0 && value < maxExclusive,
                    $"Draw {i} returned {value}, expected [0, {maxExclusive})");
            }
        }

        [Fact]
        public void Next_Two_ProducesBothValues()
        {
            var random = new SeededRandom(42);
            var seen = new HashSet<int>();

            for (int i = 0; i < 200; i++)
            {
                seen.Add(random.Next(2));
            }

            Assert.Contains(0, seen);
            Assert.Contains(1, seen);
        }

        // ---- Rough uniformity ----

        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void Next_Six_6000Draws_EachValueAtLeast800Times(int seed)
        {
            var random = new SeededRandom(seed);
            var counts = new int[6];

            for (int i = 0; i < 6000; i++)
            {
                counts[random.Next(6)]++;
            }

            for (int v = 0; v < 6; v++)
            {
                Assert.True(counts[v] >= 800, $"Value {v} came {counts[v]} times, expected at least 800");
            }
        }

        // ---- SplitMix64 reference ----
        // Raw SplitMix64 outputs for state 0 (standard constants):
        //   0xE220A8397B1DCDAF, 0x6E789E6AA1B965F4, 0x06C45D188009454F
        // Next(max) = (int)(output % (ulong)max), so with max = int.MaxValue
        // the expected values are those outputs modulo 2147483647.

        [Fact]
        public void Next_Seed0_MatchesSplitMix64ReferenceOutputs()
        {
            var random = new SeededRandom(0);

            Assert.Equal(Mod(0xE220A8397B1DCDAFUL, int.MaxValue), random.Next(int.MaxValue));
            Assert.Equal(Mod(0x6E789E6AA1B965F4UL, int.MaxValue), random.Next(int.MaxValue));
            Assert.Equal(Mod(0x06C45D188009454FUL, int.MaxValue), random.Next(int.MaxValue));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(6)]
        [InlineData(1000)]
        [InlineData(65536)]
        [InlineData(1000000007)]
        public void Next_Seed0_FirstValueIsReferenceOutputModuloMax(int maxExclusive)
        {
            var random = new SeededRandom(0);

            Assert.Equal(Mod(0xE220A8397B1DCDAFUL, maxExclusive), random.Next(maxExclusive));
        }

        [Fact]
        public void Next_DifferentMaxPerCall_EachCallConsumesOneOutput()
        {
            var random = new SeededRandom(0);

            Assert.Equal(Mod(0xE220A8397B1DCDAFUL, 6), random.Next(6));
            Assert.Equal(Mod(0x6E789E6AA1B965F4UL, int.MaxValue), random.Next(int.MaxValue));
            Assert.Equal(Mod(0x06C45D188009454FUL, 1000), random.Next(1000));
        }

        // Seed -1 must become state 0xFFFFFFFFFFFFFFFF ((ulong)(long)seed, sign-extended).
        // Expected outputs computed with the standard SplitMix64 step from that state.
        [Fact]
        public void Next_SeedMinusOne_UsesSignExtendedInitialState()
        {
            var random = new SeededRandom(-1);

            Assert.Equal(Mod(0xE4D971771B652C20UL, int.MaxValue), random.Next(int.MaxValue));
            Assert.Equal(Mod(0xE99FF867DBF682C9UL, int.MaxValue), random.Next(int.MaxValue));
            Assert.Equal(Mod(0x382FF84CB27281E9UL, int.MaxValue), random.Next(int.MaxValue));
        }

        // ---- Helpers ----

        private static int Mod(ulong value, int max)
        {
            return (int)(value % (ulong)max);
        }

        private static List<int> Draw(SeededRandom random, int count, int maxExclusive)
        {
            var values = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(random.Next(maxExclusive));
            }

            return values;
        }
    }
}
