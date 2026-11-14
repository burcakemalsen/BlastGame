using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class LevelParserTests
    {
        private const string DefaultGoals = @"[ { ""color"": ""Red"", ""count"": 5 } ]";

        /// <summary>Builds a level document; a null argument leaves that field out.</summary>
        private static string Doc(string width = "9", string height = "9", string colors = "4",
            string moves = "20", string goals = DefaultGoals)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (width != null) parts.Add(@"""width"": " + width);
            if (height != null) parts.Add(@"""height"": " + height);
            if (colors != null) parts.Add(@"""colors"": " + colors);
            if (moves != null) parts.Add(@"""moves"": " + moves);
            if (goals != null) parts.Add(@"""goals"": " + goals);
            return "{ " + string.Join(", ", parts) + " }";
        }

        private static string FailureMessage(string json)
        {
            return Assert.Throws<LevelFormatException>(() => LevelParser.Parse(json)).Message;
        }

        // ---- Örnek 1, null ----

        [Fact]
        public void Parse_ValidFile_ReturnsAllValuesInFileOrder()
        {
            const string json = @"
{
  ""width"": 9,
  ""height"": 9,
  ""colors"": 4,
  ""moves"": 20,
  ""goals"": [
    { ""color"": ""Red"", ""count"": 20 },
    { ""color"": ""Blue"", ""count"": 20 }
  ]
}";
            LevelData level = LevelParser.Parse(json);

            Assert.Equal(9, level.Width);
            Assert.Equal(9, level.Height);
            Assert.Equal(4, level.ColorCount);
            Assert.Equal(20, level.Moves);
            Assert.Equal(new[] { new ColorGoal(TileColor.Red, 20), new ColorGoal(TileColor.Blue, 20) }, level.Goals);
        }

        [Fact]
        public void Parse_NullJson_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => LevelParser.Parse(null));
            Assert.Equal("json", ex.ParamName);
        }

        // ---- Sözdizimi (Örnek 7) ----

        [Theory]
        [InlineData("")]
        [InlineData("   \n ")]
        [InlineData(@"{ ""width"": 9, ")]
        [InlineData(@"{ ""width"": 9 } extra")]
        [InlineData(@"{ ""width"": 9, ""width"": 9 }")]
        [InlineData(@"{ ""width"": 9, ""height"": 9, ""colors"": 4, ""moves"": 20, ""goals"": [ { ""color"": ""Red"", ""count"": 5, ""count"": 6 } ] }")]
        public void Parse_SyntaxError_ThrowsLevelFormatExceptionWithInnerException(string json)
        {
            var ex = Assert.Throws<LevelFormatException>(() => LevelParser.Parse(json));
            Assert.NotNull(ex.InnerException);
        }

        // ---- Kök (Örnek 6) ----

        [Theory]
        [InlineData("[1, 2]")]
        [InlineData("5")]
        [InlineData(@"""text""")]
        public void Parse_RootIsNotObject_ThrowsLevelFormatException(string json)
        {
            Assert.Throws<LevelFormatException>(() => LevelParser.Parse(json));
        }

        // ---- Zorunlu alanlar (Örnek 2) ----

        [Theory]
        [InlineData("width")]
        [InlineData("height")]
        [InlineData("colors")]
        [InlineData("moves")]
        [InlineData("goals")]
        public void Parse_MissingField_MessageContainsFieldPath(string field)
        {
            string json = Doc(
                width: field == "width" ? null : "9",
                height: field == "height" ? null : "9",
                colors: field == "colors" ? null : "4",
                moves: field == "moves" ? null : "20",
                goals: field == "goals" ? null : DefaultGoals);

            Assert.Contains("'" + field + "'", FailureMessage(json));
        }

        // ---- Tür hataları ----

        [Theory]
        [InlineData("9.0")]
        [InlineData("9.5")]
        [InlineData(@"""9""")]
        [InlineData("true")]
        [InlineData("null")]
        [InlineData("{}")]
        [InlineData("2147483648")]
        public void Parse_WidthWrongTypeOrOverflow_MessageContainsWidth(string value)
        {
            Assert.Contains("'width'", FailureMessage(Doc(width: value)));
        }

        [Theory]
        [InlineData("height")]
        [InlineData("colors")]
        [InlineData("moves")]
        public void Parse_OtherNumericFieldWithDecimal_MessageContainsFieldPath(string field)
        {
            string json = Doc(
                height: field == "height" ? "9.5" : "9",
                colors: field == "colors" ? "9.5" : "4",
                moves: field == "moves" ? "9.5" : "20");

            Assert.Contains("'" + field + "'", FailureMessage(json));
        }

        // ---- Aralıklar (Örnek 3) ----

        [Theory]
        [InlineData("width", "0")]
        [InlineData("width", "17")]
        [InlineData("height", "0")]
        [InlineData("height", "17")]
        [InlineData("colors", "0")]
        [InlineData("colors", "7")]
        [InlineData("moves", "0")]
        [InlineData("moves", "-1")]
        public void Parse_NumberOutOfRange_MessageContainsFieldPath(string field, string value)
        {
            string json = Doc(
                width: field == "width" ? value : "9",
                height: field == "height" ? value : "9",
                colors: field == "colors" ? value : "4",
                moves: field == "moves" ? value : "20");

            Assert.Contains("'" + field + "'", FailureMessage(json));
        }

        [Theory]
        [InlineData(1, 1, 1, 1)]
        [InlineData(16, 16, 6, 1)]
        public void Parse_BoundaryValues_AreAccepted(int width, int height, int colors, int moves)
        {
            LevelData level = LevelParser.Parse(Doc(width.ToString(), height.ToString(), colors.ToString(), moves.ToString()));

            Assert.Equal((width, height, colors, moves), (level.Width, level.Height, level.ColorCount, level.Moves));
        }

        // ---- goals ----

        [Theory]
        [InlineData(@"{}", "'goals'")]
        [InlineData(@"[]", "'goals'")]
        [InlineData(@"[5]", "'goals[0]'")]
        [InlineData(@"[ { ""count"": 5 } ]", "'goals[0].color'")]
        [InlineData(@"[ { ""color"": 0, ""count"": 5 } ]", "'goals[0].color'")]
        [InlineData(@"[ { ""color"": ""red"", ""count"": 5 } ]", "'goals[0].color'")]
        [InlineData(@"[ { ""color"": ""Pink"", ""count"": 5 } ]", "'goals[0].color'")]
        [InlineData(@"[ { ""color"": ""Red"" } ]", "'goals[0].count'")]
        [InlineData(@"[ { ""color"": ""Red"", ""count"": 0 } ]", "'goals[0].count'")]
        [InlineData(@"[ { ""color"": ""Red"", ""count"": 2.5 } ]", "'goals[0].count'")]
        [InlineData(@"[ { ""color"": ""Red"", ""count"": 5, ""extra"": 1 } ]", "'goals[0].extra'")]
        [InlineData(@"[ { ""color"": ""Red"", ""count"": 5 }, { ""color"": ""Green"", ""count"": 0 } ]", "'goals[1].count'")]
        [InlineData(@"[ { ""color"": ""Red"", ""count"": 5 }, { ""color"": ""red"", ""count"": 5 } ]", "'goals[1].color'")]
        [InlineData(@"[ { ""color"": ""Red"", ""count"": 5 }, { ""color"": ""Red"", ""count"": 5 } ]", "'goals[1].color'")]
        public void Parse_BadGoals_MessageContainsGoalPath(string goals, string expectedPath)
        {
            Assert.Contains(expectedPath, FailureMessage(Doc(goals: goals)));
        }

        [Fact]
        public void Parse_GoalColorAtLastIndex_IsAccepted()
        {
            LevelData level = LevelParser.Parse(Doc(colors: "3", goals: @"[ { ""color"": ""Blue"", ""count"": 5 } ]"));

            Assert.Equal(new[] { new ColorGoal(TileColor.Blue, 5) }, level.Goals);
        }

        [Fact]
        public void Parse_GoalColorBeyondColors_MessageContainsGoalColorPath()
        {
            string message = FailureMessage(Doc(colors: "3", goals: @"[ { ""color"": ""Yellow"", ""count"": 5 } ]"));

            Assert.Contains("'goals[0].color'", message);
        }

        // ---- Bilinmeyen kök alanı ----

        [Fact]
        public void Parse_UnknownRootField_MessageContainsFieldName()
        {
            const string json = @"{ ""width"": 9, ""height"": 9, ""colors"": 4, ""move"": 20, ""goals"": [ { ""color"": ""Red"", ""count"": 5 } ] }";

            Assert.Contains("'move'", FailureMessage(json));
        }

        // ---- Denetim sırası ----

        [Fact]
        public void Parse_WidthAndMovesBothInvalid_ReportsWidth()
        {
            string message = FailureMessage(Doc(width: "17", moves: "0"));

            Assert.Contains("'width'", message);
            Assert.DoesNotContain("'moves'", message);
        }

        [Fact]
        public void Parse_UnknownRootFieldAndBadWidth_ReportsUnknownFieldFirst()
        {
            const string json = @"{ ""width"": ""x"", ""bogus"": 1, ""height"": 9, ""colors"": 4, ""moves"": 20, ""goals"": [ { ""color"": ""Red"", ""count"": 5 } ] }";

            string message = FailureMessage(json);

            Assert.Contains("'bogus'", message);
            Assert.DoesNotContain("'width'", message);
        }

        [Fact]
        public void Parse_MissingMovesAndEmptyGoals_ReportsMovesFirst()
        {
            string message = FailureMessage(Doc(moves: null, goals: "[]"));

            Assert.Contains("'moves'", message);
            Assert.DoesNotContain("'goals'", message);
        }

        [Fact]
        public void Parse_BadColorAndBadCountInSameGoal_ReportsColorFirst()
        {
            string message = FailureMessage(Doc(goals: @"[ { ""color"": ""red"", ""count"": 0 } ]"));

            Assert.Contains("'goals[0].color'", message);
            Assert.DoesNotContain("'goals[0].count'", message);
        }
    }
}
