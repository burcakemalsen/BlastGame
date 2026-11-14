using UnityEngine;

namespace BlastGame.View
{
    /// <summary>
    /// The obstacle sprites from Assets/Art/Obstacles, gathered in one asset that the game loads from
    /// Resources (Resources/ObstacleArt.asset). The editor tool ObstacleArtImporter makes the PNGs and this
    /// asset; when it is missing the game draws its placeholder shapes instead (see ObstacleSprites).
    /// </summary>
    public sealed class ObstacleArt : ScriptableObject
    {
        public const string ResourcePath = "ObstacleArt";

        public Sprite Pinata;
        public Sprite Crate1;
        public Sprite Crate2;
        public Sprite Crate3;
        public Sprite WaterBalloon;
        public Sprite Anchor;
    }
}
