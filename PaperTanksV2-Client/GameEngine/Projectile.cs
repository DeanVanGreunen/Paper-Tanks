using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Projectile : GameObject
    {
        public float Damage = 10.0f;

        public SKColor color;

        public Projectile(SKColor color)
        {
            this.color = color;
        }

        public override void HandleCollision(Game game, GameObject other)
        {
            if (other == null){
                return;
            }
            bool intersects = this.Bounds.Intersects(other.Bounds);
            if (other is Tank && intersects) {
                this.deleteSelf();
            }
            if (other is Wall) {
                bool intersectsWall = this.Bounds.IntersectsWhenRotated(other.Bounds, other.Rotation);
                if (intersectsWall) {
                    this.Bounds = this.Bounds.GetNonIntersectingPosition(other.Bounds);
                    this.Velocity = new Vector2Data(this.Velocity.X * -1, this.Velocity.Y * -1);
                }
            }
        }

        public override void Update(Single deltaTime)
        {
            this.Bounds.Position = new Vector2Data(this.Bounds.Position.X + ( this.Velocity.X * deltaTime ),
                this.Bounds.Position.Y + ( this.Velocity.Y * deltaTime ));
        }

        protected override ObjectType GetObjectType()
        {
            return ObjectType.Projectile;
        }
        
        public override void Render(Game game, SKCanvas canvas, float? centerX = null, float? centerY = null)
        {
            canvas.DrawRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Size.X, this.Bounds.Size.Y, new SKPaint() {
                Color = this.color
            });
        }
    }
}
