using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Level
    {
        [JsonIgnore]
        public string fileName;
        /// <summary>
        // Level Details
        /// </summary>
        [JsonProperty("isMultiplayer")]
        public bool isMultiplayer;
        [JsonProperty("levelName")]
        public string levelName;
        /// <summary>
        // Enemies
        /// </summary>
        [JsonProperty("gameObjects")]
        public List<GameObject> gameObjects;
        /// <summary>
        // Player Positions
        /// </summary>
        [JsonProperty("playerPosition")]
        public Vector2Data playerPosition;
        /// <summary>
        /// Multi Player Spawn Points
        /// </summary>
        [JsonProperty("playerSpawnPoints")]
        public List<Vector2Data> playerSpawnPoints;

        public static bool Save(Game game, Level level, string filename)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            try {
                string levelPath = game.resources.GetResourcePath(ResourceManagerFormat.Level, filename);
                string json = JsonConvert.SerializeObject(level, settings);
                File.WriteAllText(levelPath, json, Encoding.UTF8);
                return File.Exists(levelPath);
            } catch(Exception e) {
                return false;
            }
        }

        public static bool DeleteLevel(Game game, string filename)
        {
            try {
                string levelPath = game.resources.GetResourcePath(ResourceManagerFormat.Level, filename);
                if (File.Exists(levelPath)) {
                    File.Delete(@levelPath);
                    return true;
                } else {
                    return true;
                }
            } catch(Exception e) {
                return false;
            }
        }

        public static Level Load(Game game, string filename)
        {
            try {
                if (!game.resources.Load(ResourceManagerFormat.Level, filename)) {
                    return null;
                }
                Level level = game.resources.Get(ResourceManagerFormat.Level, filename) as Level;
                return level;
            } catch(Exception e) {
                return null;
            }
        }
    }
}
