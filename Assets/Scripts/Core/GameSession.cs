using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One level in play: moves, color and obstacle goals, win/lose, on top of <see cref="TurnRules"/>.</summary>
    public sealed class GameSession
    {
        private readonly IRandomSource _random;
        private readonly ColorGoal[] _goals;
        private readonly IReadOnlyList<ColorGoal> _goalsView; // read-only wrapper; a cast back to an array fails
        private readonly int[] _remaining; // same order as _goals
        private readonly ObstacleGoal[] _obstacleGoals;
        private readonly IReadOnlyList<ObstacleGoal> _obstacleGoalsView;
        private readonly int[] _obstacleRemaining; // same order as _obstacleGoals

        /// <summary>Color goals only; an empty goal list is an error.</summary>
        public GameSession(Grid grid, int colorCount, int moves, IReadOnlyList<ColorGoal> goals, IRandomSource random)
            : this(grid, colorCount, moves, goals, null, random, false)
        {
        }

        /// <summary>Color and obstacle goals; at least one goal of either kind.</summary>
        public GameSession(Grid grid, int colorCount, int moves, IReadOnlyList<ColorGoal> goals,
                           IReadOnlyList<ObstacleGoal> obstacleGoals, IRandomSource random)
            : this(grid, colorCount, moves, goals, obstacleGoals, random, true)
        {
        }

        private GameSession(Grid grid, int colorCount, int moves, IReadOnlyList<ColorGoal> goals,
                            IReadOnlyList<ObstacleGoal> obstacleGoals, IRandomSource random, bool withObstacles)
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
            if (!withObstacles && goals.Count == 0)
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

            ObstacleGoal[] obstacleCopy;
            if (withObstacles)
            {
                if (obstacleGoals == null)
                    throw new ArgumentNullException(nameof(obstacleGoals));
                if (goals.Count == 0 && obstacleGoals.Count == 0)
                    throw new ArgumentException("At least one goal is needed.", nameof(goals));
                obstacleCopy = CopyObstacleGoals(obstacleGoals);
            }
            else
            {
                obstacleCopy = new ObstacleGoal[0];
            }

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            Grid = grid;
            ColorCount = colorCount;
            MovesLeft = moves;
            Status = GameStatus.Playing;
            _goals = copy;
            _goalsView = Array.AsReadOnly(copy);
            _remaining = new int[copy.Length];
            for (int i = 0; i < copy.Length; i++)
                _remaining[i] = copy[i].Count;
            _obstacleGoals = obstacleCopy;
            _obstacleGoalsView = Array.AsReadOnly(obstacleCopy);
            _obstacleRemaining = new int[obstacleCopy.Length];
            for (int i = 0; i < obstacleCopy.Length; i++)
                _obstacleRemaining[i] = obstacleCopy[i].Count;
            _random = random;
        }

        private static ObstacleGoal[] CopyObstacleGoals(IReadOnlyList<ObstacleGoal> obstacleGoals)
        {
            var copy = new ObstacleGoal[obstacleGoals.Count];
            var seen = new HashSet<ObstacleKind>();
            for (int i = 0; i < copy.Length; i++)
            {
                ObstacleGoal goal = obstacleGoals[i];
                if (!Enum.IsDefined(typeof(ObstacleKind), goal.Kind))
                    throw new ArgumentException("Undefined obstacle kind " + (int)goal.Kind + ".", nameof(obstacleGoals));
                if (!seen.Add(goal.Kind))
                    throw new ArgumentException("Obstacle goal " + goal.Kind + " is given twice.", nameof(obstacleGoals));
                if (goal.Count < 1)
                    throw new ArgumentException("Every obstacle goal needs a count of at least 1.", nameof(obstacleGoals));
                copy[i] = goal;
            }
            return copy;
        }

        public Grid Grid { get; }

        public int ColorCount { get; }

        public int MovesLeft { get; private set; }

        public GameStatus Status { get; private set; }

        /// <summary>Initial goals, in the order given.</summary>
        public IReadOnlyList<ColorGoal> Goals
        {
            get { return _goalsView; }
        }

        /// <summary>Initial obstacle goals, in the order given.</summary>
        public IReadOnlyList<ObstacleGoal> ObstacleGoals
        {
            get { return _obstacleGoalsView; }
        }

        public int Remaining(TileColor color)
        {
            for (int i = 0; i < _goals.Length; i++)
                if (_goals[i].Color == color)
                    return _remaining[i];
            throw new ArgumentException("Color " + color + " is not a goal.", nameof(color));
        }

        public int Remaining(ObstacleKind kind)
        {
            for (int i = 0; i < _obstacleGoals.Length; i++)
                if (_obstacleGoals[i].Kind == kind)
                    return _obstacleRemaining[i];
            throw new ArgumentException("Obstacle " + kind + " is not a goal.", nameof(kind));
        }

        public TurnResult Tap(int x, int y)
        {
            if (x < 0 || x >= Grid.Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= Grid.Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            if (Status != GameStatus.Playing)
                return new TurnResult(new List<CellPos>(), new List<CellMove>(), new List<CellPos>());

            // If the rules throw, nothing below runs, so moves and goals stay as they were.
            TurnResult result = TurnRules.Tap(Grid, x, y, ColorCount, _random);
            if (!result.HasBlasted)
                return result;

            MovesLeft--;

            // Colored cells count by color (a balloon cell by its own color); special items count for no color.
            // Balloons count when their cell is cleared or converted; Pinatas and Crates when a hit breaks them.
            foreach (TurnEffect effect in result.Effects)
            {
                foreach (ClearedCell c in effect.Cleared)
                    CountCell(c.Cell);
                foreach (ClearedCell c in effect.Converted)
                    CountCell(c.Cell);
                foreach (ObstacleHit hit in effect.Hits)
                    if (hit.After.IsEmpty)
                        CountObstacle(hit.Before.Obstacle);
            }
            for (int i = 0; i < result.Collected.Count; i++)
                CountObstacle(ObstacleKind.Anchor);

            if (AllGoalsDone())
                Status = GameStatus.Won; // checked before losing: the last move may finish the goals
            else if (MovesLeft == 0)
                Status = GameStatus.Lost;

            return result;
        }

        private void CountCell(Cell cell)
        {
            if (cell.Kind != CellKind.Color)
                return;
            if (cell.HasBalloon)
                CountObstacle(ObstacleKind.WaterBalloon);
            for (int i = 0; i < _goals.Length; i++)
                if (_goals[i].Color == cell.Color)
                {
                    _remaining[i] = Math.Max(0, _remaining[i] - 1);
                    return;
                }
        }

        private void CountObstacle(ObstacleKind kind)
        {
            for (int i = 0; i < _obstacleGoals.Length; i++)
                if (_obstacleGoals[i].Kind == kind)
                {
                    _obstacleRemaining[i] = Math.Max(0, _obstacleRemaining[i] - 1);
                    return;
                }
        }

        private bool AllGoalsDone()
        {
            for (int i = 0; i < _remaining.Length; i++)
                if (_remaining[i] > 0)
                    return false;
            for (int i = 0; i < _obstacleRemaining.Length; i++)
                if (_obstacleRemaining[i] > 0)
                    return false;
            return true;
        }
    }
}
