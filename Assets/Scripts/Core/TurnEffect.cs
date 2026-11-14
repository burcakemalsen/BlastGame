using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One step of a tap, in play order.</summary>
    public sealed class TurnEffect
    {
        private static readonly IReadOnlyList<ClearedCell> NoCells = new ClearedCell[0];

        private readonly IReadOnlyList<ClearedCell> _cleared;
        private readonly int _mergedCount;
        private readonly IReadOnlyList<ClearedCell> _converted;

        /// <summary>Combo-aware constructor. Stores the lists as they are (no copy).</summary>
        public TurnEffect(EffectKind kind, CellPos origin, IReadOnlyList<ClearedCell> cleared, Cell created,
                          int triggeredBy, int mergedCount, IReadOnlyList<ClearedCell> converted)
        {
            throw new NotImplementedException();
        }

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
            _mergedCount = kind == EffectKind.Group ? 0 : 1;
            _converted = NoCells;
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

        /// <summary>Number of participants at the start of Cleared; 0 for Group, 1 for a single item.</summary>
        public int MergedCount
        {
            get { return _mergedCount; }
        }

        /// <summary>Cells turned into special items; filled only for OrbRocket and OrbBomb.</summary>
        public IReadOnlyList<ClearedCell> Converted
        {
            get { return _converted; }
        }
    }
}
