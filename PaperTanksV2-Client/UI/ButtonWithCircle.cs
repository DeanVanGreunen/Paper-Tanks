using SkiaSharp;
using System;
namespace PaperTanksV2Client.UI
{
    public class ButtonWithCircle : MenuItem
    {
        private string text;
        private int x;
        private int y;
        private int w;
        private int h;
        private int cx;
        private int cy;
        private int r;
        private bool isActive;
        SKColor fontColor;
        SKColor fontHoverColor;
        SKTypeface face;
        SKFont font;
        readonly Action<Game> callback = null;
        bool isHover = false;
        bool isClicked = false;
#pragma warning disable IDE0044 // Add readonly modifier
        SKPaint paint = null;
#pragma warning restore IDE0044 // Add readonly modifier
        SKPaint paintHover = null;
        private SKColor GreyColor;
        public ButtonWithCircle(string text, int x, int y, SKColor fontColor, SKColor fontHoverColor, SKTypeface face, SKFont font, float fontSize, SKTextAlign align, Action<Game> callback, bool isActive = false) : base()
        {
            SKColor.TryParse("707070", out this.GreyColor);
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
            
            // Circle centered around text, with max diameter of 64px
            this.r = Math.Min(64, Math.Max(this.w, this.h)) / 2;
            this.cx = this.x + this.w / 2;
            this.cy = this.y + this.h / 2;
            
            this.isActive = isActive;
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
                (game.mouse.ScaledMousePosition.X >= this.x &&
                game.mouse.ScaledMousePosition.X < ( this.x + this.w ) &&
                game.mouse.ScaledMousePosition.Y >= this.y &&
                game.mouse.ScaledMousePosition.Y < ( this.y + this.h )) || (
                    (game.mouse.ScaledMousePosition.X - this.cx) * (game.mouse.ScaledMousePosition.X - this.cx) + (game.mouse.ScaledMousePosition.Y - this.cy) * (game.mouse.ScaledMousePosition.Y - this.cy) <= this.r * this.r
                );

            // if on next frame and is clicked and button release, then mark as unclicked
            if (this.isClicked == true && !game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = false;
            } else if (this.isHover == true && this.isClicked == false &&
                       game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = true;
                this.isActive = true;
                this.callback?.Invoke(game);
            }
        }

        public void Render(Game game, SKCanvas canvas)
        {
            canvas.Save();
            var metrics = paint.FontMetrics;
            float yAdjusted = y + ( -metrics.Ascent );
            
            using(SKPaint greyCirclePaint = new SKPaint {
                Color = GreyColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            }) {
                using(SKPaint whiteCirclePaint = new SKPaint {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                }) {
                    using (var whiteTextPaint = new SKPaint {
                        Color = SKColors.White,
                        TextSize = paint.TextSize,
                        TextAlign = paint.TextAlign,
                        Typeface = face,
                        IsAntialias = true
                    }) {
                        using (var greyTextPaint = new SKPaint {
                            Color = GreyColor,
                            TextSize = paint.TextSize,
                            TextAlign = paint.TextAlign,
                            Typeface = face,
                            IsAntialias = true
                        }) {
                            // Active or Hover: Grey background, white text
                            if (this.isActive || this.isHover) {
                                canvas.DrawCircle(this.cx, this.cy, this.r, greyCirclePaint);
                                canvas.DrawText(text, x, yAdjusted, whiteTextPaint);
                            } 
                            // Normal: White background, grey text
                            else {
                                canvas.DrawCircle(this.cx, this.cy, this.r, whiteCirclePaint);
                                canvas.DrawText(text, x, yAdjusted, greyTextPaint);
                            }
                        }
                    }
                }
            }

            canvas.Restore();
        }
    }
}
