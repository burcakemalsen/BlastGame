using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class CellTests
    {
        private static readonly TileColor[] AllColors =
        {
            TileColor.Red, TileColor.Green, TileColor.Blue,
            TileColor.Yellow, TileColor.Purple, TileColor.Orange
        };

        // Empty plus the six colored cells: 7 distinct values.
        private static Cell[] AllCells()
        {
            var cells = new List<Cell> { Cell.Empty };
            foreach (TileColor color in AllColors)
            {
                cells.Add(Cell.OfColor(color));
            }

            return cells.ToArray();
        }

        // ---- Empty ----

        [Fact]
        public void Empty_Kind_IsEmpty()
        {
            Assert.Equal(CellKind.Empty, Cell.Empty.Kind);
        }

        [Fact]
        public void Empty_IsEmpty_ReturnsTrue()
        {
            Assert.True(Cell.Empty.IsEmpty);
        }

        [Fact]
        public void Default_KindIsEmptyAndIsEmptyTrue()
        {
            Cell cell = default;

            Assert.Equal(CellKind.Empty, cell.Kind);
            Assert.True(cell.IsEmpty);
        }

        [Fact]
        public void CellKind_EmptyIsZero()
        {
            Assert.Equal(0, (int)CellKind.Empty);
        }

        [Fact]
        public void Empty_EqualsDefault()
        {
            Assert.True(Cell.Empty.Equals(default(Cell)));
            Assert.True(Cell.Empty == default(Cell));
            Assert.False(Cell.Empty != default(Cell));
        }

        [Fact]
        public void Empty_Color_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() => Cell.Empty.Color);
        }

        [Fact]
        public void Default_Color_ThrowsInvalidOperation()
        {
            Cell cell = default;

            Assert.Throws<InvalidOperationException>(() => cell.Color);
        }

        // ---- OfColor ----

        [Theory]
        [InlineData(TileColor.Red)]
        [InlineData(TileColor.Green)]
        [InlineData(TileColor.Blue)]
        [InlineData(TileColor.Yellow)]
        [InlineData(TileColor.Purple)]
        [InlineData(TileColor.Orange)]
        public void OfColor_DefinedColor_KindColorAndColorMatch(TileColor color)
        {
            Cell cell = Cell.OfColor(color);

            Assert.Equal(CellKind.Color, cell.Kind);
            Assert.Equal(color, cell.Color);
            Assert.False(cell.IsEmpty);
        }

        [Theory]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(99)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void OfColor_UndefinedColor_ThrowsArgumentOutOfRange(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Cell.OfColor((TileColor)value));
        }

        // ---- Equality ----

        [Fact]
        public void Equals_RedAndEmpty_AreNotEqualInBothDirections()
        {
            Cell red = Cell.OfColor(TileColor.Red);

            Assert.False(red.Equals(Cell.Empty));
            Assert.False(Cell.Empty.Equals(red));
            Assert.False(red == Cell.Empty);
            Assert.False(Cell.Empty == red);
            Assert.True(red != Cell.Empty);
            Assert.True(Cell.Empty != red);
            Assert.False(red.Equals((object)Cell.Empty));
            Assert.False(((object)Cell.Empty).Equals(red));
        }

        [Fact]
        public void Equals_RedAndDefault_AreNotEqual()
        {
            Assert.NotEqual(Cell.OfColor(TileColor.Red), default(Cell));
        }

        [Theory]
        [InlineData(TileColor.Red)]
        [InlineData(TileColor.Green)]
        [InlineData(TileColor.Blue)]
        [InlineData(TileColor.Yellow)]
        [InlineData(TileColor.Purple)]
        [InlineData(TileColor.Orange)]
        public void Equals_SameColorBuiltTwice_AreEqual(TileColor color)
        {
            Cell a = Cell.OfColor(color);
            Cell b = Cell.OfColor(color);

            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a.Equals((object)b));
        }

        [Fact]
        public void Equals_EveryPairOfDifferentColors_AreNotEqual()
        {
            foreach (TileColor c1 in AllColors)
            {
                foreach (TileColor c2 in AllColors)
                {
                    if (c1 == c2)
                    {
                        continue;
                    }

                    Cell a = Cell.OfColor(c1);
                    Cell b = Cell.OfColor(c2);
                    Assert.False(a.Equals(b), a + " vs " + b);
                    Assert.False(a == b, a + " vs " + b);
                    Assert.True(a != b, a + " vs " + b);
                }
            }
        }

        [Fact]
        public void EqualityMembers_AllPairsOfSevenValues_AreConsistent()
        {
            Cell[] cells = AllCells();

            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells.Length; j++)
                {
                    bool expected = i == j;
                    Cell a = cells[i];
                    Cell b = cells[j];
                    string pair = a + " vs " + b;

                    Assert.True(expected == a.Equals(b), "Equals(Cell) " + pair);
                    Assert.True(expected == a.Equals((object)b), "Equals(object) " + pair);
                    Assert.True(expected == (a == b), "== " + pair);
                    Assert.True(expected != (a != b), "!= " + pair);
                    if (expected)
                    {
                        Assert.True(a.GetHashCode() == b.GetHashCode(), "GetHashCode " + pair);
                    }
                }
            }
        }

        [Fact]
        public void EqualsObject_Null_ReturnsFalse()
        {
            Assert.False(Cell.OfColor(TileColor.Red).Equals((object)null));
            Assert.False(Cell.Empty.Equals((object)null));
        }

        [Fact]
        public void EqualsObject_OtherTypes_ReturnFalse()
        {
            Assert.False(Cell.OfColor(TileColor.Red).Equals(TileColor.Red));
            Assert.False(Cell.OfColor(TileColor.Red).Equals("Red"));
            Assert.False(Cell.Empty.Equals(0));
            Assert.False(Cell.Empty.Equals(CellKind.Empty));
        }

        [Fact]
        public void GetHashCode_EmptyAndDefault_AreEqual()
        {
            Assert.Equal(Cell.Empty.GetHashCode(), default(Cell).GetHashCode());
        }

        [Fact]
        public void GetHashCode_SameCellCalledTwice_IsStable()
        {
            Cell cell = Cell.OfColor(TileColor.Blue);

            Assert.Equal(cell.GetHashCode(), cell.GetHashCode());
        }

        [Fact]
        public void HashSet_SevenDistinctValuesStaySeven_DuplicatesCollapse()
        {
            var set = new HashSet<Cell>(AllCells());
            set.Add(Cell.Empty);
            set.Add(default);
            set.Add(Cell.OfColor(TileColor.Green));

            Assert.Equal(7, set.Count);
        }

        // ---- Value semantics ----

        [Fact]
        public void Copy_OfCell_IsEqualToOriginal()
        {
            Cell original = Cell.OfColor(TileColor.Purple);
            Cell copy = original;

            Assert.Equal(original, copy);
            Assert.Equal(TileColor.Purple, copy.Color);
        }

        // ---- ToString ----

        [Fact]
        public void ToString_Empty_IsEmpty()
        {
            Assert.Equal("Empty", Cell.Empty.ToString());
            Assert.Equal("Empty", default(Cell).ToString());
        }

        [Theory]
        [InlineData(TileColor.Red, "Color(Red)")]
        [InlineData(TileColor.Green, "Color(Green)")]
        [InlineData(TileColor.Blue, "Color(Blue)")]
        [InlineData(TileColor.Yellow, "Color(Yellow)")]
        [InlineData(TileColor.Purple, "Color(Purple)")]
        [InlineData(TileColor.Orange, "Color(Orange)")]
        public void ToString_Colored_IsColorWithName(TileColor color, string expected)
        {
            Assert.Equal(expected, Cell.OfColor(color).ToString());
        }
    }
}
