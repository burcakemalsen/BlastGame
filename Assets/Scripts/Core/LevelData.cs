using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Immutable level settings: board size, color count, moves and goals.</summary>
    public sealed class LevelData
    {
        public LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals)
        {
            throw new System.NotImplementedException();
        }

        public int Width
        {
            get { throw new System.NotImplementedException(); }
        }

        public int Height
        {
            get { throw new System.NotImplementedException(); }
        }

        public int ColorCount
        {
            get { throw new System.NotImplementedException(); }
        }

        public int Moves
        {
            get { throw new System.NotImplementedException(); }
        }

        public IReadOnlyList<ColorGoal> Goals
        {
            get { throw new System.NotImplementedException(); }
        }

        public GameSession CreateSession(IRandomSource random)
        {
            throw new System.NotImplementedException();
        }
    }
}
