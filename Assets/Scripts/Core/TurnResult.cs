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
        private readonly IReadOnlyList<TurnEffect> _effects;
        private readonly IReadOnlyList<CellMove> _collected;

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
            _effects = Array.Empty<TurnEffect>();
            _collected = Array.Empty<CellMove>();
        }

        /// <summary>Stores the given lists as they are (no copy).</summary>
        public TurnResult(IReadOnlyList<CellPos> blasted, IReadOnlyList<CellMove> moves, IReadOnlyList<CellPos> filled,
                          IReadOnlyList<TurnEffect> effects)
        {
            if (blasted == null)
                throw new ArgumentNullException(nameof(blasted));
            if (moves == null)
                throw new ArgumentNullException(nameof(moves));
            if (filled == null)
                throw new ArgumentNullException(nameof(filled));
            if (effects == null)
                throw new ArgumentNullException(nameof(effects));

            _blasted = blasted;
            _moves = moves;
            _filled = filled;
            _effects = effects;
            _collected = Array.Empty<CellMove>();
        }

        /// <summary>Stores the given lists as they are (no copy).</summary>
        public TurnResult(IReadOnlyList<CellPos> blasted, IReadOnlyList<CellMove> moves, IReadOnlyList<CellPos> filled,
                          IReadOnlyList<TurnEffect> effects, IReadOnlyList<CellMove> collected)
            : this(blasted, moves, filled, effects)
        {
            if (collected == null)
                throw new ArgumentNullException(nameof(collected));
            _collected = collected;
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

        /// <summary>Effects in run (queue) order; empty when nothing was blasted.</summary>
        public IReadOnlyList<TurnEffect> Effects
        {
            get { return _effects; }
        }

        /// <summary>Collected Anchors: from -> (x, 0); x ascending, then From.Y ascending.</summary>
        public IReadOnlyList<CellMove> Collected
        {
            get { return _collected; }
        }

        /// <summary>True when Blasted.Count > 0.</summary>
        public bool HasBlasted
        {
            get { return _blasted.Count > 0; }
        }
    }
}
