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

        public PlayerData Load(Game game) {
            try {
                PlayerData player = game.resources.Get(ResourceManagerFormat.Player, "player.json") as PlayerData;
                this.name = player.name;
                this.score = player.score;
                this.lastLevel = player.lastLevel;
                this.tank = player.tank;
                this.weapon0 = player.weapon0;
                this.weapon1 = player.weapon1;
                this.weapon2 = player.weapon2;
            } catch (Exception e) {
                return null;
            }
            return this;
        }
    }
}
