using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Immutable level settings: board size, color count, moves and goals.</summary>
    public sealed class LevelData
    {
        private readonly IReadOnlyList<ColorGoal> _goals;
        private readonly IReadOnlyList<ObstaclePlacement> _obstacles = Array.Empty<ObstaclePlacement>();
        private readonly IReadOnlyList<ObstacleGoal> _obstacleGoals = Array.Empty<ObstacleGoal>();

        public LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals,
                         IReadOnlyList<ObstaclePlacement> obstacles, IReadOnlyList<ObstacleGoal> obstacleGoals)
        {
            throw new NotImplementedException();
        }

        public LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals)
        {
            if (width < Grid.MinSize || width > Grid.MaxSize)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < Grid.MinSize || height > Grid.MaxSize)
                throw new ArgumentOutOfRangeException(nameof(height));
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
                    throw new ArgumentException("Goal color " + goal.Color + " is not among the first " + colorCount + " colors.", nameof(goals));
                if (!seen.Add(goal.Color))
                    throw new ArgumentException("Goal color " + goal.Color + " is given twice.", nameof(goals));
                if (goal.Count < 1) // default(ColorGoal) skips the constructor check
                    throw new ArgumentException("Every goal needs a count of at least 1.", nameof(goals));
                copy[i] = goal;
            }

            Width = width;
            Height = height;
            ColorCount = colorCount;
            Moves = moves;
            _goals = Array.AsReadOnly(copy);
        }

        public int Width { get; }

        public int Height { get; }

        public int ColorCount { get; }

        public int Moves { get; }

        public IReadOnlyList<ColorGoal> Goals
        {
            get { return _goals; }
        }

        public IReadOnlyList<ObstaclePlacement> Obstacles
        {
            get { return _obstacles; }
        }

        public IReadOnlyList<ObstacleGoal> ObstacleGoals
        {
            get { return _obstacleGoals; }
        }

        public GameSession CreateSession(IRandomSource random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            Grid grid = GridFactory.CreateRandom(Width, Height, ColorCount, random);
            return new GameSession(grid, ColorCount, Moves, _goals, random);
        }
    }
}
