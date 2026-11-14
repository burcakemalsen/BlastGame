using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>What <see cref="GravityRules.Settle"/> did: moved cells and collected Anchors.</summary>
    public sealed class GravityResult
    {
        private readonly IReadOnlyList<CellMove> _moves;
        private readonly IReadOnlyList<CellMove> _collected;

        /// <summary>Stores the lists as they are (no copy).</summary>
        public GravityResult(IReadOnlyList<CellMove> moves, IReadOnlyList<CellMove> collected)
        {
            if (moves == null)
                throw new ArgumentNullException(nameof(moves));
            if (collected == null)
                throw new ArgumentNullException(nameof(collected));
            _moves = moves;
            _collected = collected;
        }

        /// <summary>Cells that moved and were not collected: x ascending, then To.Y ascending.</summary>
        public IReadOnlyList<CellMove> Moves
        {
            get { return _moves; }
        }

        /// <summary>Collected Anchors: from -> (x, 0); x ascending, then From.Y ascending.</summary>
        public IReadOnlyList<CellMove> Collected
        {
            get { return _collected; }
        }
    }
}
