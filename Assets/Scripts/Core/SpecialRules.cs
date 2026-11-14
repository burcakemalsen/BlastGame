using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Special item birth and activation (with chains).</summary>
    public static class SpecialRules
    {
        public const int RocketMinGroup = 5;
        public const int BombMinGroup = 7;
        public const int OrbMinGroup = 9;

        /// <summary>
        /// The item born from a group of the given size and color; size below 5 gives Cell.Empty without
        /// calling the source. Sizes 5-6 draw once with Next(2).
        /// </summary>
        public static Cell ForGroup(int size, TileColor color, IRandomSource random)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Runs the special item at (x, y), finishes the chain and returns the effects in run (queue) order.
        /// Does not apply gravity or refill.
        /// </summary>
        public static IReadOnlyList<TurnEffect> Activate(Grid grid, int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}
