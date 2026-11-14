using BlastGame.Core;
using UnityEngine;

namespace BlastGame.View
{
    /// <summary>
    /// The one place obstacle looks come from (board tiles and HUD icons). Uses the approved art
    /// (Resources/ObstacleArt) when it is there; otherwise placeholder shapes drawn in code, tinted.
    /// Art sprites are drawn untinted (white).
    /// </summary>
    public static class ObstacleSprites
    {
        private static bool _loaded;
        private static ObstacleArt _art;

        // Placeholder tints (used only without art).
        private static readonly Color PinataTint = new Color(0.95f, 0.35f, 0.75f, 1f);
        private static readonly Color AnchorTint = new Color(0.35f, 0.42f, 0.55f, 1f);
        private static readonly Color BalloonTint = new Color(0.65f, 0.90f, 1f, 0.45f);
        private static readonly Color[] CrateTints =
        {
            new Color(0.85f, 0.60f, 0.32f, 1f), // 1 layer: light wood
            new Color(0.66f, 0.43f, 0.22f, 1f), // 2 layers
            new Color(0.45f, 0.28f, 0.14f, 1f)  // 3 layers: dark wood
        };

        /// <summary>Wood color of crate planks (for the falling-plank effect).</summary>
        public static readonly Color Wood = new Color(0.62f, 0.40f, 0.20f, 1f);

        /// <summary>Splash color of a popping water balloon.</summary>
        public static readonly Color Water = new Color(0.55f, 0.85f, 1f, 0.9f);

        private static ObstacleArt Art
        {
            get
            {
                if (!_loaded)
                {
                    _loaded = true;
                    _art = Resources.Load<ObstacleArt>(ObstacleArt.ResourcePath);
                }
                return _art;
            }
        }

        /// <summary>Sprite and tint of an obstacle cell (Pinata, Crate with its layers, Anchor).</summary>
        public static Sprite ForCell(Cell cell, out Color tint)
        {
            int layers = cell.Obstacle == ObstacleKind.Crate ? cell.Layers : 1;
            return For(cell.Obstacle, layers, out tint);
        }

        /// <summary>Sprite and tint of an obstacle kind; layers matter only for a Crate (1..3).</summary>
        public static Sprite For(ObstacleKind kind, int layers, out Color tint)
        {
            ObstacleArt art = Art;
            Sprite sprite = null;
            if (art != null)
            {
                switch (kind)
                {
                    case ObstacleKind.Pinata: sprite = art.Pinata; break;
                    case ObstacleKind.Crate: sprite = layers >= 3 ? art.Crate3 : layers == 2 ? art.Crate2 : art.Crate1; break;
                    case ObstacleKind.WaterBalloon: sprite = art.WaterBalloon; break;
                    case ObstacleKind.Anchor: sprite = art.Anchor; break;
                }
            }
            if (sprite != null)
            {
                tint = Color.white;
                return sprite;
            }

            switch (kind)
            {
                case ObstacleKind.Pinata:
                    tint = PinataTint;
                    return ShapeSprites.Disc;
                case ObstacleKind.Crate:
                    tint = CrateTints[Mathf.Clamp(layers, 1, 3) - 1];
                    return ShapeSprites.Square;
                case ObstacleKind.WaterBalloon:
                    tint = BalloonTint;
                    return ShapeSprites.Disc;
                default:
                    tint = AnchorTint;
                    return ShapeSprites.Diamond(true);
            }
        }
    }
}
