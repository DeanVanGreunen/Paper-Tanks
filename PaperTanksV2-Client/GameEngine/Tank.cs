using Cairo;
using Newtonsoft.Json;
using SkiaSharp;
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

        private SKTypeface MenuTypeface = null;
        private SKFont MenuFont = null;
        private SKTypeface SecondMenuTypeface = null;
        private SKFont SecondMenuFont = null;
        
        public Tank(bool isPlayer, Weapon w0, Weapon w1, Weapon w2,
            SKTypeface MenuTypeface,
                SKFont MenuFont,
                SKTypeface SecondMenuTypeface,
                SKFont SecondMenuFont) : base(){
            this.IsPlayer = isPlayer;
            this.Health = 100;
            this.Weapon0 = w0;
            this.Weapon1 = w1;
            this.Weapon2 = w2;
            this.MenuTypeface =  MenuTypeface; 
            this.MenuFont =  MenuFont; 
            this.SecondMenuTypeface =  SecondMenuTypeface; 
            this.SecondMenuFont =  SecondMenuFont;
            this.Bounds = new BoundsData(new Vector2Data(0, 0), new Vector2Data(50, 50));
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

        public override void Render(Game game, SKCanvas canvas, float? centerX = null, float? centerY = null)
        {
            float innerSpacing = 8f;
            float gunXOffset = 18;
            float gunYOffset = 5;
            float gunXSize = 50;
            float gunYSize = 10;
            canvas.DrawRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Size.X, this.Bounds.Size.Y, new SKPaint() {
                Color = this.IsPlayer ? SKColors.Green : SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            });
            canvas.DrawCircle(this.Bounds.Position.X + (this.Bounds.Size.X / 2), this.Bounds.Position.Y + (this.Bounds.Size.Y / 2), (this.Bounds.Size.X / 2) - innerSpacing, new SKPaint() {
                Color = this.IsPlayer ? SKColors.Green : SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            });
            canvas.DrawRect(this.Bounds.Position.X + (this.Bounds.Size.X / 2) + gunXOffset, this.Bounds.Position.Y + (this.Bounds.Size.Y / 2) - gunYOffset, gunXSize, gunYSize, new SKPaint() {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 2f,
            });
            canvas.DrawRect(this.Bounds.Position.X + (this.Bounds.Size.X / 2) + gunXOffset, this.Bounds.Position.Y + (this.Bounds.Size.Y / 2) - gunYOffset, gunXSize, gunYSize, new SKPaint() {
                Color = this.IsPlayer ? SKColors.Green : SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
            });
            if (centerX != null && centerY != null) {
                canvas.RotateDegrees(this.Rotation * -1, (float)centerX, (float)centerY);
                canvas.DrawText(this.Health.ToString(), this.Bounds.Position.X + ( this.Bounds.Size.X / 2 ),
                this.Bounds.Position.Y + ( this.Bounds.Size.Y / 2 ) + 4, new SKPaint() {
                    Color = this.IsPlayer ? SKColors.Green : SKColors.Red,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2f,
                    TextAlign = SKTextAlign.Center
                });
                canvas.RotateDegrees(this.Rotation, (float)centerX, (float)centerY);
            }
        }

        protected override ObjectType GetObjectType()
        {
            return this.IsPlayer ? ObjectType.Player : ObjectType.Enemy;
        }
    }
}
