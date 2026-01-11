using PaperTanksV2Client.GameEngine.data;
using PaperTanksV2Client.GameEngine.Server.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.GameEngine
{
    public class Wall : GameObject
    {
        public Wall(int x, int y, int w, int h, int angle) : base()
        {
            this.Bounds = new BoundsData(new Vector2Data(x, y), new Vector2Data(w, h));
            this.Rotation = angle;
        }
        protected override ObjectClassType GetObjectClassType() => ObjectClassType.Wall;
        public override void HandleCollision(Game game, GameObject other)
        {
            if (other == null){
                return;
            }
        }
        
        public override void Render(Game game, SKCanvas canvas, float? centerX = null, float? centerY = null)
        {
            var rect = new SKRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Position.X + this.Bounds.Size.X, this.Bounds.Position.Y + this.Bounds.Size.Y);
            var paint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.Save();
            if (centerX != null && centerY != null) {
                // Rotate the canvas 45 degrees around the rectangle's center
                canvas.RotateDegrees(-this.Rotation, (float)centerX, (float)centerY);
            }

            // Draw the rectangle
            canvas.DrawRect(rect, paint);
    
            // Restore the canvas state
            canvas.Restore();
        }
        public override byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            byte[] typeId = BinaryHelper.GetBytesBigEndian(GetObjectClassType());
            Console.WriteLine($"[Serialize] {this.GetType().Name} - TypeID: {typeId}, Expected: {GetObjectClassType()}");
            bytes.AddRange(typeId);

            bytes.AddRange(base.GetBytes());
            return bytes.ToArray();
        }
        
    }
}