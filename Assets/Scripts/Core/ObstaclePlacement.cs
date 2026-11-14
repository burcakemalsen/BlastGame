using System;

namespace BlastGame.Core
{
    /// <summary>One obstacle placed by a level file.</summary>
    public readonly struct ObstaclePlacement : IEquatable<ObstaclePlacement>
    {
        private readonly ObstacleKind _kind;
        private readonly CellPos _pos;
        private readonly int _layers;

        public ObstaclePlacement(ObstacleKind kind, CellPos pos, int layers)
        {
            if (!Enum.IsDefined(typeof(ObstacleKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind), "Undefined obstacle kind: " + (int)kind + ".");
            if (layers < 1 || layers > Cell.MaxLayers(kind))
                throw new ArgumentOutOfRangeException(nameof(layers), kind + " takes 1.." + Cell.MaxLayers(kind) + " layers, got " + layers + ".");
            _kind = kind;
            _pos = pos;
            _layers = layers;
        }

        public ObstacleKind Kind
        {
            get { return _kind; }
        }

        public CellPos Pos
        {
            get { return _pos; }
        }

        public int Layers
        {
            get { return _layers; }
        }

        public bool Equals(ObstaclePlacement other)
        {
            return _kind == other._kind && _pos == other._pos && _layers == other._layers;
        }

        public override bool Equals(object obj)
        {
            return obj is ObstaclePlacement other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked(((int)_kind * 397 ^ _pos.GetHashCode()) * 397 ^ _layers);
        }

        /// <summary>Formats as "Crate (0, 4) x2".</summary>
        public override string ToString()
        {
            return _kind + " " + _pos + " x" + _layers;
        }

        public static bool operator ==(ObstaclePlacement a, ObstaclePlacement b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ObstaclePlacement a, ObstaclePlacement b)
        {
            return !a.Equals(b);
        }
    }
}
