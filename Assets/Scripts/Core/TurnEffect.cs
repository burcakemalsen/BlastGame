using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One step of a tap, in play order.</summary>
    public sealed class TurnEffect
    {
        /// <summary>Stores the cleared list as it is (no copy).</summary>
        public TurnEffect(EffectKind kind, CellPos origin, IReadOnlyList<ClearedCell> cleared, Cell created, int triggeredBy)
        {
            throw new NotImplementedException();
        }

        public EffectKind Kind
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>Index in Effects of the effect that triggered this one; -1 for the first effect and for Group.</summary>
        public int TriggeredBy
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>Group: the tapped cell; others: the item's cell.</summary>
        public CellPos Origin
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>Cells this effect cleared, in scan order.</summary>
        public IReadOnlyList<ClearedCell> Cleared
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>The item born from a Group effect; otherwise Cell.Empty.</summary>
        public Cell Created
        {
            get { throw new NotImplementedException(); }
        }
    }
}
