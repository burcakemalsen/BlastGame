using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Immutable level settings: board size, color count, moves, goals and obstacle placements.</summary>
    public sealed class LevelData
    {
        private readonly IReadOnlyList<ColorGoal> _goals;
        private readonly IReadOnlyList<ObstaclePlacement> _obstacles;
        private readonly IReadOnlyList<ObstacleGoal> _obstacleGoals;

        /// <summary>No obstacles; an empty goal list is an error.</summary>
        public LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals)
            : this(width, height, colorCount, moves, goals, null, null, false)
        {
        }

        /// <summary>Color goals, obstacle placements and obstacle goals; at least one goal of either kind.</summary>
        public LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals,
                         IReadOnlyList<ObstaclePlacement> obstacles, IReadOnlyList<ObstacleGoal> obstacleGoals)
            : this(width, height, colorCount, moves, goals, obstacles, obstacleGoals, true)
        {
        }

        private LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals,
                          IReadOnlyList<ObstaclePlacement> obstacles, IReadOnlyList<ObstacleGoal> obstacleGoals,
                          bool withObstacles)
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
            if (!withObstacles && goals.Count == 0)
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

            var placements = new ObstaclePlacement[0];
            var obstacleCopy = new ObstacleGoal[0];
            if (withObstacles)
            {
                if (obstacles == null)
                    throw new ArgumentNullException(nameof(obstacles));
                placements = CopyPlacements(obstacles, width, height);

                if (obstacleGoals == null)
                    throw new ArgumentNullException(nameof(obstacleGoals));
                if (goals.Count == 0 && obstacleGoals.Count == 0)
                    throw new ArgumentException("At least one goal is needed.", nameof(goals));
                obstacleCopy = CopyObstacleGoals(obstacleGoals, placements);
            }

            Width = width;
            Height = height;
            ColorCount = colorCount;
            Moves = moves;
            _goals = Array.AsReadOnly(copy);
            _obstacles = Array.AsReadOnly(placements);
            _obstacleGoals = Array.AsReadOnly(obstacleCopy);
        }

        private static ObstaclePlacement[] CopyPlacements(IReadOnlyList<ObstaclePlacement> obstacles, int width, int height)
        {
            var copy = new ObstaclePlacement[obstacles.Count];
            var taken = new bool[width, height];
            for (int i = 0; i < copy.Length; i++)
            {
                ObstaclePlacement p = obstacles[i];
                if (!Enum.IsDefined(typeof(ObstacleKind), p.Kind) || p.Layers < 1 || p.Layers > Cell.MaxLayers(p.Kind))
                    throw new ArgumentException("Placement " + i + " is not valid.", nameof(obstacles)); // default(ObstaclePlacement)
                int x = p.Pos.X, y = p.Pos.Y;
                if (x < 0 || x >= width || y < 0 || y >= height)
                    throw new ArgumentException("Placement " + i + " at " + p.Pos + " is outside the grid.", nameof(obstacles));
                if (taken[x, y])
                    throw new ArgumentException("Placement " + i + " at " + p.Pos + " overlaps another one.", nameof(obstacles));
                if (p.Kind == ObstacleKind.Anchor && y == 0)
                    throw new ArgumentException("An Anchor cannot start on y = 0 (placement " + i + ").", nameof(obstacles));
                taken[x, y] = true;
                copy[i] = p;
            }
            return copy;
        }

        private static ObstacleGoal[] CopyObstacleGoals(IReadOnlyList<ObstacleGoal> obstacleGoals, ObstaclePlacement[] placements)
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
                int placed = 0;
                foreach (ObstaclePlacement p in placements)
                    if (p.Kind == goal.Kind)
                        placed++;
                if (goal.Count > placed)
                    throw new ArgumentException("Obstacle goal " + goal + " needs more than the " + placed + " placed.", nameof(obstacleGoals));
                copy[i] = goal;
            }
            return copy;
        }

        public int Width { get; }

        public int Height { get; }

        public int ColorCount { get; }

        public int Moves { get; }

        public IReadOnlyList<ColorGoal> Goals
        {
            get { return _goals; }
        }

        /// <summary>Obstacle placements, in file order.</summary>
        public IReadOnlyList<ObstaclePlacement> Obstacles
        {
            get { return _obstacles; }
        }

        /// <summary>Obstacle goals, in file order.</summary>
        public IReadOnlyList<ObstacleGoal> ObstacleGoals
        {
            get { return _obstacleGoals; }
        }

        /// <summary>
        /// Random grid first (same draws as before), then the placements in order: Pinata, Crate and Anchor
        /// replace the cell, a WaterBalloon goes on the colored cell there.
        /// </summary>
        public GameSession CreateSession(IRandomSource random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            Grid grid = GridFactory.CreateRandom(Width, Height, ColorCount, random);
            foreach (ObstaclePlacement p in _obstacles)
            {
                if (p.Kind == ObstacleKind.WaterBalloon)
                    grid.Set(p.Pos.X, p.Pos.Y, grid.Get(p.Pos.X, p.Pos.Y).WithBalloon(true));
                else
                    grid.Set(p.Pos.X, p.Pos.Y, Cell.OfObstacle(p.Kind, p.Layers));
            }
            return new GameSession(grid, ColorCount, Moves, _goals, _obstacleGoals, random);
        }
    }
}
