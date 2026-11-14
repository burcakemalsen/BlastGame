using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>Tests for the TestGrid helper itself (row order, letters).</summary>
    public class GridBuilderTests
    {
        [Fact]
        public void Parse_TwoRows_FirstRowIsTopAndLastRowIsYZero()
        {
            Grid grid = TestGrid.Parse("RG", "BY");

            Assert.Equal(2, grid.Width);
            Assert.Equal(2, grid.Height);
            Assert.Equal(Cell.OfColor(TileColor.Blue), grid.Get(0, 0));
            Assert.Equal(Cell.OfColor(TileColor.Yellow), grid.Get(1, 0));
            Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(0, 1));
            Assert.Equal(Cell.OfColor(TileColor.Green), grid.Get(1, 1));
        }

        [Fact]
        public void Parse_ThreeWideTwoTall_WidthIsRowLengthAndHeightIsRowCount()
        {
            Grid grid = TestGrid.Parse("RGB", "OPY");

            Assert.Equal(3, grid.Width);
            Assert.Equal(2, grid.Height);
            Assert.Equal(Cell.OfColor(TileColor.Orange), grid.Get(0, 0));
            Assert.Equal(Cell.OfColor(TileColor.Purple), grid.Get(1, 0));
            Assert.Equal(Cell.OfColor(TileColor.Yellow), grid.Get(2, 0));
            Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(0, 1));
            Assert.Equal(Cell.OfColor(TileColor.Green), grid.Get(1, 1));
            Assert.Equal(Cell.OfColor(TileColor.Blue), grid.Get(2, 1));
        }

        [Fact]
        public void Parse_OneWideThreeTall_WidthOneHeightThree()
        {
            Grid grid = TestGrid.Parse("R", "G", "B");

            Assert.Equal(1, grid.Width);
            Assert.Equal(3, grid.Height);
            Assert.Equal(Cell.OfColor(TileColor.Blue), grid.Get(0, 0));
            Assert.Equal(Cell.OfColor(TileColor.Green), grid.Get(0, 1));
            Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(0, 2));
        }

        [Fact]
        public void Parse_Dot_IsEmptyCell()
        {
            Grid grid = TestGrid.Parse("R.", ".G");

            Assert.Equal(Cell.Empty, grid.Get(1, 1));
            Assert.Equal(Cell.Empty, grid.Get(0, 0));
            Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(0, 1));
            Assert.Equal(Cell.OfColor(TileColor.Green), grid.Get(1, 0));
        }

        [Fact]
        public void Dump_AfterParse_ReturnsSameRows()
        {
            string[] rows = { "RG.", "BYP", ".OR" };

            Assert.Equal(rows, TestGrid.Dump(TestGrid.Parse(rows)));
        }

        [Fact]
        public void Dump_GridBuiltWithSet_PutsYZeroInLastRow()
        {
            var grid = new Grid(2, 3);
            grid.Set(0, 0, Cell.OfColor(TileColor.Blue));
            grid.Set(1, 2, Cell.OfColor(TileColor.Red));

            Assert.Equal(new[] { ".R", "..", "B." }, TestGrid.Dump(grid));
        }

        [Fact]
        public void Parse_NoRows_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => TestGrid.Parse());
        }

        [Fact]
        public void Parse_RaggedRows_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => TestGrid.Parse("RG", "R"));
        }

        [Fact]
        public void Parse_UnknownLetter_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => TestGrid.Parse("RX"));
        }
    }
}
