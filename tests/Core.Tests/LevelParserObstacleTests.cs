using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, LevelParser: "obstacles" field, obstacle goals, error paths and check order.</summary>
    public class LevelParserObstacleTests
    {
        private const string RedGoal = @"[ { ""color"": ""Red"", ""count"": 5 } ]";
        private const string Pinata01 = @"{ ""type"": ""Pinata"", ""x"": 0, ""y"": 1 }";

        /// <summary>4x4 level, 3 colors; a null obstacles argument leaves the field out.</summary>
        private static string Doc(string obstacles, string goals = RedGoal, string moves = "10")
        {
            var parts = new List<string> { @"""width"": 4", @"""height"": 4", @"""colors"": 3", @"""moves"": " + moves };
            if (obstacles != null) parts.Add(@"""obstacles"": " + obstacles);
            if (goals != null) parts.Add(@"""goals"": " + goals);
            return "{ " + string.Join(", ", parts) + " }";
        }

        private static string Fail(string json)
        {
            return Assert.Throws<LevelFormatException>(() => LevelParser.Parse(json)).Message;
        }

        // ---- Valid files ----

        [Fact]
        public void Parse_SpecExample_ReadsObstaclesAndBothGoalKinds()
        {
            const string json = @"{
  ""width"": 9, ""height"": 9, ""colors"": 3, ""moves"": 30,
  ""obstacles"": [
    { ""type"": ""Crate"", ""x"": 0, ""y"": 4, ""layers"": 2 },
    { ""type"": ""Pinata"", ""x"": 4, ""y"": 4 },
    { ""type"": ""WaterBalloon"", ""x"": 2, ""y"": 2 },
    { ""type"": ""Anchor"", ""x"": 6, ""y"": 8 }
  ],
  ""goals"": [
    { ""obstacle"": ""Crate"", ""count"": 1 },
    { ""color"": ""Red"", ""count"": 10 }
  ]
}";
            LevelData level = LevelParser.Parse(json);

            Assert.Equal(new[]
            {
                new ObstaclePlacement(ObstacleKind.Crate, new CellPos(0, 4), 2),
                new ObstaclePlacement(ObstacleKind.Pinata, new CellPos(4, 4), 1),
                new ObstaclePlacement(ObstacleKind.WaterBalloon, new CellPos(2, 2), 1),
                new ObstaclePlacement(ObstacleKind.Anchor, new CellPos(6, 8), 1),
            }, level.Obstacles);
            Assert.Equal(new[] { new ObstacleGoal(ObstacleKind.Crate, 1) }, level.ObstacleGoals);
            Assert.Equal(new[] { new ColorGoal(TileColor.Red, 10) }, level.Goals);
            Assert.Equal(30, level.Moves);
        }

        [Fact]
        public void Parse_CrateWithoutLayers_DefaultsToOne()
        {
            LevelData level = LevelParser.Parse(Doc(@"[ { ""type"": ""Crate"", ""x"": 3, ""y"": 0 } ]"));

            Assert.Equal(new[] { new ObstaclePlacement(ObstacleKind.Crate, new CellPos(3, 0), 1) }, level.Obstacles);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("[]")]
        public void Parse_NoObstaclesOrEmptyArray_NoObstacles(string obstacles)
        {
            LevelData level = LevelParser.Parse(Doc(obstacles));

            Assert.Empty(level.Obstacles);
            Assert.Empty(level.ObstacleGoals);
            Assert.Equal(new[] { new ColorGoal(TileColor.Red, 5) }, level.Goals);
        }

        [Fact]
        public void Parse_OnlyObstacleGoal_IsValid()
        {
            LevelData level = LevelParser.Parse(Doc("[" + Pinata01 + "]", @"[ { ""obstacle"": ""Pinata"", ""count"": 1 } ]"));

            Assert.Empty(level.Goals);
            Assert.Equal(new[] { new ObstacleGoal(ObstacleKind.Pinata, 1) }, level.ObstacleGoals);
        }

        // ---- Error paths ----

        [Theory]
        [InlineData("5", "'obstacles'")]
        [InlineData("[ 5 ]", "'obstacles[0]'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""x"": 0, ""y"": 1, ""z"": 1 } ]", "'obstacles[0].z'")]
        [InlineData(@"[ { ""x"": 0, ""y"": 1 } ]", "'obstacles[0].type'")]
        [InlineData(@"[ { ""type"": 0, ""x"": 0, ""y"": 1 } ]", "'obstacles[0].type'")]
        [InlineData(@"[ { ""type"": ""pinata"", ""x"": 0, ""y"": 1 } ]", "'obstacles[0].type'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""y"": 1 } ]", "'obstacles[0].x'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""x"": 1.5, ""y"": 1 } ]", "'obstacles[0].x'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""x"": 4, ""y"": 1 } ]", "'obstacles[0].x'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""x"": 0, ""y"": 4 } ]", "'obstacles[0].y'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""x"": 0, ""y"": 1, ""layers"": 1 } ]", "'obstacles[0].layers'")]
        [InlineData(@"[ { ""type"": ""Crate"", ""x"": 0, ""y"": 1, ""layers"": 0 } ]", "'obstacles[0].layers'")]
        [InlineData(@"[ { ""type"": ""Crate"", ""x"": 0, ""y"": 1, ""layers"": 4 } ]", "'obstacles[0].layers'")]
        [InlineData(@"[ { ""type"": ""Crate"", ""x"": 0, ""y"": 1, ""layers"": ""2"" } ]", "'obstacles[0].layers'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""x"": 0, ""y"": 1 }, { ""type"": ""Pinata"", ""x"": 9, ""y"": 1 } ]", "'obstacles[1].x'")]
        [InlineData(@"[ { ""type"": ""Pinata"", ""x"": 0, ""y"": 1 }, { ""type"": ""WaterBalloon"", ""x"": 0, ""y"": 1 } ]", "'obstacles[1]'")]
        public void Parse_BadObstacle_MessageHasFieldPath(string obstacles, string path)
        {
            Assert.Contains(path, Fail(Doc(obstacles)));
        }

        [Fact]
        public void Parse_AnchorOnBottomRow_MessageHasYPath()
        {
            string message = Fail(Doc(@"[ { ""type"": ""Anchor"", ""x"": 2, ""y"": 0 } ]"));

            Assert.Contains("'obstacles[0].y'", message);
        }

        [Theory]
        [InlineData(@"[ { ""obstacle"": ""crate"", ""count"": 1 } ]", "'goals[0].obstacle'")]
        [InlineData(@"[ { ""obstacle"": 1, ""count"": 1 } ]", "'goals[0].obstacle'")]
        [InlineData(@"[ { ""obstacle"": ""Pinata"", ""count"": 1 }, { ""obstacle"": ""Pinata"", ""count"": 1 } ]", "'goals[1].obstacle'")]
        [InlineData(@"[ { ""obstacle"": ""Pinata"", ""count"": 2 } ]", "'goals[0].count'")]
        [InlineData(@"[ { ""obstacle"": ""Pinata"", ""count"": 0 } ]", "'goals[0].count'")]
        [InlineData(@"[ { ""obstacle"": ""Anchor"", ""count"": 1 } ]", "'goals[0].count'")]
        public void Parse_BadObstacleGoal_MessageHasFieldPath(string goals, string path)
        {
            Assert.Contains(path, Fail(Doc("[" + Pinata01 + "]", goals)));
        }

        [Theory]
        [InlineData(@"[ { ""color"": ""Red"", ""obstacle"": ""Pinata"", ""count"": 1 } ]")]
        [InlineData(@"[ { ""count"": 1 } ]")]
        public void Parse_GoalWithBothOrNeitherKindField_ThrowsWithGoalPath(string goals)
        {
            Assert.Contains("'goals[0]'", Fail(Doc("[" + Pinata01 + "]", goals)));
        }

        [Fact]
        public void Parse_ObstaclesButEmptyGoals_ThrowsWithGoalsPath()
        {
            Assert.Contains("'goals'", Fail(Doc("[" + Pinata01 + "]", "[]")));
        }

        // ---- Check order ----

        [Theory]
        [InlineData("0", "5", RedGoal, "'moves'", "'obstacles'")]
        [InlineData("10", "5", "[]", "'obstacles'", "'goals'")]
        [InlineData("10", @"[ { ""z"": 1, ""x"": 0, ""y"": 1 } ]", RedGoal, "'obstacles[0].z'", "'obstacles[0].type'")]
        [InlineData("10", @"[ { ""type"": ""Rock"", ""x"": 9, ""y"": 1 } ]", RedGoal, "'obstacles[0].type'", "'obstacles[0].x'")]
        [InlineData("10", @"[ { ""type"": ""Pinata"", ""x"": 9, ""y"": 9 } ]", RedGoal, "'obstacles[0].x'", "'obstacles[0].y'")]
        [InlineData("10", @"[ { ""type"": ""Pinata"", ""x"": 0, ""y"": 9, ""layers"": 2 } ]", RedGoal, "'obstacles[0].y'", "'obstacles[0].layers'")]
        [InlineData("10", @"[ { ""type"": ""Pinata"", ""x"": 0, ""y"": 1 }, { ""type"": ""Crate"", ""x"": 0, ""y"": 1, ""layers"": 4 } ]", RedGoal, "'obstacles[1].layers'", "'obstacles[1]'")]
        [InlineData("10", @"[ { ""type"": ""Pinata"", ""x"": 9, ""y"": 1 }, { ""type"": 3, ""x"": 0, ""y"": 1 } ]", RedGoal, "'obstacles[0].x'", "'obstacles[1]")]
        [InlineData("10", "[" + Pinata01 + "]", @"[ { ""z"": 1, ""color"": ""Red"", ""obstacle"": ""Pinata"", ""count"": 1 } ]", "'goals[0].z'", "'goals[0].count'")]
        [InlineData("10", "[" + Pinata01 + "]", @"[ { ""obstacle"": ""Rock"", ""count"": 0 } ]", "'goals[0].obstacle'", "'goals[0].count'")]
        public void Parse_TwoErrors_FirstInCheckOrderIsReported(string moves, string obstacles, string goals,
            string expected, string notExpected)
        {
            string message = Fail(Doc(obstacles, goals, moves));

            Assert.Contains(expected, message);
            Assert.DoesNotContain(notExpected, message);
        }
    }
}
