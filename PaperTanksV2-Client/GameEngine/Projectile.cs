using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Projectile : GameObject
    {
        public override void HandleCollision(GameObject other)
        {
        }

        public override void Update(Single deltaTime)
        {
        }

        protected override ObjectType GetObjectType()
        {
            return ObjectType.Projectile;
        }
    }
}
