using SkiaSharp;
using System;
namespace PaperTanksV2Client.UI
{
    class TextWithRotation : MenuItem
    {
        string text;
        int x;
        int y;
        int w;
        int h;
        float angle;
        SKColor fontColor;
        SKFont font;
        SKTypeface face;
        SKPaint paint = null;
        public SKPaint hoverPaint = null;
        public bool isHover = false;
        public TextWithRotation(string text, int x, int y, SKColor fontColor, SKTypeface face, SKFont font, float fontSize, SKTextAlign align, float angle) : base()
        {
            this.text = text;
            this.x = x;
            this.y = y;
            this.fontColor = fontColor;
            this.font = font;
            this.face = face;
            this.angle = angle;
            this.paint = new SKPaint {
                Color = fontColor,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            };
            this.hoverPaint = new SKPaint {
                Color = fontColor,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            };
            SKRect textBounds = new SKRect();
            this.paint.MeasureText(text, ref textBounds);
            this.w = (int) Math.Ceiling(textBounds.Width);
            this.h = (int) Helper.GetSingleLineHeight(this.paint);
        }

        public void updateText(string text)
        {
            this.text = text;
        }
        public void Dispose()
        {
        }

        public void Input(Game game)
        {
            this.isHover =
                   game.mouse.ScaledMousePosition.X >= this.x &&
                   game.mouse.ScaledMousePosition.X < ( this.x + this.w ) &&
                   game.mouse.ScaledMousePosition.Y >= this.y &&
                   game.mouse.ScaledMousePosition.Y < ( this.y + this.h );
        }

        /*public void Render(Game game, SKCanvas canvas)
        {
            canvas.Save();
            var metrics = paint.FontMetrics;
            canvas.Translate(x, y);
            canvas.RotateDegrees(this.angle);
            float yAdjusted = y + ( -metrics.Ascent );
            canvas.DrawText(text, x, yAdjusted, isHover ? hoverPaint : paint);
            canvas.Restore();
        }*/
        public void Render(Game game, SKCanvas canvas)
        {
            canvas.Save();
            var metrics = paint.FontMetrics;
            canvas.Translate(x, y);
            canvas.RotateDegrees(this.angle);
            float yAdjusted = -metrics.Ascent;  // Changed: removed 'y +' since we're already at that position
            canvas.DrawText(text, 0, yAdjusted, isHover ? hoverPaint : paint);  // Changed: draw at (0, yAdjusted) not (x, yAdjusted)
            canvas.Restore();
        }
    }
}
