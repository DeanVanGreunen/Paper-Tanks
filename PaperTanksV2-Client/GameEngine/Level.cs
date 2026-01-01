using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Level
    {
        /// <summary>
        // Level Details
        /// </summary>
        public bool isMultiplayer;
        public string levelName;
        /// <summary>
        // Enemies
        /// </summary>
        public GameObject[] gameObjects;
        /// <summary>
        // Player Positions
        /// </summary>
        public Vector2 playerPosition;
        /// <summary>
        /// Multi Player Spawn Points
        /// </summary>
        public Vector2[] playerSpawnPoints;
    }
}
