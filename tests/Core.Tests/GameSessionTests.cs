using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class GameSessionTests
    {
        // Example grid of spec 04 (top row first, y = 0 is the last row):
        //   y=2  B G G
        //   y=1  R R G
        //   y=0  B R B
        private static Grid ExampleGrid()
        {
            return TestGrid.Parse("BGG", "RRG", "BRB");
        }

        private static ColorGoal G(TileColor color, int count)
        {
            return new ColorGoal(color, count);
        }

        private static List<ColorGoal> Goals(params ColorGoal[] goals)
        {
            return new List<ColorGoal>(goals);
        }

        private static GameSession Session(Grid grid, int colorCount, int moves, IRandomSource random, params ColorGoal[] goals)
        {
            return new GameSession(grid, colorCount, moves, Goals(goals), random);
        }

        private static GameSession Valid()
        {
            return Session(ExampleGrid(), 3, 5, CountingRandom.Constant(1), G(TileColor.Red, 2));
        }

        private static void AssertEmptyResult(TurnResult result)
        {
            Assert.NotNull(result);
            Assert.False(result.HasBlasted);
            Assert.Empty(result.Blasted);
            Assert.Empty(result.Moves);
            Assert.Empty(result.Filled);
        }

        // Won after the first tap (Example 2) or Lost (Example 3). Returns the session.
        private static GameSession Finished(bool won, CountingRandom random)
        {
            GameSession session = won
                ? Session(ExampleGrid(), 3, 1, random, G(TileColor.Red, 3))
                : Session(ExampleGrid(), 3, 1, random, G(TileColor.Blue, 1));
            session.Tap(1, 0);
            return session;
        }

        // ---------- constructor: valid ----------

        [Fact]
        public void Ctor_ValidArguments_SetsInitialState()
        {
            Grid grid = ExampleGrid();
            var random = CountingRandom.Constant(1);

            var session = Session(grid, 3, 5, random, G(TileColor.Red, 2), G(TileColor.Blue, 5));

            Assert.Same(grid, session.Grid);
            Assert.Equal(3, session.ColorCount);
            Assert.Equal(5, session.MovesLeft);
            Assert.Equal(GameStatus.Playing, session.Status);
            Assert.Equal(2, session.Remaining(TileColor.Red));
            Assert.Equal(5, session.Remaining(TileColor.Blue));
            Assert.Equal(0, random.Calls);
            TestGrid.AssertRows(new[] { "BGG", "RRG", "BRB" }, grid);
        }

        [Fact]
        public void Goals_GivenOrder_KeptWithInitialValues()
        {
            var session = Session(ExampleGrid(), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Blue, 5), G(TileColor.Red, 2));

            Assert.Equal(2, session.Goals.Count);
            Assert.Equal(G(TileColor.Blue, 5), session.Goals[0]);
            Assert.Equal(G(TileColor.Red, 2), session.Goals[1]);
        }

        [Fact]
        public void Goals_AfterTaps_StillHoldInitialValues()
        {
            var session = Session(ExampleGrid(), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Red, 2), G(TileColor.Blue, 5));

            session.Tap(1, 0);

            Assert.Equal(G(TileColor.Red, 2), session.Goals[0]);
            Assert.Equal(G(TileColor.Blue, 5), session.Goals[1]);
        }

        [Fact]
        public void Ctor_GoalsListChangedAfterwards_SessionUnaffected()
        {
            var list = Goals(G(TileColor.Red, 2), G(TileColor.Blue, 5));
            var session = new GameSession(ExampleGrid(), 3, 5, list, CountingRandom.Constant(1));

            list[0] = G(TileColor.Red, 99);
            list.Add(G(TileColor.Green, 1));
            list.RemoveAt(1);

            Assert.Equal(2, session.Goals.Count);
            Assert.Equal(G(TileColor.Red, 2), session.Goals[0]);
            Assert.Equal(G(TileColor.Blue, 5), session.Goals[1]);
            Assert.Equal(2, session.Remaining(TileColor.Red));
            Assert.Equal(5, session.Remaining(TileColor.Blue));
            Assert.Throws<ArgumentException>(() => session.Remaining(TileColor.Green));
        }

        [Fact]
        public void Ctor_MovesOneAndSingleGoal_Accepted()
        {
            var session = Session(ExampleGrid(), 3, 1, CountingRandom.Constant(1), G(TileColor.Red, 1));

            Assert.Equal(1, session.MovesLeft);
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Ctor_OneGoalPerColor_AllRemainingSet()
        {
            var goals = new List<ColorGoal>();
            for (int c = 0; c < GridFactory.MaxColorCount; c++)
            {
                goals.Add(G((TileColor)c, c + 1));
            }

            var session = new GameSession(TestGrid.Filled(2, 2, TileColor.Red), GridFactory.MaxColorCount, 3,
                goals, CountingRandom.Constant(0));

            for (int c = 0; c < GridFactory.MaxColorCount; c++)
            {
                Assert.Equal(c + 1, session.Remaining((TileColor)c));
            }
        }

        [Fact]
        public void Ctor_GoalColorIsColorCountMinusOne_Accepted()
        {
            var session = Session(ExampleGrid(), 3, 5, CountingRandom.Constant(1), G(TileColor.Blue, 4));

            Assert.Equal(4, session.Remaining(TileColor.Blue));
        }

        [Fact]
        public void Ctor_ColorCountOne_Accepted()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 1, 2, CountingRandom.Constant(0),
                G(TileColor.Red, 3));

            Assert.Equal(1, session.ColorCount);
        }

        // ---------- constructor: errors ----------

        [Fact]
        public void Ctor_GridNull_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(null, 3, 5, Goals(G(TileColor.Red, 1)), CountingRandom.Constant(1)));

            Assert.Equal("grid", ex.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(GridFactory.MaxColorCount + 1)]
        [InlineData(int.MaxValue)]
        public void Ctor_ColorCountOutOfRange_ThrowsArgumentOutOfRange(int colorCount)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameSession(ExampleGrid(), colorCount, 5, Goals(G(TileColor.Red, 1)), CountingRandom.Constant(0)));

            Assert.Equal("colorCount", ex.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Ctor_MovesBelowOne_ThrowsArgumentOutOfRange(int moves)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameSession(ExampleGrid(), 3, moves, Goals(G(TileColor.Red, 1)), CountingRandom.Constant(1)));

            Assert.Equal("moves", ex.ParamName);
        }

        [Fact]
        public void Ctor_GoalsNull_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(ExampleGrid(), 3, 5, null, CountingRandom.Constant(1)));

            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Ctor_GoalsEmpty_ThrowsArgumentExceptionExactType()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(ExampleGrid(), 3, 5, new List<ColorGoal>(), CountingRandom.Constant(1)));

            Assert.Equal("goals", ex.ParamName);
        }

        [Theory]
        [InlineData(3, TileColor.Yellow)]  // color == colorCount
        [InlineData(1, TileColor.Green)]   // color == colorCount (smallest)
        [InlineData(2, TileColor.Orange)]  // color > colorCount
        [InlineData(5, TileColor.Orange)]  // color == colorCount, largest enum value
        public void Ctor_GoalColorNotInPlay_ThrowsArgumentExceptionExactType(int colorCount, TileColor color)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(ExampleGrid(), colorCount, 5,
                    Goals(G(TileColor.Red, 1), G(color, 1)), CountingRandom.Constant(0)));

            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Ctor_SameColorTwice_ThrowsArgumentExceptionExactType()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(ExampleGrid(), 3, 5,
                    Goals(G(TileColor.Red, 2), G(TileColor.Blue, 1), G(TileColor.Red, 7)), CountingRandom.Constant(1)));

            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Ctor_DefaultGoalWithCountZero_ThrowsArgumentExceptionExactType()
        {
            Grid grid = ExampleGrid();
            var random = CountingRandom.Constant(1);

            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(grid, 3, 5, Goals(G(TileColor.Blue, 1), default(ColorGoal)), random));

            Assert.Equal("goals", ex.ParamName);
            Assert.Equal(0, random.Calls);
            TestGrid.AssertRows(new[] { "BGG", "RRG", "BRB" }, grid);
        }

        [Fact]
        public void Ctor_RandomNull_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(ExampleGrid(), 3, 5, Goals(G(TileColor.Red, 1)), null));

            Assert.Equal("random", ex.ParamName);
        }

        // ---------- constructor: error order ----------

        [Fact]
        public void Ctor_EverythingInvalid_ReportsGridFirst()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(null, 0, 0, null, null));

            Assert.Equal("grid", ex.ParamName);
        }

        [Fact]
        public void Ctor_ColorCountAndLaterArgumentsInvalid_ReportsColorCountBeforeMoves()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameSession(ExampleGrid(), 0, 0, null, null));

            Assert.Equal("colorCount", ex.ParamName);
        }

        [Fact]
        public void Ctor_MovesAndLaterArgumentsInvalid_ReportsMovesBeforeGoals()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameSession(ExampleGrid(), 3, 0, null, null));

            Assert.Equal("moves", ex.ParamName);
        }

        [Fact]
        public void Ctor_GoalsNullAndRandomNull_ReportsGoalsFirst()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(ExampleGrid(), 3, 5, null, null));

            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Ctor_GoalsEmptyAndRandomNull_ReportsGoalsFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(ExampleGrid(), 3, 5, new List<ColorGoal>(), null));

            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Ctor_BadGoalColorAndRandomNull_ReportsGoalsFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(ExampleGrid(), 3, 5, Goals(G(TileColor.Yellow, 1)), null));

            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Ctor_Failure_LeavesGridAndSourceUntouched()
        {
            Grid grid = ExampleGrid();
            var random = CountingRandom.Constant(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameSession(grid, 3, 0, Goals(G(TileColor.Red, 1)), random));
            Assert.Throws<ArgumentException>(() =>
                new GameSession(grid, 3, 5, new List<ColorGoal>(), random));

            TestGrid.AssertRows(new[] { "BGG", "RRG", "BRB" }, grid);
            Assert.Equal(0, random.Calls);
        }

        // ---------- spec examples ----------

        [Fact]
        public void Tap_Example1_CountsBlastedRedAndSpendsOneMove()
        {
            var random = CountingRandom.Constant(1);
            var session = Session(ExampleGrid(), 3, 5, random, G(TileColor.Red, 2), G(TileColor.Blue, 5));

            TurnResult result = session.Tap(1, 0);

            Assert.Equal(3, result.Blasted.Count);
            Assert.Equal(4, session.MovesLeft);
            Assert.Equal(0, session.Remaining(TileColor.Red));
            Assert.Equal(5, session.Remaining(TileColor.Blue));
            Assert.Equal(GameStatus.Playing, session.Status);
            TestGrid.AssertRows(new[] { "GGG", "BGG", "BGB" }, session.Grid);
            Assert.Equal(3, random.Calls);
            Assert.Equal(new[] { 3, 3, 3 }, random.MaxArgs);
        }

        [Fact]
        public void Tap_Example2_CompletingGoalOnLastMoveWins()
        {
            var session = Session(ExampleGrid(), 3, 1, CountingRandom.Constant(1), G(TileColor.Red, 3));

            session.Tap(1, 0);

            Assert.Equal(0, session.MovesLeft);
            Assert.Equal(0, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Won, session.Status);
        }

        [Fact]
        public void Tap_Example3_GoalOpenOnLastMoveLoses_ThenTapDoesNothing()
        {
            var random = CountingRandom.Constant(1);
            var session = Session(ExampleGrid(), 3, 1, random, G(TileColor.Blue, 1));

            session.Tap(1, 0);

            Assert.Equal(0, session.MovesLeft);
            Assert.Equal(1, session.Remaining(TileColor.Blue));
            Assert.Equal(GameStatus.Lost, session.Status);

            string[] before = TestGrid.Dump(session.Grid);
            int callsBefore = random.Calls;

            TurnResult result = session.Tap(2, 2);

            AssertEmptyResult(result);
            Assert.Equal(before, TestGrid.Dump(session.Grid));
            Assert.Equal(callsBefore, random.Calls);
            Assert.Equal(0, session.MovesLeft);
            Assert.Equal(GameStatus.Lost, session.Status);
        }

        [Fact]
        public void Tap_Example4_SingleTile_ChangesNothingAndSkipsSource()
        {
            var random = CountingRandom.Constant(1);
            var session = Session(ExampleGrid(), 3, 5, random, G(TileColor.Red, 2), G(TileColor.Blue, 5));

            TurnResult result = session.Tap(0, 0);

            AssertEmptyResult(result);
            Assert.Equal(5, session.MovesLeft);
            Assert.Equal(2, session.Remaining(TileColor.Red));
            Assert.Equal(5, session.Remaining(TileColor.Blue));
            Assert.Equal(GameStatus.Playing, session.Status);
            TestGrid.AssertRows(new[] { "BGG", "RRG", "BRB" }, session.Grid);
            Assert.Equal(0, random.Calls);
        }

        // ---------- moves ----------

        [Fact]
        public void Tap_EmptyCell_SpendsNoMoveAndSkipsSource()
        {
            var random = CountingRandom.Constant(0);
            var session = Session(TestGrid.Parse("RR", "R."), 3, 4, random, G(TileColor.Red, 3));

            TurnResult result = session.Tap(1, 0);

            AssertEmptyResult(result);
            Assert.Equal(4, session.MovesLeft);
            Assert.Equal(3, session.Remaining(TileColor.Red));
            TestGrid.AssertRows(new[] { "RR", "R." }, session.Grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_NoBlastOnLastMove_StaysPlayingWithMoveKept()
        {
            var session = Session(ExampleGrid(), 3, 1, CountingRandom.Constant(1), G(TileColor.Blue, 1));

            session.Tap(0, 0);

            Assert.Equal(1, session.MovesLeft);
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_BlastingTap_SpendsExactlyOneMove()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 3, CountingRandom.Constant(0),
                G(TileColor.Blue, 9));

            session.Tap(0, 0);

            Assert.Equal(2, session.MovesLeft);
        }

        // ---------- goal counting ----------

        [Fact]
        public void Tap_BlastedColorIsSecondGoal_OnlyThatGoalDrops()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Blue), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Red, 3), G(TileColor.Blue, 6));

            session.Tap(0, 0);

            Assert.Equal(3, session.Remaining(TileColor.Red));
            Assert.Equal(2, session.Remaining(TileColor.Blue));
        }

        [Fact]
        public void Tap_BlastLargerThanRemaining_StopsAtZero()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Red, 2), G(TileColor.Blue, 9));

            session.Tap(0, 0);

            Assert.Equal(0, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_BlastEqualsRemaining_GoalReachesZeroExactly()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Red, 4), G(TileColor.Blue, 9));

            session.Tap(0, 0);

            Assert.Equal(0, session.Remaining(TileColor.Red));
            Assert.Equal(9, session.Remaining(TileColor.Blue));
        }

        [Fact]
        public void Tap_BlastedColorIsNotAGoal_SpendsMoveButChangesNoGoal()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Blue), 3, 5, CountingRandom.Constant(0),
                G(TileColor.Red, 1));

            session.Tap(0, 0);

            Assert.Equal(4, session.MovesLeft);
            Assert.Equal(1, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_RefilledTilesOfGoalColor_AreNotCounted()
        {
            // Refill brings 4 new red tiles; only the 4 blasted ones count.
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 5, CountingRandom.Constant(0),
                G(TileColor.Red, 6));

            session.Tap(0, 0);

            Assert.Equal(2, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_FallenTilesOfGoalColor_AreNotCounted()
        {
            // Blasting the blue pair makes the red pair above it fall; falling is not blasting.
            //   y=1  R R
            //   y=0  B B
            var session = Session(TestGrid.Parse("RR", "BB"), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Red, 5));

            session.Tap(0, 0);   // blasts the two blue; the two red fall

            Assert.Equal(5, session.Remaining(TileColor.Red));
            Assert.Equal(4, session.MovesLeft);
        }

        [Fact]
        public void Tap_SeveralBlasts_AccumulateAcrossTaps()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 5, CountingRandom.Constant(0),
                G(TileColor.Red, 9));

            session.Tap(0, 0);
            Assert.Equal(5, session.Remaining(TileColor.Red));
            session.Tap(0, 0);
            Assert.Equal(1, session.Remaining(TileColor.Red));
            Assert.Equal(3, session.MovesLeft);
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        // ---------- status ----------

        [Fact]
        public void Tap_OneOfTwoGoalsDone_StaysPlaying_ThenWinsWhenSecondDone()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Red, 4), G(TileColor.Green, 4));

            session.Tap(0, 0);   // 4 red out, 4 green in

            Assert.Equal(0, session.Remaining(TileColor.Red));
            Assert.Equal(4, session.Remaining(TileColor.Green));
            Assert.Equal(GameStatus.Playing, session.Status);

            session.Tap(0, 0);   // 4 green out

            Assert.Equal(0, session.Remaining(TileColor.Green));
            Assert.Equal(GameStatus.Won, session.Status);
        }

        [Fact]
        public void Tap_GoalsDoneWithMovesLeft_Wins()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 5, CountingRandom.Constant(1),
                G(TileColor.Red, 4));

            session.Tap(0, 0);

            Assert.Equal(GameStatus.Won, session.Status);
            Assert.Equal(4, session.MovesLeft);
        }

        [Fact]
        public void Tap_MovesRunOutAfterSeveralTaps_Loses_NotBefore()
        {
            var session = Session(TestGrid.Filled(2, 2, TileColor.Red), 3, 2, CountingRandom.Constant(0),
                G(TileColor.Blue, 10));

            session.Tap(0, 0);
            Assert.Equal(1, session.MovesLeft);
            Assert.Equal(GameStatus.Playing, session.Status);

            session.Tap(0, 0);
            Assert.Equal(0, session.MovesLeft);
            Assert.Equal(GameStatus.Lost, session.Status);
        }

        // ---------- after the game is over ----------

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Tap_AfterGameOver_ChangesNothingAndSkipsSource(bool won)
        {
            var random = CountingRandom.Constant(1);
            GameSession session = Finished(won, random);
            string[] gridBefore = TestGrid.Dump(session.Grid);
            int callsBefore = random.Calls;
            TileColor goalColor = won ? TileColor.Red : TileColor.Blue;
            int remainingGoal = session.Remaining(goalColor);
            GameStatus status = session.Status;

            for (int i = 0; i < 3; i++)
            {
                TurnResult result = session.Tap(i, i);
                AssertEmptyResult(result);
            }

            Assert.Equal(gridBefore, TestGrid.Dump(session.Grid));
            Assert.Equal(callsBefore, random.Calls);
            Assert.Equal(0, session.MovesLeft);
            Assert.Equal(remainingGoal, session.Remaining(goalColor));
            Assert.Equal(status, session.Status);
        }

        [Theory]
        [InlineData(true, -1, 0, "x")]
        [InlineData(true, 0, 3, "y")]
        [InlineData(false, 3, 0, "x")]
        [InlineData(false, 0, -1, "y")]
        public void Tap_AfterGameOverOutsideGrid_StillThrowsArgumentOutOfRange(bool won, int x, int y, string paramName)
        {
            var random = CountingRandom.Constant(1);
            GameSession session = Finished(won, random);
            int callsBefore = random.Calls;

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => session.Tap(x, y));

            Assert.Equal(paramName, ex.ParamName);
            Assert.Equal(callsBefore, random.Calls);
        }

        // ---------- Tap argument checks ----------

        [Theory]
        [InlineData(-1, 0, "x")]
        [InlineData(3, 0, "x")]
        [InlineData(0, -1, "y")]
        [InlineData(0, 3, "y")]
        public void Tap_OutsideGrid_ThrowsAndChangesNothing(int x, int y, string paramName)
        {
            var random = CountingRandom.Constant(1);
            var session = Session(ExampleGrid(), 3, 5, random, G(TileColor.Red, 2));

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => session.Tap(x, y));

            Assert.Equal(paramName, ex.ParamName);
            Assert.Equal(5, session.MovesLeft);
            Assert.Equal(2, session.Remaining(TileColor.Red));
            TestGrid.AssertRows(new[] { "BGG", "RRG", "BRB" }, session.Grid);
            Assert.Equal(0, random.Calls);
        }

        [Theory]
        [InlineData(-1, -1)]
        [InlineData(3, 3)]
        public void Tap_BothCoordinatesOutside_ReportsXFirst(int x, int y)
        {
            var session = Valid();

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => session.Tap(x, y));

            Assert.Equal("x", ex.ParamName);
        }

        // ---------- Remaining ----------

        [Theory]
        [InlineData(TileColor.Blue)]      // valid color, not a goal
        [InlineData(TileColor.Orange)]    // not even in play
        [InlineData((TileColor)99)]       // not a TileColor at all
        public void Remaining_ColorNotAGoal_ThrowsArgumentExceptionExactType(TileColor color)
        {
            var session = Valid();

            var ex = Assert.Throws<ArgumentException>(() => session.Remaining(color));

            Assert.Equal("color", ex.ParamName);
        }

        // ---------- source failure ----------

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]   // == colorCount
        public void Tap_SourceReturnsInvalidValue_ThrowsAndCountersStay(int badValue)
        {
            var session = Session(ExampleGrid(), 3, 5, CountingRandom.Constant(badValue),
                G(TileColor.Red, 2), G(TileColor.Blue, 5));

            Assert.Throws<InvalidOperationException>(() => session.Tap(1, 0));

            Assert.Equal(5, session.MovesLeft);
            Assert.Equal(2, session.Remaining(TileColor.Red));
            Assert.Equal(5, session.Remaining(TileColor.Blue));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        // ---------- reproducibility ----------

        [Fact]
        public void Tap_SameSeedSameTaps_GivesSameStates()
        {
            GameSession a = SeededSession(1234);
            GameSession b = SeededSession(1234);

            for (int i = 0; i < 25; i++)
            {
                int x = (i * 7) % 9;
                int y = (i * 5 + 2) % 9;

                TurnResult ra = a.Tap(x, y);
                TurnResult rb = b.Tap(x, y);

                Assert.Equal(ra.Blasted.Count, rb.Blasted.Count);
                Assert.Equal(Snapshot(a), Snapshot(b));
            }
        }

        [Fact]
        public void Tap_SeededPlay_MovesNeverNegativeAndSomethingBlasts()
        {
            GameSession session = SeededSession(99);
            int blastingTaps = 0;

            for (int i = 0; i < 40; i++)
            {
                int before = session.MovesLeft;
                bool blasted = session.Tap((i * 4) % 9, (i * 3) % 9).HasBlasted;
                if (blasted)
                {
                    blastingTaps++;
                    Assert.True(session.MovesLeft == before - 1 || before == 0);
                }

                Assert.True(session.MovesLeft >= 0);
            }

            Assert.True(blastingTaps > 0);
        }

        private static GameSession SeededSession(int seed)
        {
            var random = new SeededRandom(seed);
            Grid grid = GridFactory.CreateRandom(9, 9, 3, random);
            return Session(grid, 3, 6, random, G(TileColor.Red, 12), G(TileColor.Blue, 12));
        }

        private static string Snapshot(GameSession s)
        {
            return s.Status + "|" + s.MovesLeft + "|" + s.Remaining(TileColor.Red) + "|" +
                   s.Remaining(TileColor.Blue) + "|" + string.Join("/", SpecialGrid.Dump(s.Grid));
        }
    }
}
