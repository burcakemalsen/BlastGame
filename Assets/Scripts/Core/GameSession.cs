using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One level in play: moves, color goals, win/lose, on top of <see cref="TurnRules"/>.</summary>
    public sealed class GameSession
    {
        private readonly IRandomSource _random;
        private readonly ColorGoal[] _goals;
        private readonly int[] _remaining; // same order as _goals

        public GameSession(Grid grid, int colorCount, int moves, IReadOnlyList<ColorGoal> goals, IRandomSource random)
        {
            // Validate everything, in the spec's order, before keeping anything.
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (colorCount < 1 || colorCount > GridFactory.MaxColorCount)
                throw new ArgumentOutOfRangeException(nameof(colorCount));
            if (moves < 1)
                throw new ArgumentOutOfRangeException(nameof(moves));
            if (goals == null)
                throw new ArgumentNullException(nameof(goals));
            if (goals.Count == 0)
                throw new ArgumentException("At least one goal is needed.", nameof(goals));

            var copy = new ColorGoal[goals.Count];
            var seen = new HashSet<TileColor>();
            for (int i = 0; i < copy.Length; i++)
            {
                ColorGoal goal = goals[i];
                if ((int)goal.Color >= colorCount)
                    throw new ArgumentException("Goal color " + goal.Color + " never appears with " + colorCount + " colors.", nameof(goals));
                if (!seen.Add(goal.Color))
                    throw new ArgumentException("Goal color " + goal.Color + " is given twice.", nameof(goals));
                if (goal.Count < 1) // default(ColorGoal) skips the constructor check
                    throw new ArgumentException("Every goal needs a count of at least 1.", nameof(goals));
                copy[i] = goal;
            }

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            Grid = grid;
            ColorCount = colorCount;
            MovesLeft = moves;
            Status = GameStatus.Playing;
            _goals = copy;
            _remaining = new int[copy.Length];
            for (int i = 0; i < copy.Length; i++)
                _remaining[i] = copy[i].Count;
            _random = random;
        }

        public Grid Grid { get; }

        public int ColorCount { get; }

        public int MovesLeft { get; private set; }

        public GameStatus Status { get; private set; }

        /// <summary>Initial goals, in the order given.</summary>
        public IReadOnlyList<ColorGoal> Goals
        {
            get { return _goals; }
        }

        public int Remaining(TileColor color)
        {
            for (int i = 0; i < _goals.Length; i++)
                if (_goals[i].Color == color)
                    return _remaining[i];
            throw new ArgumentException("Color " + color + " is not a goal.", nameof(color));
        }

        public TurnResult Tap(int x, int y)
        {
            if (x < 0 || x >= Grid.Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= Grid.Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            if (Status != GameStatus.Playing)
                return new TurnResult(new List<CellPos>(), new List<CellMove>(), new List<CellPos>());

            // The grid changes during the tap, so read the tapped cell first: a blasted group has its color.
            Cell tapped = Grid.Get(x, y);

            // If the rules throw, nothing below runs, so moves and goals stay as they were.
            TurnResult result = TurnRules.Tap(Grid, x, y, ColorCount, _random);
            if (!result.HasBlasted)
                return result;

            MovesLeft--;

            for (int i = 0; i < _goals.Length; i++)
            {
                if (_goals[i].Color != tapped.Color)
                    continue;
                _remaining[i] = Math.Max(0, _remaining[i] - result.Blasted.Count);
                break;
            }

            if (AllGoalsDone())
                Status = GameStatus.Won; // checked before losing: the last move may finish the goals
            else if (MovesLeft == 0)
                Status = GameStatus.Lost;

            return result;
        }

        private bool AllGoalsDone()
        {
            for (int i = 0; i < _remaining.Length; i++)
                if (_remaining[i] > 0)
                    return false;
            return true;
        }
    }
}
