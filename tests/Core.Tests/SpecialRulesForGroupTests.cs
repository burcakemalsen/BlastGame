using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class SpecialRulesForGroupTests
    {
        // Passes against the stub as well (constants are part of the stub).
        [Fact]
        public void Thresholds_AreFiveSevenNine()
        {
            Assert.Equal(5, SpecialRules.RocketMinGroup);
            Assert.Equal(7, SpecialRules.BombMinGroup);
            Assert.Equal(9, SpecialRules.OrbMinGroup);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        public void ForGroup_BelowFive_EmptyAndSourceNotCalled(int size)
        {
            var random = CountingRandom.Constant(0);

            Cell cell = SpecialRules.ForGroup(size, TileColor.Red, random);

            Assert.True(cell.IsEmpty);
            Assert.Equal(0, random.Calls);
        }

        [Theory]
        [InlineData(5, 0, SpecialKind.RocketHorizontal)]
        [InlineData(5, 1, SpecialKind.RocketVertical)]
        [InlineData(6, 0, SpecialKind.RocketHorizontal)]
        [InlineData(6, 1, SpecialKind.RocketVertical)]
        public void ForGroup_FiveOrSix_RocketFromOneNext2Draw(int size, int draw, SpecialKind expected)
        {
            var random = new CountingRandom(draw);

            Cell cell = SpecialRules.ForGroup(size, TileColor.Blue, random);

            Assert.True(Cell.OfSpecial(expected) == cell);
            Assert.Equal(new[] { 2 }, random.MaxArgs);
        }

        [Theory]
        [InlineData(7)]
        [InlineData(8)]
        public void ForGroup_SevenOrEight_BombWithoutDraw(int size)
        {
            var random = CountingRandom.Constant(0);

            Cell cell = SpecialRules.ForGroup(size, TileColor.Green, random);

            Assert.True(Cell.OfSpecial(SpecialKind.Bomb) == cell);
            Assert.Equal(0, random.Calls);
        }

        [Theory]
        [InlineData(9, TileColor.Red)]
        [InlineData(10, TileColor.Orange)]
        [InlineData(256, TileColor.Green)]
        public void ForGroup_NineOrMore_OrbOfGroupColorWithoutDraw(int size, TileColor color)
        {
            var random = CountingRandom.Constant(0);

            Cell cell = SpecialRules.ForGroup(size, color, random);

            Assert.True(Cell.OfOrb(color) == cell);
            Assert.Equal(0, random.Calls);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public void ForGroup_RocketDrawOutOfRange_ThrowsInvalidOperation(int draw)
        {
            var random = new CountingRandom(draw);

            Assert.Throws<InvalidOperationException>(() => SpecialRules.ForGroup(5, TileColor.Red, random));
            Assert.Equal(1, random.Calls);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(9)]
        public void ForGroup_NullRandom_ThrowsArgumentNullAtAnySize(int size)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SpecialRules.ForGroup(size, TileColor.Red, null));
            Assert.Equal("random", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void ForGroup_NegativeSize_ThrowsOutOfRangeWithSizeName(int size)
        {
            var random = CountingRandom.Constant(0);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SpecialRules.ForGroup(size, TileColor.Red, random));
            Assert.Equal("size", ex.ParamName);
            Assert.Equal(0, random.Calls);
        }

        [Theory]
        [InlineData(0, 6)]
        [InlineData(5, -1)]
        [InlineData(9, 6)]
        public void ForGroup_UndefinedColor_ThrowsOutOfRangeAtAnySizeWithoutDraw(int size, int color)
        {
            var random = CountingRandom.Constant(0);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SpecialRules.ForGroup(size, (TileColor)color, random));
            Assert.Equal("color", ex.ParamName);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void ForGroup_NullRandomNegativeSizeBadColor_RandomReportedFirst()
        {
            Assert.Throws<ArgumentNullException>(() => SpecialRules.ForGroup(-1, (TileColor)9, null));
        }

        [Fact]
        public void ForGroup_NegativeSizeAndBadColor_SizeReportedFirst()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => SpecialRules.ForGroup(-1, (TileColor)9, CountingRandom.Constant(0)));
            Assert.Equal("size", ex.ParamName);
        }

        [Fact]
        public void ForGroup_BadColorAndBadDraw_ColorReportedBeforeDrawing()
        {
            var random = CountingRandom.Constant(5);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SpecialRules.ForGroup(6, (TileColor)9, random));
            Assert.Equal("color", ex.ParamName);
            Assert.Equal(0, random.Calls);
        }
    }
}
