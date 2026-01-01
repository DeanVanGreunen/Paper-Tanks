using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Tank : GameObject
    {
        bool isPlayer = false;
        Weapon weapon0 = null;
        Weapon weapon1 = null;
        Weapon weapon2 = null;
        public Tank(bool isPlayer, Weapon w0, Weapon w1, Weapon w2) {
            this.isPlayer = isPlayer;
            this.Health = 100;
            this.weapon0 = w0;
            this.weapon1 = w1;
            this.weapon2 = w2;
        }
        public override void HandleCollision(GameObject other)
        {
        }

        public override void Update(Single deltaTime)
        {
        }

        protected override ObjectType GetObjectType()
        {
            return this.isPlayer ? ObjectType.Player : ObjectType.Enemy;
        }
    }
}
