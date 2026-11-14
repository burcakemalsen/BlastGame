using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>What <see cref="GravityRules.Settle"/> did: moved cells and collected Anchors.</summary>
    public sealed class GravityResult
    {
        public GravityResult(IReadOnlyList<CellMove> moves, IReadOnlyList<CellMove> collected)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<CellMove> Moves
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<CellMove> Collected
        {
            get { throw new NotImplementedException(); }
        }
    }
}
