using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>What happened during one tap: blasted cells, gravity moves and filled cells.</summary>
    public sealed class TurnResult
    {
        public TurnResult(IReadOnlyList<CellPos> blasted, IReadOnlyList<CellMove> moves, IReadOnlyList<CellPos> filled)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<CellPos> Blasted
        {
            get { throw new System.NotImplementedException(); }
        }

        public IReadOnlyList<CellMove> Moves
        {
            get { throw new System.NotImplementedException(); }
        }

        public IReadOnlyList<CellPos> Filled
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>True when Blasted.Count > 0.</summary>
        public bool HasBlasted
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
