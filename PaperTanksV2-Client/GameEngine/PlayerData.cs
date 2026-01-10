using Newtonsoft.Json;
using PaperTanksV2Client.GameEngine.data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class PlayerData
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "Unknown";

        [JsonProperty("score")]
        public float Score { get; set; } = 0;

        [JsonProperty("lastLevel")]
        public string LastLevel { get; set; } = "";

        [JsonProperty("tank")]
        public Tank Tank { get; set; } = null;

        [JsonProperty("weapon0")]
        public Weapon Weapon0 { get; set; } = null;
        [JsonProperty("weapon1")]
        public Weapon Weapon1 { get; set; } = null;
        [JsonProperty("weapon2")]
        public Weapon Weapon2 { get; set; } = null;


        public static PlayerData Load(Game game)
        {
            PlayerData pData = new PlayerData();
            if (game == null) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine("Error Game is null PlayerData.Load");
                return null;
            }
            if (game.resources == null) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine("Error Game.resources is null PlayerData.Load");
                return null;
            }
            try {
                PlayerData player = game.resources.Get(ResourceManagerFormat.Player, "player.json") as PlayerData;
                pData.Name = player.Name;
                pData.Score = player.Score;
                pData.LastLevel = player.LastLevel;
                pData.Tank = player.Tank;
                pData.Weapon0 = player.Weapon0;
                pData.Weapon1 = player.Weapon1;
                pData.Weapon2 = player.Weapon2;
                return pData;
            } catch (JsonException ex) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"JSON parsing error: {ex.Message}");
                throw;
            }
        }
        public static PlayerData NewPlayer(Game game) {
            return new PlayerData();
        }
        public override String ToString()
        {
            return $"{this.Name}, {this.Score}, {this.Tank?.ToString()}, {this.Weapon0?.ToString()}, {this.Weapon1?.ToString()}, {this.Weapon2?.ToString()}";
        }
    }
}
