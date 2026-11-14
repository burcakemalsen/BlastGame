using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>What happened during one tap: blasted cells, gravity moves and filled cells.</summary>
    public sealed class TurnResult
    {
        private readonly IReadOnlyList<CellPos> _blasted;
        private readonly IReadOnlyList<CellMove> _moves;
        private readonly IReadOnlyList<CellPos> _filled;

        /// <summary>Stores the given lists as they are (no copy).</summary>
        public TurnResult(IReadOnlyList<CellPos> blasted, IReadOnlyList<CellMove> moves, IReadOnlyList<CellPos> filled)
        {
            if (blasted == null)
                throw new ArgumentNullException(nameof(blasted));
            if (moves == null)
                throw new ArgumentNullException(nameof(moves));
            if (filled == null)
                throw new ArgumentNullException(nameof(filled));

            _blasted = blasted;
            _moves = moves;
            _filled = filled;
        }

        public IReadOnlyList<CellPos> Blasted
        {
            get { return _blasted; }
        }

        public IReadOnlyList<CellMove> Moves
        {
            get { return _moves; }
        }

        public IReadOnlyList<CellPos> Filled
        {
            get { return _filled; }
        }

        /// <summary>True when Blasted.Count > 0.</summary>
        public bool HasBlasted
        {
            get { return _blasted.Count > 0; }
        }
    }
}
