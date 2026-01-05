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
        readonly Action<Game> callback = null;
        bool isHover = false;
        bool isClicked = false;
        bool isStroked = false;
#pragma warning disable IDE0044 // Add readonly modifier
        SKPaint paint = null;
#pragma warning restore IDE0044 // Add readonly modifier
        SKPaint paintHover = null;
        private bool wasPressedLastFrame = false;
        private DateTime lastClickTime = DateTime.MinValue;
        private readonly TimeSpan clickCooldown = TimeSpan.FromSeconds(5);

        public Button(string text, int x, int y, SKColor fontColor, SKColor fontHoverColor, SKTypeface face, SKFont font, float fontSize, SKTextAlign align, Action<Game> callback, bool isStroked = false) : base()
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
            if (this.paint != null) this.paint.Dispose();
            if (this.paintHover != null) this.paintHover.Dispose();
        }

        public void Input(Game game)
        {
            // show if hovered
            this.isHover =
                game.mouse.ScaledMousePosition.X >= this.x &&
                game.mouse.ScaledMousePosition.X < (this.x + this.w) &&
                game.mouse.ScaledMousePosition.Y >= this.y &&
                game.mouse.ScaledMousePosition.Y < (this.y + this.h);

            bool isCurrentlyPressed = game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left);
            
            // Reset clicked state when button is released
            if (!isCurrentlyPressed) {
                this.isClicked = false;
            }
    
            // Check if cooldown has passed
            bool canClick = (DateTime.Now - this.lastClickTime) >= this.clickCooldown;

            // Only trigger on initial press while hovering, not already clicked, and cooldown expired
            if (this.isHover && !this.isStroked && isCurrentlyPressed && !this.wasPressedLastFrame && canClick) {
                if (!this.isClicked) {
                    this.isClicked = true;
                    this.lastClickTime = DateTime.Now;
                    this.callback?.Invoke(game);
                }
            }
            
            // Store state for next frame
            this.wasPressedLastFrame = isCurrentlyPressed;
        }

        public void Render(Game game, SKCanvas canvas)
        {
            canvas.Save();
            var metrics = paint.FontMetrics;
            float yAdjusted = y + ( -metrics.Ascent );
            canvas.DrawText(text, x, yAdjusted, (isHover && !isStroked) ? paintHover : paint);
            if (isStroked) {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var linePaint = new SKPaint {
                    Color = paint.Color,
                    StrokeWidth = 2,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                };
#pragma warning restore CA2000 // Dispose objects before losing scope
                canvas.DrawLine(x, y + 12 - 2, x + w, y + h - 12 - 2, linePaint);
                canvas.DrawLine(x, y + 12 + 2, x + w, y + h - 12 + 2, linePaint);
            }
            canvas.Restore();
        }
    }
}