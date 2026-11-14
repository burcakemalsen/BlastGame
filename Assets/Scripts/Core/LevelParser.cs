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
        private static readonly string[] RootFields = { "width", "height", "colors", "moves", "goals" };
        private static readonly string[] GoalFields = { "color", "count" };

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

            JToken goalsToken;
            if (!obj.TryGetValue("goals", out goalsToken))
                throw new LevelFormatException("Missing field 'goals'.");
            var goalsArray = goalsToken as JArray;
            if (goalsArray == null)
                throw new LevelFormatException("Field 'goals' must be an array.");
            if (goalsArray.Count == 0)
                throw new LevelFormatException("Field 'goals' must have at least one goal.");

            var goals = new List<ColorGoal>();
            var seen = new HashSet<TileColor>();
            for (int i = 0; i < goalsArray.Count; i++)
                goals.Add(ReadGoal(goalsArray[i], i, colors, seen));

            return new LevelData(width, height, colors, moves, goals);
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
                            throw new JsonReaderException("Additional text found in JSON string after parsing content.");
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

        private static ColorGoal ReadGoal(JToken token, int index, int colors, HashSet<TileColor> seen)
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

            string colorPath = prefix + ".color";
            JToken colorToken;
            if (!obj.TryGetValue("color", out colorToken))
                throw new LevelFormatException("Missing field '" + colorPath + "'.");
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
            return new ColorGoal(color, count);
        }
    }
}
