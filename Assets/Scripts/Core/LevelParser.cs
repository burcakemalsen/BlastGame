using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlastGame.Core
{
    /// <summary>Reads and validates a level from JSON text.</summary>
    public static class LevelParser
    {
        private static readonly string[] RootFields = { "width", "height", "colors", "moves", "obstacles", "goals" };
        private static readonly string[] GoalFields = { "color", "obstacle", "count" };
        private static readonly string[] ObstacleFields = { "type", "x", "y", "layers" };

        public static LevelData Parse(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            JToken root = ReadToken(json);
            var obj = root as JObject;
            if (obj == null)
                throw new LevelFormatException("The level file must contain a JSON object.");

            foreach (JProperty property in obj.Properties())
            {
                if (Array.IndexOf(RootFields, property.Name) < 0)
                    throw new LevelFormatException("Unknown field '" + property.Name + "'.");
            }

            int width = ReadInt(obj, "width", "width", Grid.MinSize, Grid.MaxSize);
            int height = ReadInt(obj, "height", "height", Grid.MinSize, Grid.MaxSize);
            int colors = ReadInt(obj, "colors", "colors", 1, GridFactory.MaxColorCount);
            int moves = ReadInt(obj, "moves", "moves", 1, int.MaxValue);

            var obstacles = new List<ObstaclePlacement>();
            JToken obstaclesToken;
            if (obj.TryGetValue("obstacles", out obstaclesToken))
            {
                var obstaclesArray = obstaclesToken as JArray;
                if (obstaclesArray == null)
                    throw new LevelFormatException("Field 'obstacles' must be an array.");
                var taken = new bool[width, height];
                for (int i = 0; i < obstaclesArray.Count; i++)
                    obstacles.Add(ReadObstacle(obstaclesArray[i], i, width, height, taken));
            }

            JToken goalsToken;
            if (!obj.TryGetValue("goals", out goalsToken))
                throw new LevelFormatException("Missing field 'goals'.");
            var goalsArray = goalsToken as JArray;
            if (goalsArray == null)
                throw new LevelFormatException("Field 'goals' must be an array.");
            if (goalsArray.Count == 0)
                throw new LevelFormatException("Field 'goals' must have at least one goal.");

            var goals = new List<ColorGoal>();
            var obstacleGoals = new List<ObstacleGoal>();
            var seen = new HashSet<TileColor>();
            var seenObstacles = new HashSet<ObstacleKind>();
            for (int i = 0; i < goalsArray.Count; i++)
                ReadGoal(goalsArray[i], i, colors, obstacles, seen, seenObstacles, goals, obstacleGoals);

            return new LevelData(width, height, colors, moves, goals, obstacles, obstacleGoals);
        }

        private static JToken ReadToken(string json)
        {
            try
            {
                using (var reader = new JsonTextReader(new StringReader(json)))
                {
                    reader.DateParseHandling = DateParseHandling.None;
                    var settings = new JsonLoadSettings
                    {
                        DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
                    };
                    JToken token = JToken.ReadFrom(reader, settings);
                    while (reader.Read())
                    {
                        if (reader.TokenType != JsonToken.Comment)
                            throw new JsonReaderException("Additional text found after the JSON value.", reader.Path, reader.LineNumber, reader.LinePosition, null);
                    }
                    return token;
                }
            }
            catch (JsonException ex)
            {
                throw new LevelFormatException("Invalid JSON: " + ex.Message, ex);
            }
        }

        private static int ReadInt(JObject obj, string name, string path, int min, int max)
        {
            JToken token;
            if (!obj.TryGetValue(name, out token))
                throw new LevelFormatException("Missing field '" + path + "'.");
            return CheckInt(token, path, min, max);
        }

        private static int CheckInt(JToken token, string path, int min, int max)
        {
            var value = token as JValue;
            if (value == null || token.Type != JTokenType.Integer || !(value.Value is long))
                throw new LevelFormatException("Field '" + path + "' must be an integer.");
            long number = (long)value.Value;
            if (number < int.MinValue || number > int.MaxValue)
                throw new LevelFormatException("Field '" + path + "' must be an integer.");
            if (number < min || number > max)
            {
                if (max == int.MaxValue)
                    throw new LevelFormatException("Field '" + path + "' must be at least " + min + ", got " + number + ".");
                throw new LevelFormatException("Field '" + path + "' must be between " + min + " and " + max + ", got " + number + ".");
            }
            return (int)number;
        }

        private static ObstaclePlacement ReadObstacle(JToken token, int index, int width, int height, bool[,] taken)
        {
            string prefix = "obstacles[" + index + "]";
            var obj = token as JObject;
            if (obj == null)
                throw new LevelFormatException("Field '" + prefix + "' must be an object.");

            foreach (JProperty property in obj.Properties())
            {
                if (Array.IndexOf(ObstacleFields, property.Name) < 0)
                    throw new LevelFormatException("Unknown field '" + prefix + "." + property.Name + "'.");
            }

            ObstacleKind kind = ReadObstacleKind(obj, "type", prefix + ".type");
            int x = ReadInt(obj, "x", prefix + ".x", 0, width - 1);
            int y = ReadInt(obj, "y", prefix + ".y", 0, height - 1);

            int layers = 1;
            JToken layersToken;
            if (obj.TryGetValue("layers", out layersToken))
            {
                string layersPath = prefix + ".layers";
                if (kind != ObstacleKind.Crate)
                    throw new LevelFormatException("Field '" + layersPath + "' is only allowed for a Crate.");
                layers = CheckInt(layersToken, layersPath, 1, Cell.MaxLayers(ObstacleKind.Crate));
            }

            if (kind == ObstacleKind.Anchor && y == 0)
                throw new LevelFormatException("Field '" + prefix + ".y' cannot be 0 for an Anchor.");
            if (taken[x, y])
                throw new LevelFormatException("Obstacle '" + prefix + "' is on a cell that already has one.");
            taken[x, y] = true;

            return new ObstaclePlacement(kind, new CellPos(x, y), layers);
        }

        private static ObstacleKind ReadObstacleKind(JObject obj, string name, string path)
        {
            JToken token;
            if (!obj.TryGetValue(name, out token))
                throw new LevelFormatException("Missing field '" + path + "'.");
            if (token.Type != JTokenType.String)
                throw new LevelFormatException("Field '" + path + "' must be a string.");
            string text = (string)token;
            foreach (ObstacleKind candidate in (ObstacleKind[])Enum.GetValues(typeof(ObstacleKind)))
            {
                if (candidate.ToString() == text)
                    return candidate;
            }
            throw new LevelFormatException("Field '" + path + "' has unknown obstacle '" + text + "'.");
        }

        private static void ReadGoal(JToken token, int index, int colors, List<ObstaclePlacement> obstacles,
                                     HashSet<TileColor> seen, HashSet<ObstacleKind> seenObstacles,
                                     List<ColorGoal> goals, List<ObstacleGoal> obstacleGoals)
        {
            string prefix = "goals[" + index + "]";
            var obj = token as JObject;
            if (obj == null)
                throw new LevelFormatException("Field '" + prefix + "' must be an object.");

            foreach (JProperty property in obj.Properties())
            {
                if (Array.IndexOf(GoalFields, property.Name) < 0)
                    throw new LevelFormatException("Unknown field '" + prefix + "." + property.Name + "'.");
            }

            bool hasColor = obj.ContainsKey("color");
            bool hasObstacle = obj.ContainsKey("obstacle");
            if (hasColor == hasObstacle)
                throw new LevelFormatException("Goal '" + prefix + "' needs exactly one of '" + prefix + ".color' and '" + prefix + ".obstacle'.");

            if (hasObstacle)
            {
                string obstaclePath = prefix + ".obstacle";
                ObstacleKind kind = ReadObstacleKind(obj, "obstacle", obstaclePath);
                if (!seenObstacles.Add(kind))
                    throw new LevelFormatException("Obstacle goal '" + kind + "' at '" + obstaclePath + "' is given twice.");
                int placed = 0;
                foreach (ObstaclePlacement p in obstacles)
                    if (p.Kind == kind)
                        placed++;
                string countPath = prefix + ".count";
                int obstacleCount = ReadInt(obj, "count", countPath, 1, int.MaxValue);
                if (obstacleCount > placed)
                    throw new LevelFormatException("Field '" + countPath + "' is " + obstacleCount + ", but only " + placed + " " + kind + " placed.");
                obstacleGoals.Add(new ObstacleGoal(kind, obstacleCount));
                return;
            }

            string colorPath = prefix + ".color";
            JToken colorToken = obj["color"];
            if (colorToken.Type != JTokenType.String)
                throw new LevelFormatException("Field '" + colorPath + "' must be a string.");
            string colorName = (string)colorToken;

            bool known = false;
            TileColor color = TileColor.Red;
            foreach (TileColor candidate in (TileColor[])Enum.GetValues(typeof(TileColor)))
            {
                if (candidate.ToString() == colorName)
                {
                    known = true;
                    color = candidate;
                    break;
                }
            }
            if (!known)
                throw new LevelFormatException("Field '" + colorPath + "' has unknown color '" + colorName + "'.");
            if ((int)color >= colors)
                throw new LevelFormatException("Goal color '" + color + "' at '" + colorPath + "' is not among the first " + colors + " colors.");
            if (!seen.Add(color))
                throw new LevelFormatException("Goal color '" + color + "' at '" + colorPath + "' is given twice.");

            int count = ReadInt(obj, "count", prefix + ".count", 1, int.MaxValue);
            goals.Add(new ColorGoal(color, count));
        }
    }
}
