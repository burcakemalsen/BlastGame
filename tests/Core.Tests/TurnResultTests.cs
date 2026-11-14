using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class TurnResultTests
    {
        private static List<CellPos> Cells(params int[] xy)
        {
            var list = new List<CellPos>();
            for (int i = 0; i + 1 < xy.Length; i += 2)
            {
                list.Add(new CellPos(xy[i], xy[i + 1]));
            }

            return list;
        }

        [Fact]
        public void Constructor_Lists_StoredWithoutCopy()
        {
            var blasted = Cells(1, 0, 0, 1);
            var moves = new List<CellMove> { new CellMove(new CellPos(0, 2), new CellPos(0, 1)) };
            var filled = Cells(0, 2);

            var result = new TurnResult(blasted, moves, filled);

            Assert.Same(blasted, result.Blasted);
            Assert.Same(moves, result.Moves);
            Assert.Same(filled, result.Filled);
        }

        [Fact]
        public void Constructor_NullBlasted_ThrowsWithBlastedName()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new TurnResult(null, new List<CellMove>(), Cells()));
            Assert.Equal("blasted", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullMoves_ThrowsWithMovesName()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new TurnResult(Cells(), null, Cells()));
            Assert.Equal("moves", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullFilled_ThrowsWithFilledName()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new TurnResult(Cells(), new List<CellMove>(), null));
            Assert.Equal("filled", ex.ParamName);
        }

        [Fact]
        public void Constructor_AllNull_ReportsBlastedFirst()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TurnResult(null, null, null));
            Assert.Equal("blasted", ex.ParamName);
        }

        [Fact]
        public void Constructor_MovesAndFilledNull_ReportsMovesFirst()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TurnResult(Cells(), null, null));
            Assert.Equal("moves", ex.ParamName);
        }

        [Fact]
        public void HasBlasted_EmptyBlasted_False()
        {
            Assert.False(new TurnResult(Cells(), new List<CellMove>(), Cells()).HasBlasted);
        }

        [Fact]
        public void HasBlasted_EmptyBlastedButOtherListsFilled_False()
        {
            var moves = new List<CellMove> { new CellMove(new CellPos(0, 1), new CellPos(0, 0)) };
            Assert.False(new TurnResult(Cells(), moves, Cells(0, 1)).HasBlasted);
        }

        [Fact]
        public void HasBlasted_OneBlastedCell_True()
        {
            Assert.True(new TurnResult(Cells(0, 0), new List<CellMove>(), Cells()).HasBlasted);
        }
    }
}
