using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Weapon : GameObject
    {
        public float Power = 10.0f;
        public Shape shape;
        public Weapon(Shape shape, float Power) {
        }

        public override void HandleCollision(GameObject other)
        {
        }

        public override void Update(Single deltaTime)
        {
        }

        protected override ObjectType GetObjectType()
        {
            return ObjectType.Weapon;
        }
    }
}
