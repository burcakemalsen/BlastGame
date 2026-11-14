using BlastGame.Core;

namespace BlastGame.View
{
    /// <summary>Names one goal of the HUD bar: a color goal or an obstacle goal.</summary>
    public readonly struct GoalRef
    {
        public readonly bool IsObstacle;
        public readonly TileColor Color;   // color goals
        public readonly ObstacleKind Kind; // obstacle goals

        private GoalRef(bool isObstacle, TileColor color, ObstacleKind kind)
        {
            IsObstacle = isObstacle;
            Color = color;
            Kind = kind;
        }

        public static GoalRef ForColor(TileColor color)
        {
            return new GoalRef(false, color, default(ObstacleKind));
        }

        public static GoalRef ForObstacle(ObstacleKind kind)
        {
            return new GoalRef(true, default(TileColor), kind);
        }
    }
}
