using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Projectile : GameObject
    {
        public float Damage = 10.0f;

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
        
        public override void Render(Game game, SKCanvas canvas)
        {
            canvas.DrawRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Size.X, this.Bounds.Size.Y, new SKPaint() {
                Color = SKColors.Red
            });
        }
    }
}
