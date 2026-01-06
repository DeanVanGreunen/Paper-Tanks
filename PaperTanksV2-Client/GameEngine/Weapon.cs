using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Weapon : GameObject
    {
        [JsonProperty("Power")]
        public float Power { get; set; }
        public Shape Shape { get; set; }
        [JsonProperty("AmmoCount")]
        public int AmmoCount { get; set; }
        public Weapon(float Power, int AmmoCount) {
            this.Shape = Shape;
            this.Power = Power;
            this.AmmoCount = AmmoCount;
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
        public override void Render(Game game, SKCanvas canvas, float? centerX = null, float? centerY = null)
        {
            canvas.DrawRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Size.X, this.Bounds.Size.Y, new SKPaint() {
                Color = SKColors.Red
            });
        }
        
    }
}
