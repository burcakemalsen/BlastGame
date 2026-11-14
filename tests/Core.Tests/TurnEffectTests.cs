using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class TurnEffectTests
    {
        private static List<ClearedCell> OneCleared()
        {
            return new List<ClearedCell> { new ClearedCell(new CellPos(2, 1), Cell.OfSpecial(SpecialKind.Bomb)) };
        }

        [Fact]
        public void Ctor_AllArguments_StoredAndListNotCopied()
        {
            var cleared = OneCleared();
            Cell created = Cell.OfOrb(TileColor.Green);

            var e = new TurnEffect(EffectKind.Group, new CellPos(2, 1), cleared, created, -1);

            Assert.Equal(EffectKind.Group, e.Kind);
            Assert.Equal(new CellPos(2, 1), e.Origin);
            Assert.Same(cleared, e.Cleared);
            Assert.True(created == e.Created);
            Assert.Equal(-1, e.TriggeredBy);
        }

        [Fact]
        public void Ctor_NullCleared_ThrowsArgumentNullWithName()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new TurnEffect(EffectKind.Rocket, new CellPos(0, 0), null, Cell.Empty, -1));
            Assert.Equal("cleared", ex.ParamName);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(int.MinValue)]
        public void Ctor_TriggeredByBelowMinusOne_ThrowsOutOfRangeWithName(int triggeredBy)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => new TurnEffect(EffectKind.Bomb, new CellPos(0, 0), OneCleared(), Cell.Empty, triggeredBy));
            Assert.Equal("triggeredBy", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Ctor_TriggeredByMinusOneOrMore_Accepted(int triggeredBy)
        {
            var e = new TurnEffect(EffectKind.Bomb, new CellPos(0, 0), OneCleared(), Cell.Empty, triggeredBy);
            Assert.Equal(triggeredBy, e.TriggeredBy);
        }

        [Fact]
        public void Ctor_EmptyClearedAndMismatchedCreated_NotChecked()
        {
            var empty = new List<ClearedCell>();

            var e = new TurnEffect(EffectKind.Rocket, new CellPos(1, 1), empty, Cell.OfSpecial(SpecialKind.Bomb), 3);

            Assert.Empty(e.Cleared);
            Assert.Equal(EffectKind.Rocket, e.Kind);
            Assert.True(Cell.OfSpecial(SpecialKind.Bomb) == e.Created);
        }
    }

    public class TurnResultEffectsTests
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

        private static List<TurnEffect> OneEffect()
        {
            var cleared = new List<ClearedCell> { new ClearedCell(new CellPos(0, 0), Cell.OfColor(TileColor.Red)) };
            return new List<TurnEffect> { new TurnEffect(EffectKind.Group, new CellPos(0, 0), cleared, Cell.Empty, -1) };
        }

        [Fact]
        public void Ctor4_Lists_StoredWithoutCopy()
        {
            var blasted = Cells(0, 0);
            var moves = new List<CellMove>();
            var filled = Cells(0, 0);
            var effects = OneEffect();

            var result = new TurnResult(blasted, moves, filled, effects);

            Assert.Same(blasted, result.Blasted);
            Assert.Same(moves, result.Moves);
            Assert.Same(filled, result.Filled);
            Assert.Same(effects, result.Effects);
            Assert.True(result.HasBlasted);
        }

        [Fact]
        public void Ctor4_NullEffects_ThrowsArgumentNullWithName()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new TurnResult(Cells(), new List<CellMove>(), Cells(), null));
            Assert.Equal("effects", ex.ParamName);
        }

        // Null checks in order blasted, moves, filled, effects: with effects also null,
        // the earlier list is named.
        [Theory]
        [InlineData(0, "blasted")]
        [InlineData(1, "moves")]
        [InlineData(2, "filled")]
        public void Ctor4_EarlierListNullToo_EarlierNameReported(int nullIndex, string expected)
        {
            List<CellPos> blasted = nullIndex == 0 ? null : Cells();
            List<CellMove> moves = nullIndex == 1 ? null : new List<CellMove>();
            List<CellPos> filled = nullIndex == 2 ? null : Cells();

            var ex = Assert.Throws<ArgumentNullException>(() => new TurnResult(blasted, moves, filled, null));
            Assert.Equal(expected, ex.ParamName);
        }

        [Fact]
        public void Ctor4_BlastedNotMatchingEffects_NotChecked()
        {
            var result = new TurnResult(Cells(), new List<CellMove>(), Cells(), OneEffect());

            Assert.False(result.HasBlasted);
            Assert.Single(result.Effects);
        }

        // Passes against the stub as well: the old constructor was already implemented.
        [Fact]
        public void Ctor3_Effects_EmptyNotNull()
        {
            var result = new TurnResult(Cells(1, 0), new List<CellMove>(), Cells(1, 0));

            Assert.NotNull(result.Effects);
            Assert.Empty(result.Effects);
        }
    }
}
