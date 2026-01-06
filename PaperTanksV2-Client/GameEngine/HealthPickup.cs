using Cairo;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class HealthPickup : GameObject
    {
        private SKImage HeartImage = null;
        
        public HealthPickup(float Health, SKImage HeartImage) : base(){
            this.Health = Health;
            this.HeartImage = HeartImage;
            this.Bounds = new BoundsData(new Vector2Data(0, 0), new Vector2Data(42, 42));
        }
        public override void HandleCollision(GameObject other)
        {
            if (other == null) return;
            if (!( other is Tank )) {
                ( other as Tank ).Health += this.Health;
                this.deleteSelf();
                return;
            }
        }

        public override void Update(Single deltaTime)
        {
        }

        public override void Render(Game game, SKCanvas canvas, float? centerX = null, float? centerY = null)
        {
            if (centerX != null && centerY != null) {
                canvas.RotateDegrees(this.Rotation * -1, (float)centerX, (float)centerY);
                canvas.DrawImage(this.HeartImage, new SKRect(
                    this.Bounds.Position.X, 
                    this.Bounds.Position.Y, 
                    this.Bounds.Position.X + this.Bounds.Size.X,  // right = left + width
                    this.Bounds.Position.Y + this.Bounds.Size.Y   // bottom = top + height
                ), new SKPaint() {
                });
                canvas.RotateDegrees(this.Rotation, (float)centerX, (float)centerY);
            }
        }

        protected override ObjectType GetObjectType()
        {
            return ObjectType.Pickup;
        }
    }
}
