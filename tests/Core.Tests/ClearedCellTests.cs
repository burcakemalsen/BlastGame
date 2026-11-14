using Xunit;

namespace BlastGame.Core.Tests
{
    public class ClearedCellTests
    {
        private static readonly Cell Red = Cell.OfColor(TileColor.Red);

        [Fact]
        public void Ctor_PosAndCell_Stored()
        {
            Cell bomb = Cell.OfSpecial(SpecialKind.Bomb);
            var c = new ClearedCell(new CellPos(3, 1), bomb);

            Assert.Equal(new CellPos(3, 1), c.Pos);
            Assert.True(bomb == c.Cell);
        }

        [Fact]
        public void Equals_SamePosAndCell_EqualWithSameHash()
        {
            var a = new ClearedCell(new CellPos(1, 2), Red);
            var b = new ClearedCell(new CellPos(1, 2), Red);

            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equals_DifferentPos_NotEqual()
        {
            var a = new ClearedCell(new CellPos(1, 2), Red);
            var b = new ClearedCell(new CellPos(2, 1), Red);

            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_DifferentCell_NotEqual()
        {
            var a = new ClearedCell(new CellPos(1, 2), Red);
            var b = new ClearedCell(new CellPos(1, 2), Cell.OfOrb(TileColor.Red));

            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_OtherTypeOrNull_False()
        {
            var a = new ClearedCell(new CellPos(1, 2), Red);

            Assert.False(a.Equals(new CellPos(1, 2)));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void ToString_Colored_PosThenCell()
        {
            Assert.Equal("(1, 2) Color(Red)", new ClearedCell(new CellPos(1, 2), Red).ToString());
        }

        [Fact]
        public void ToString_Special_PosThenCell()
        {
            Assert.Equal("(0, 0) Special(Orb, Blue)",
                new ClearedCell(new CellPos(0, 0), Cell.OfOrb(TileColor.Blue)).ToString());
        }
    }
}
