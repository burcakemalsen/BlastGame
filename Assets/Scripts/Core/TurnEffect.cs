using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One step of a tap, in play order.</summary>
    public sealed class TurnEffect
    {
        private readonly IReadOnlyList<ClearedCell> _cleared;

        /// <summary>Stores the cleared list as it is (no copy).</summary>
        public TurnEffect(EffectKind kind, CellPos origin, IReadOnlyList<ClearedCell> cleared, Cell created, int triggeredBy)
        {
            if (cleared == null)
                throw new ArgumentNullException(nameof(cleared));
            if (triggeredBy < -1)
                throw new ArgumentOutOfRangeException(nameof(triggeredBy), "TriggeredBy must be -1 or an effect index.");

            Kind = kind;
            Origin = origin;
            _cleared = cleared;
            Created = created;
            TriggeredBy = triggeredBy;
        }

        public EffectKind Kind { get; }

        /// <summary>Index in Effects of the effect that triggered this one; -1 for the first effect and for Group.</summary>
        public int TriggeredBy { get; }

        /// <summary>Group: the tapped cell; others: the item's cell.</summary>
        public CellPos Origin { get; }

        /// <summary>Cells this effect cleared, in scan order.</summary>
        public IReadOnlyList<ClearedCell> Cleared
        {
            get { return _cleared; }
        }

        /// <summary>The item born from a Group effect; otherwise Cell.Empty.</summary>
        public Cell Created { get; }
    }
}
