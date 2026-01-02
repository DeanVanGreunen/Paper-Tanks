using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Tank : GameObject
    {
        [JsonProperty("isPlayer")]
        public bool IsPlayer { get; set; } = false;

        [JsonProperty("weapon0")]
        public Weapon Weapon0 { get; set; } = null;

        [JsonProperty("weapon1")]
        public Weapon Weapon1 { get; set; } = null;

        [JsonProperty("weapon2")]
        public Weapon Weapon2 { get; set; } = null;


        public Tank(bool isPlayer, Weapon w0, Weapon w1, Weapon w2) {
            this.IsPlayer = isPlayer;
            this.Health = 100;
            this.Weapon0 = w0;
            this.Weapon1 = w1;
            this.Weapon2 = w2;
        }
        public override void HandleCollision(GameObject other)
        {
            if (other == null) return;
            if (!( other is Projectile )) {
                this.Health -= ( other as Projectile ).Damage;
                this.deleteSelf();
                return;
            }
        }

        public override void Update(Single deltaTime)
        {
        }

        protected override ObjectType GetObjectType()
        {
            return this.IsPlayer ? ObjectType.Player : ObjectType.Enemy;
        }
    }
}
