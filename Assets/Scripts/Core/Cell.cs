using System;

namespace BlastGame.Core
{
    /// <summary>Value type describing what a grid cell holds.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public static readonly Cell Empty;

        private readonly CellKind _kind;
        private readonly TileColor _color;     // Color cells and Orbs
        private readonly SpecialKind _special; // Special cells only
        private readonly ObstacleKind _obstacle; // Obstacle cells only
        private readonly int _layers;            // Obstacle cells only
        private readonly bool _balloon;          // Color cells only

        private Cell(CellKind kind, TileColor color, SpecialKind special)
            : this(kind, color, special, default(ObstacleKind), 0, false)
        {
        }

        private Cell(CellKind kind, TileColor color, SpecialKind special, ObstacleKind obstacle, int layers, bool balloon)
        {
            _kind = kind;
            _color = color;
            _special = special;
            _obstacle = obstacle;
            _layers = layers;
            _balloon = balloon;
        }

        public static Cell OfColor(TileColor color)
        {
            if (!Enum.IsDefined(typeof(TileColor), color))
                throw new ArgumentOutOfRangeException(nameof(color), "Undefined tile color: " + (int)color + ".");
            return new Cell(CellKind.Color, color, default(SpecialKind));
        }

        /// <summary>A Rocket or a Bomb. Use <see cref="OfOrb"/> for an Orb, which needs a color.</summary>
        public static Cell OfSpecial(SpecialKind kind)
        {
            if (!Enum.IsDefined(typeof(SpecialKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind), "Undefined special kind: " + (int)kind + ".");
            if (kind == SpecialKind.Orb)
                throw new ArgumentException("An Orb needs a color; use OfOrb.", nameof(kind));
            return new Cell(CellKind.Special, default(TileColor), kind);
        }

        public static Cell OfOrb(TileColor color)
        {
            if (!Enum.IsDefined(typeof(TileColor), color))
                throw new ArgumentOutOfRangeException(nameof(color), "Undefined tile color: " + (int)color + ".");
            return new Cell(CellKind.Special, color, SpecialKind.Orb);
        }

        /// <summary>A Pinata, Crate or Anchor. WaterBalloon is a flag on a colored cell; see <see cref="WithBalloon"/>.</summary>
        public static Cell OfObstacle(ObstacleKind kind, int layers)
        {
            if (!Enum.IsDefined(typeof(ObstacleKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind), "Undefined obstacle kind: " + (int)kind + ".");
            if (kind == ObstacleKind.WaterBalloon)
                throw new ArgumentException("A water balloon rides on a colored cell; use WithBalloon.", nameof(kind));
            if (layers < 1 || layers > MaxLayers(kind))
                throw new ArgumentOutOfRangeException(nameof(layers), kind + " takes 1.." + MaxLayers(kind) + " layers, got " + layers + ".");
            return new Cell(CellKind.Obstacle, default(TileColor), default(SpecialKind), kind, layers, false);
        }

        /// <summary>Highest layer count a placed obstacle of this kind can have (Crate 3, others 1).</summary>
        internal static int MaxLayers(ObstacleKind kind)
        {
            return kind == ObstacleKind.Crate ? 3 : 1;
        }

        public CellKind Kind
        {
            get { return _kind; }
        }

        public bool IsEmpty
        {
            get { return _kind == CellKind.Empty; }
        }

        public bool IsSpecial
        {
            get { return _kind == CellKind.Special; }
        }

        public bool IsObstacle
        {
            get { return _kind == CellKind.Obstacle; }
        }

        public ObstacleKind Obstacle
        {
            get
            {
                if (_kind != CellKind.Obstacle)
                    throw new InvalidOperationException("Only an obstacle cell has an obstacle kind; this cell is " + _kind + ".");
                return _obstacle;
            }
        }

        public int Layers
        {
            get
            {
                if (_kind != CellKind.Obstacle)
                    throw new InvalidOperationException("Only an obstacle cell has layers; this cell is " + _kind + ".");
                return _layers;
            }
        }

        /// <summary>Readable on every cell; only a colored cell can carry a balloon.</summary>
        public bool HasBalloon
        {
            get { return _balloon; }
        }

        /// <summary>The same colored cell with or without a water balloon.</summary>
        public Cell WithBalloon(bool hasBalloon)
        {
            if (_kind != CellKind.Color)
                throw new InvalidOperationException("Only a colored cell can carry a balloon; this cell is " + _kind + ".");
            return new Cell(CellKind.Color, _color, default(SpecialKind), default(ObstacleKind), 0, hasBalloon);
        }

        /// <summary>True for a colored cell of the given color, with or without a balloon.</summary>
        internal bool IsColor(TileColor color)
        {
            return _kind == CellKind.Color && _color == color;
        }

        public SpecialKind Special
        {
            get
            {
                if (_kind != CellKind.Special)
                    throw new InvalidOperationException("Only a special cell has a special kind; this cell is " + _kind + ".");
                return _special;
            }
        }

        public TileColor OrbColor
        {
            get
            {
                if (_kind != CellKind.Special || _special != SpecialKind.Orb)
                    throw new InvalidOperationException("Only an Orb has an orb color; this cell is " + ToString() + ".");
                return _color;
            }
        }

        public TileColor Color
        {
            get
            {
                if (_kind != CellKind.Color)
                    throw new InvalidOperationException("Only a colored cell has a color; this cell is " + _kind + ".");
                return _color;
            }
        }

        private bool IsOrb
        {
            get { return _kind == CellKind.Special && _special == SpecialKind.Orb; }
        }

        public bool Equals(Cell other)
        {
            if (_kind != other._kind) return false;
            switch (_kind)
            {
                case CellKind.Color:
                    return _color == other._color && _balloon == other._balloon;
                case CellKind.Special:
                    return _special == other._special && (_special != SpecialKind.Orb || _color == other._color);
                case CellKind.Obstacle:
                    return _obstacle == other._obstacle && _layers == other._layers;
                default:
                    return true;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            switch (_kind)
            {
                case CellKind.Color:
                    return 1 + (int)_color * 31 + (_balloon ? 500 : 0);
                case CellKind.Special:
                    return 1000 + (int)_special * 31 + (IsOrb ? 1 + (int)_color : 0) * 7;
                case CellKind.Obstacle:
                    return 2000 + (int)_obstacle * 31 + _layers;
                default:
                    return 0;
            }
        }

        /// <summary>"Empty", "Color(Red)", "Color(Red, Balloon)", "Special(Bomb)", "Special(Orb, Red)", "Obstacle(Crate, 2)".</summary>
        public override string ToString()
        {
            switch (_kind)
            {
                case CellKind.Color:
                    return _balloon ? "Color(" + _color + ", Balloon)" : "Color(" + _color + ")";
                case CellKind.Special:
                    return IsOrb ? "Special(Orb, " + _color + ")" : "Special(" + _special + ")";
                case CellKind.Obstacle:
                    return "Obstacle(" + _obstacle + ", " + _layers + ")";
                default:
                    return "Empty";
            }
        }

        public static bool operator ==(Cell a, Cell b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cell a, Cell b)
        {
            return !a.Equals(b);
        }
    }
}
