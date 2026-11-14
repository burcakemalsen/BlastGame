using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07: ObstacleHit, ObstacleGoal, ObstaclePlacement value types.</summary>
    public class ObstacleValueTypeTests
    {
        private static Cell C2 => Cell.OfObstacle(ObstacleKind.Crate, 2);
        private static Cell C1 => Cell.OfObstacle(ObstacleKind.Crate, 1);

        // ---- ObstacleHit ----

        [Fact]
        public void ObstacleHit_Constructor_KeepsValues()
        {
            var hit = new ObstacleHit(new CellPos(2, 3), C2, C1);

            Assert.Equal(new CellPos(2, 3), hit.Pos);
            Assert.True(hit.Before == C2);
            Assert.True(hit.After == C1);
        }

        [Fact]
        public void ObstacleHit_SameValues_AreEqualWithSameHash()
        {
            var a = new ObstacleHit(new CellPos(1, 1), C2, C1);
            var b = new ObstacleHit(new CellPos(1, 1), C2, C1);

            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.False(a.Equals("hit"));
            Assert.False(string.IsNullOrEmpty(a.ToString()));
        }

        [Fact]
        public void ObstacleHit_AnyFieldDiffers_NotEqual()
        {
            var a = new ObstacleHit(new CellPos(1, 1), C2, C1);

            Assert.True(a != new ObstacleHit(new CellPos(1, 2), C2, C1));
            Assert.True(a != new ObstacleHit(new CellPos(1, 1), Cell.OfObstacle(ObstacleKind.Crate, 3), C1));
            Assert.True(a != new ObstacleHit(new CellPos(1, 1), C2, Cell.Empty));
        }

        // ---- ObstacleGoal ----

        [Fact]
        public void ObstacleGoal_Constructor_KeepsValues()
        {
            var goal = new ObstacleGoal(ObstacleKind.Anchor, 4);

            Assert.Equal(ObstacleKind.Anchor, goal.Kind);
            Assert.Equal(4, goal.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void ObstacleGoal_CountNotValidated_KeepsValue(int count)
        {
            Assert.Equal(count, new ObstacleGoal(ObstacleKind.Crate, count).Count);
        }

        [Fact]
        public void ObstacleGoal_UndefinedKind_ThrowsArgumentOutOfRange()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ObstacleGoal((ObstacleKind)9, 1));
            Assert.Equal("kind", ex.ParamName);
        }

        [Fact]
        public void ObstacleGoal_Equality_ComparesKindAndCount()
        {
            var a = new ObstacleGoal(ObstacleKind.Pinata, 2);

            Assert.True(a == new ObstacleGoal(ObstacleKind.Pinata, 2));
            Assert.Equal(a.GetHashCode(), new ObstacleGoal(ObstacleKind.Pinata, 2).GetHashCode());
            Assert.True(a != new ObstacleGoal(ObstacleKind.Pinata, 3));
            Assert.True(a != new ObstacleGoal(ObstacleKind.Crate, 2));
            Assert.False(a.Equals((object)new ObstacleGoal(ObstacleKind.Crate, 2)));
            Assert.False(string.IsNullOrEmpty(a.ToString()));
        }

        // ---- ObstaclePlacement ----

        [Theory]
        [InlineData(ObstacleKind.Crate, 3)]
        [InlineData(ObstacleKind.WaterBalloon, 1)]
        [InlineData(ObstacleKind.Anchor, 1)]
        public void ObstaclePlacement_ValidLayers_KeepsValues(ObstacleKind kind, int layers)
        {
            var p = new ObstaclePlacement(kind, new CellPos(3, 4), layers);

            Assert.Equal(kind, p.Kind);
            Assert.Equal(new CellPos(3, 4), p.Pos);
            Assert.Equal(layers, p.Layers);
        }

        [Theory]
        [InlineData(ObstacleKind.Crate, 0)]
        [InlineData(ObstacleKind.Crate, 4)]
        [InlineData(ObstacleKind.Pinata, 2)]
        [InlineData(ObstacleKind.WaterBalloon, 2)]
        [InlineData(ObstacleKind.Anchor, 0)]
        public void ObstaclePlacement_LayersOutOfRange_ThrowsArgumentOutOfRange(ObstacleKind kind, int layers)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ObstaclePlacement(kind, new CellPos(0, 1), layers));
            Assert.Equal("layers", ex.ParamName);
        }

        [Fact]
        public void ObstaclePlacement_UndefinedKind_ThrowsArgumentOutOfRange()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ObstaclePlacement((ObstacleKind)(-1), new CellPos(0, 1), 1));
            Assert.Equal("kind", ex.ParamName);
        }

        [Fact]
        public void ObstaclePlacement_Equality_ComparesKindPosAndLayers()
        {
            var a = new ObstaclePlacement(ObstacleKind.Crate, new CellPos(1, 2), 2);

            Assert.True(a == new ObstaclePlacement(ObstacleKind.Crate, new CellPos(1, 2), 2));
            Assert.Equal(a.GetHashCode(), new ObstaclePlacement(ObstacleKind.Crate, new CellPos(1, 2), 2).GetHashCode());
            Assert.True(a != new ObstaclePlacement(ObstacleKind.Crate, new CellPos(2, 1), 2));
            Assert.True(a != new ObstaclePlacement(ObstacleKind.Crate, new CellPos(1, 2), 3));
            Assert.True(a != new ObstaclePlacement(ObstacleKind.Pinata, new CellPos(1, 2), 1));
            Assert.False(string.IsNullOrEmpty(a.ToString()));
        }

        // ---- TurnEffect, TurnResult, GravityResult additions ----

        private static readonly IReadOnlyList<ClearedCell> NoCleared = new List<ClearedCell>();
        private static readonly IReadOnlyList<ObstacleHit> NoHits = new List<ObstacleHit>();
        private static readonly IReadOnlyList<CellPos> NoPos = new List<CellPos>();
        private static readonly IReadOnlyList<CellMove> NoMoves = new List<CellMove>();
        private static readonly IReadOnlyList<TurnEffect> NoEffects = new List<TurnEffect>();

        [Fact]
        public void TurnEffect_EightParameterCtor_KeepsHitsWithoutCopy()
        {
            var hits = new List<ObstacleHit> { new ObstacleHit(new CellPos(0, 0), C1, Cell.Empty) };
            var e = new TurnEffect(EffectKind.Bomb, new CellPos(1, 1), NoCleared, Cell.Empty, -1, 1, NoCleared, hits);

            Assert.Same(hits, e.Hits);
        }

        [Fact]
        public void TurnEffect_NullHits_ThrowsArgumentNullAfterEarlierChecks()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new TurnEffect(EffectKind.Bomb, new CellPos(0, 0), NoCleared, Cell.Empty, -1, 1, NoCleared, null));
            Assert.Equal("hits", ex.ParamName);

            var first = Assert.Throws<ArgumentNullException>(() =>
                new TurnEffect(EffectKind.Bomb, new CellPos(0, 0), null, Cell.Empty, -1, 1, NoCleared, null));
            Assert.Equal("cleared", first.ParamName);

            var range = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TurnEffect(EffectKind.Bomb, new CellPos(0, 0), NoCleared, Cell.Empty, -1, -1, NoCleared, null));
            Assert.Equal("mergedCount", range.ParamName);
        }

        [Fact]
        public void TurnEffect_OldConstructors_HaveEmptyHits()
        {
            var five = new TurnEffect(EffectKind.Group, new CellPos(0, 0), NoCleared, Cell.Empty, -1);
            var seven = new TurnEffect(EffectKind.Bomb, new CellPos(0, 0), NoCleared, Cell.Empty, -1, 1, NoCleared);

            Assert.NotNull(five.Hits);
            Assert.Empty(five.Hits);
            Assert.NotNull(seven.Hits);
            Assert.Empty(seven.Hits);
        }

        [Fact]
        public void TurnResult_FiveParameterCtor_KeepsCollectedWithoutCopy()
        {
            var collected = new List<CellMove> { new CellMove(new CellPos(0, 2), new CellPos(0, 0)) };
            var r = new TurnResult(NoPos, NoMoves, NoPos, NoEffects, collected);

            Assert.Same(collected, r.Collected);
        }

        [Fact]
        public void TurnResult_NullCollected_ThrowsArgumentNullAfterEffects()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TurnResult(NoPos, NoMoves, NoPos, NoEffects, null));
            Assert.Equal("collected", ex.ParamName);

            var first = Assert.Throws<ArgumentNullException>(() => new TurnResult(NoPos, NoMoves, NoPos, null, null));
            Assert.Equal("effects", first.ParamName);
        }

        [Fact]
        public void TurnResult_OldConstructors_HaveEmptyCollected()
        {
            var three = new TurnResult(NoPos, NoMoves, NoPos);
            var four = new TurnResult(NoPos, NoMoves, NoPos, NoEffects);

            Assert.NotNull(three.Collected);
            Assert.Empty(three.Collected);
            Assert.NotNull(four.Collected);
            Assert.Empty(four.Collected);
        }

        [Fact]
        public void GravityResult_Constructor_KeepsLists()
        {
            var moves = new List<CellMove> { new CellMove(new CellPos(0, 1), new CellPos(0, 0)) };
            var collected = new List<CellMove>();
            var r = new GravityResult(moves, collected);

            Assert.Equal(moves, r.Moves);
            Assert.Equal(collected, r.Collected);
        }

        [Fact]
        public void GravityResult_NullLists_ThrowArgumentNullMovesFirst()
        {
            Assert.Equal("moves", Assert.Throws<ArgumentNullException>(() => new GravityResult(null, null)).ParamName);
            Assert.Equal("collected", Assert.Throws<ArgumentNullException>(() => new GravityResult(NoMoves, null)).ParamName);
        }
    }
}
