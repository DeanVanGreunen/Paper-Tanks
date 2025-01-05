using SkiaSharp;
using System;
namespace PaperTanksV2Client.UI
{
    class Button : MenuItem
    {
        string text;
        int x;
        int y;
        int w;
        int h;
        SKColor fontColor;
        SKColor fontHoverColor;
        SKTypeface face;
        SKFont font;
        readonly Action<GameEngine> callback = null;
        bool isHover = false;
        bool isClicked = false;
        bool isStroked = false;
        SKPaint paint = null;
        SKPaint paintHover = null;
        public Button(string text, int x, int y, SKColor fontColor, SKColor fontHoverColor, SKTypeface face, SKFont font, float fontSize, SKTextAlign align, Action<GameEngine> callback, bool isStroked = false) : base()
        {
            this.text = text;
            this.x = x;
            this.y = y;
            this.fontColor = fontColor;
            this.fontHoverColor = fontHoverColor;
            this.font = font;
            this.face = face;
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.isHover = false;
            this.isClicked = false;
            this.isStroked = isStroked;
            this.paint = new SKPaint {
                Color = fontColor,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            };
            this.paintHover = new SKPaint {
                Color = fontHoverColor,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            };
            SKRect textBounds = new SKRect();
            this.paint.MeasureText(text, ref textBounds);
            SKFontMetrics metrics;
            paint.GetFontMetrics(out metrics);
            this.w = (int) Math.Ceiling(textBounds.Width);
            this.h = (int) Helper.GetSingleLineHeight(this.paint);

        }

        public void Dispose()
        {
        }

        public void Input(GameEngine game)
        {
            // show if hovered
            this.isHover =
                game.mouse.ScaledMousePosition.X >= this.x &&
                game.mouse.ScaledMousePosition.X < ( this.x + this.w ) &&
                game.mouse.ScaledMousePosition.Y >= this.y &&
                game.mouse.ScaledMousePosition.Y < ( this.y + this.h );

            // if on next frame and is clicked and button release, then mark as unclicked
            if (this.isClicked == true && !game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = false;
            } else if (this.isHover == true && this.isClicked == false && !this.isStroked && game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = true;
                this.callback?.Invoke(game);
            }
        }

        public void Render(GameEngine game, SKCanvas canvas)
        {
            canvas.Save();
            var metrics = paint.FontMetrics;
            float yAdjusted = y + ( -metrics.Ascent );
            canvas.DrawText(text, x, yAdjusted, (isHover && !isStroked) ? paintHover : paint);
            if (isStroked) {
                var linePaint = new SKPaint {
                    Color = paint.Color,
                    StrokeWidth = 2,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                };
                canvas.DrawLine(x, y + 12 - 2, x + w, y + h - 12 - 2, linePaint);
                canvas.DrawLine(x, y + 12 + 2, x + w, y + h - 12 + 2, linePaint);
            }
            canvas.Restore();
        }
    }
}
