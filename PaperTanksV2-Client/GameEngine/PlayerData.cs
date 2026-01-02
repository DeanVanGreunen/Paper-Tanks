using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class PlayerData
    {
        string name = "Unknown";
        float score = 0;
        string lastLevel = "";
        Tank tank = null;

        public Weapon weapon0 = null;
        public Weapon weapon1 = null;
        public Weapon weapon2 = null;

        public static PlayerData Load(Game game)
        {
            PlayerData pData = new PlayerData();
            if (game == null) {
                Console.WriteLine("Error Game is null PlayerData.Load");
                return null;
            }
            if (game.resources == null) {
                Console.WriteLine("Error Game.resources is null PlayerData.Load");
                return null;
            }
            try {
                PlayerData player = game.resources.Get(ResourceManagerFormat.Player, "player.json") as PlayerData;
                pData.name = player.name;
                pData.score = player.score;
                pData.lastLevel = player.lastLevel;
                pData.tank = player.tank;
                pData.weapon0 = player.weapon0;
                pData.weapon1 = player.weapon1;
                pData.weapon2 = player.weapon2;
                return pData;
            } catch (JsonException ex) {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                throw;
            }
        }
        public static PlayerData NewPlayer(Game game) {
            return new PlayerData();
        }
        public override String ToString()
        {
            return $"{this.name}, {this.score}, {this.tank?.ToString()}, {this.weapon0?.ToString()}, {this.weapon1?.ToString()}, {this.weapon2?.ToString()}";
        }
    }
}
