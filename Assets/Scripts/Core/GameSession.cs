using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One level in play: moves, color goals, win/lose, on top of <see cref="TurnRules"/>.</summary>
    public sealed class GameSession
    {
        public GameSession(Grid grid, int colorCount, int moves, IReadOnlyList<ColorGoal> goals, IRandomSource random)
        {
            throw new NotImplementedException();
        }

        public Grid Grid
        {
            get { throw new NotImplementedException(); }
        }

        public int ColorCount
        {
            get { throw new NotImplementedException(); }
        }

        public int MovesLeft
        {
            get { throw new NotImplementedException(); }
        }

        public GameStatus Status
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>Initial goals, in the order given.</summary>
        public IReadOnlyList<ColorGoal> Goals
        {
            get { throw new NotImplementedException(); }
        }

        public int Remaining(TileColor color)
        {
            throw new NotImplementedException();
        }

        public TurnResult Tap(int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}
