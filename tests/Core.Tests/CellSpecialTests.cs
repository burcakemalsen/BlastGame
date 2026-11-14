using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class CellSpecialTests
    {
        // 0 empty, 1 colored red, 2 rocket H, 3 rocket V, 4 bomb, 5 red orb
        private static Cell Sample(int index)
        {
            switch (index)
            {
                case 0: return Cell.Empty;
                case 1: return Cell.OfColor(TileColor.Red);
                case 2: return Cell.OfSpecial(SpecialKind.RocketHorizontal);
                case 3: return Cell.OfSpecial(SpecialKind.RocketVertical);
                case 4: return Cell.OfSpecial(SpecialKind.Bomb);
                default: return Cell.OfOrb(TileColor.Red);
            }
        }

        // ---------- creation ----------

        [Theory]
        [InlineData(SpecialKind.RocketHorizontal)]
        [InlineData(SpecialKind.RocketVertical)]
        [InlineData(SpecialKind.Bomb)]
        public void OfSpecial_NonOrbKind_IsSpecialWithThatKind(SpecialKind kind)
        {
            Cell cell = Cell.OfSpecial(kind);

            Assert.Equal(CellKind.Special, cell.Kind);
            Assert.True(cell.IsSpecial);
            Assert.False(cell.IsEmpty);
            Assert.Equal(kind, cell.Special);
        }

        [Fact]
        public void OfSpecial_Orb_ThrowsArgumentExceptionWithKindName()
        {
            var ex = Assert.Throws<ArgumentException>(() => Cell.OfSpecial(SpecialKind.Orb));
            Assert.Equal("kind", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        public void OfSpecial_UndefinedKind_ThrowsOutOfRangeWithKindName(int kind)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Cell.OfSpecial((SpecialKind)kind));
            Assert.Equal("kind", ex.ParamName);
        }

        [Theory]
        [InlineData(TileColor.Red)]
        [InlineData(TileColor.Orange)]
        public void OfOrb_DefinedColor_SpecialOrbWithColor(TileColor color)
        {
            Cell cell = Cell.OfOrb(color);

            Assert.Equal(CellKind.Special, cell.Kind);
            Assert.True(cell.IsSpecial);
            Assert.False(cell.IsEmpty);
            Assert.Equal(SpecialKind.Orb, cell.Special);
            Assert.Equal(color, cell.OrbColor);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(6)]
        public void OfOrb_UndefinedColor_ThrowsOutOfRangeWithColorName(int color)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Cell.OfOrb((TileColor)color));
            Assert.Equal("color", ex.ParamName);
        }

        // ---------- accessors on the wrong kind ----------

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void IsSpecial_EmptyOrColored_False(int index)
        {
            Assert.False(Sample(index).IsSpecial);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Special_NotSpecial_ThrowsInvalidOperation(int index)
        {
            Cell cell = Sample(index);
            Assert.Throws<InvalidOperationException>(() => cell.Special);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        public void OrbColor_NotOrb_ThrowsInvalidOperation(int index)
        {
            Cell cell = Sample(index);
            Assert.Throws<InvalidOperationException>(() => cell.OrbColor);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        public void Color_OnSpecial_ThrowsInvalidOperation(int index)
        {
            Cell cell = Sample(index);
            Assert.Throws<InvalidOperationException>(() => cell.Color);
        }

        // ---------- equality ----------

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(5)]
        public void Equals_SameSpecial_EqualWithSameHash(int index)
        {
            Cell a = Sample(index);
            Cell b = Sample(index);

            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Theory]
        [InlineData(2, 3)]   // rocket direction differs
        [InlineData(2, 4)]   // rocket vs bomb
        [InlineData(4, 5)]   // bomb vs orb
        [InlineData(4, 0)]   // bomb vs empty
        [InlineData(5, 1)]   // red orb vs red colored cell
        public void Equals_DifferentKindOrSpecial_NotEqual(int left, int right)
        {
            Cell a = Sample(left);
            Cell b = Sample(right);

            Assert.False(a.Equals(b));
            Assert.False(a.Equals((object)b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_OrbDifferentColors_NotEqual()
        {
            Cell red = Cell.OfOrb(TileColor.Red);
            Cell blue = Cell.OfOrb(TileColor.Blue);

            Assert.False(red == blue);
            Assert.False(red.Equals(blue));
        }

        // ---------- ToString ----------

        [Fact]
        public void ToString_Bomb_SpecialBomb()
        {
            Assert.Equal("Special(Bomb)", Cell.OfSpecial(SpecialKind.Bomb).ToString());
        }

        [Fact]
        public void ToString_Orb_SpecialOrbWithColor()
        {
            Assert.Equal("Special(Orb, Red)", Cell.OfOrb(TileColor.Red).ToString());
        }

        [Theory]
        [InlineData(SpecialKind.RocketHorizontal, "Special(RocketHorizontal)")]
        [InlineData(SpecialKind.RocketVertical, "Special(RocketVertical)")]
        public void ToString_Rocket_SpecialWithKindName(SpecialKind kind, string expected)
        {
            Assert.Equal(expected, Cell.OfSpecial(kind).ToString());
        }
    }
}
