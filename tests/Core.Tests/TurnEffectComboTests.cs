using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>TurnEffect's combo constructor and the new members (spec 06b, interface contract).</summary>
    public class TurnEffectComboTests
    {
        private static List<ClearedCell> Cells(int x, int y, Cell cell)
        {
            return new List<ClearedCell> { new ClearedCell(new CellPos(x, y), cell) };
        }

        [Fact]
        public void EffectKind_NewKindsAppendedInOrder()
        {
            Assert.Equal(
                new[] { "Group", "Rocket", "Bomb", "Orb", "RocketCross", "RocketBomb", "BigBomb", "OrbRocket", "OrbBomb", "OrbOrb" },
                Enum.GetNames(typeof(EffectKind)));
            Assert.Equal(0, (int)EffectKind.Group);
            Assert.Equal(9, (int)EffectKind.OrbOrb);
        }

        [Fact]
        public void Ctor7_AllArguments_StoredListsNotCopied()
        {
            var cleared = Cells(1, 1, Cell.OfOrb(TileColor.Red));
            var converted = Cells(0, 0, Cell.OfColor(TileColor.Red));

            var e = new TurnEffect(EffectKind.OrbBomb, new CellPos(1, 1), cleared, Cell.Empty, -1, 3, converted);

            Assert.Equal(EffectKind.OrbBomb, e.Kind);
            Assert.Equal(new CellPos(1, 1), e.Origin);
            Assert.Same(cleared, e.Cleared);
            Assert.True(Cell.Empty == e.Created);
            Assert.Equal(-1, e.TriggeredBy);
            Assert.Equal(3, e.MergedCount);
            Assert.Same(converted, e.Converted);
        }

        [Fact]
        public void Ctor7_NullCleared_ThrowsArgumentNullCleared()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new TurnEffect(EffectKind.OrbBomb, new CellPos(0, 0), null, Cell.Empty, -1, 2, new List<ClearedCell>()));
            Assert.Equal("cleared", ex.ParamName);
        }

        [Fact]
        public void Ctor7_NullConverted_ThrowsArgumentNullConverted()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new TurnEffect(EffectKind.OrbBomb, new CellPos(0, 0), new List<ClearedCell>(), Cell.Empty, -1, 2, null));
            Assert.Equal("converted", ex.ParamName);
        }

        [Fact]
        public void Ctor7_BothListsNull_ClearedReportedFirst()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new TurnEffect(EffectKind.OrbBomb, new CellPos(0, 0), null, Cell.Empty, -1, 2, null));
            Assert.Equal("cleared", ex.ParamName);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(int.MinValue)]
        public void Ctor7_TriggeredByBelowMinusOne_ThrowsOutOfRangeTriggeredBy(int triggeredBy)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TurnEffect(EffectKind.RocketCross, new CellPos(0, 0), new List<ClearedCell>(), Cell.Empty,
                               triggeredBy, 2, new List<ClearedCell>()));
            Assert.Equal("triggeredBy", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Ctor7_MergedCountNegative_ThrowsOutOfRangeMergedCount(int mergedCount)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TurnEffect(EffectKind.RocketCross, new CellPos(0, 0), new List<ClearedCell>(), Cell.Empty,
                               -1, mergedCount, new List<ClearedCell>()));
            Assert.Equal("mergedCount", ex.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Ctor7_MergedCountZeroOrMore_Accepted(int mergedCount)
        {
            var e = new TurnEffect(EffectKind.BigBomb, new CellPos(0, 0), new List<ClearedCell>(), Cell.Empty,
                                   0, mergedCount, new List<ClearedCell>());
            Assert.Equal(mergedCount, e.MergedCount);
            Assert.Equal(0, e.TriggeredBy);
        }

        // Clarification: kind is not checked against mergedCount or converted.
        [Fact]
        public void Ctor7_KindMismatchedWithCountAndConverted_NotChecked()
        {
            var converted = Cells(2, 2, Cell.OfColor(TileColor.Blue));

            var e = new TurnEffect(EffectKind.Group, new CellPos(0, 0), new List<ClearedCell>(), Cell.Empty, -1, 7, converted);

            Assert.Equal(EffectKind.Group, e.Kind);
            Assert.Equal(7, e.MergedCount);
            Assert.Same(converted, e.Converted);
        }

        [Theory]
        [InlineData(EffectKind.Group, 0)]
        [InlineData(EffectKind.Rocket, 1)]
        [InlineData(EffectKind.OrbOrb, 1)]
        public void Ctor5_MergedCountByKindConvertedEmpty(EffectKind kind, int merged)
        {
            var e = new TurnEffect(kind, new CellPos(0, 0), new List<ClearedCell>(), Cell.Empty, -1);

            Assert.Equal(merged, e.MergedCount);
            Assert.NotNull(e.Converted);
            Assert.Empty(e.Converted);
        }
    }
}
