using Cairo;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class AmmoPickup : GameObject
    {
        private float AmmoCount = 0;
        
        private SKTypeface MenuTypeface = null;
        private SKFont MenuFont = null;
        private SKTypeface SecondMenuTypeface = null;
        private SKFont SecondMenuFont = null;
        public AmmoPickup(float ammoCount,
            SKTypeface MenuTypeface,
            SKFont MenuFont,
            SKTypeface SecondMenuTypeface,
            SKFont SecondMenuFont) : base(){
            this.AmmoCount = ammoCount;
            this.MenuTypeface =  MenuTypeface; 
            this.MenuFont =  MenuFont; 
            this.SecondMenuTypeface =  SecondMenuTypeface; 
            this.SecondMenuFont =  SecondMenuFont;
            this.Bounds = new BoundsData(new Vector2Data(0, 0), new Vector2Data(82, 42));
        }
        public override void HandleCollision(GameObject other)
        {
            if (other == null) return;
            if (other is Tank ) {
                ( other as Tank ).Weapon0.AmmoCount += (int)this.AmmoCount;
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
                SKRect rect = new SKRect(
                    this.Bounds.Position.X,
                    this.Bounds.Position.Y,
                    this.Bounds.Position.X + this.Bounds.Size.X, // right = left + width
                    this.Bounds.Position.Y + this.Bounds.Size.Y // bottom = top + height
                );
                canvas.DrawRect(rect, new SKPaint() {
                    Color = SKColors.Orange,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2f,
                    TextAlign = SKTextAlign.Center
                });
                SKPaint textPaint = new SKPaint() {
                    Color = SKColors.Orange,
                    TextAlign = SKTextAlign.Center,
                    Typeface = this.MenuTypeface,
                    TextSize = 28 // adjust as needed
                };
                // Calculate center position
                float centerX1 = rect.MidX;
                float centerY1 = rect.MidY - (textPaint.FontMetrics.Ascent + textPaint.FontMetrics.Descent) / 2;

                canvas.DrawText("Ammo", centerX1, centerY1, textPaint);
                canvas.RotateDegrees(this.Rotation, (float)centerX, (float)centerY);
            }
        }

        protected override ObjectType GetObjectType()
        {
            return ObjectType.Pickup;
        }
    }
}
