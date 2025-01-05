using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.UI
{
    class Toggle : MenuItem
    {
        Text text = null;
        bool state = false;
        bool isHover = false;
        bool isClicked = false;
        SKRect checkboxRect;
        int w;
        int h;
        Action<Game, bool> callback;
        SKPaint paint = null;
        SKPaint hoverPaint = null;
        SKPoint l1;
        SKPoint l2;
        SKPoint l3;
        SKPoint l4;
        public Toggle(string text, int x, int y, int w, int h, SKColor fontColor, SKColor fontHoverColor, SKTypeface face, SKFont font, float fontSize, bool state, Action<Game, bool> callback) : base()
        {
            this.paint = new SKPaint() {
                Color = fontColor,
                StrokeWidth = 3f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            this.hoverPaint = new SKPaint() {
                Color = fontHoverColor,
                StrokeWidth = 3f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
        };
            this.text = new Text(text, x + w + 8, y, fontColor, face, font, fontSize, SKTextAlign.Left);
            this.text.hoverPaint.Color = fontHoverColor;
            this.state = state;
            this.checkboxRect = new SKRect(x, y + 4, x + w, y + h + 4);
            this.callback = callback;
            this.l1 = new SKPoint(x, y + 4);
            this.l2 = new SKPoint(x + w, y + h + 4);
            this.l3 = new SKPoint(x + w, y + 4);
            this.l4 = new SKPoint(x, y + h + 4);
        }
        public void updateValue(bool value)
        {
            state = value;
        }
        public void Input(Game game)
        {
            text?.Input(game);
            this.isHover = ( game.mouse.ScaledMousePosition.X >= checkboxRect.Left &&
                game.mouse.ScaledMousePosition.X < ( checkboxRect.Left + checkboxRect.Width ) &&
                game.mouse.ScaledMousePosition.Y >= checkboxRect.Top &&
                game.mouse.ScaledMousePosition.Y < ( checkboxRect.Top + checkboxRect.Height ) ) || this.text.isHover;
            if(this.text != null) this.text.isHover = this.isHover;
            if (this.isClicked == true && !game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = false;
            } else if (this.isHover == true && this.isClicked == false && game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = true;
                this.state = !this.state;
                this.callback?.Invoke(game, this.state);
            }
        }
        public void Render(Game game, SKCanvas canvas) {
            canvas.DrawRect(this.checkboxRect.Left, this.checkboxRect.Top, this.checkboxRect.Width, this.checkboxRect.Height, this.isHover ? this.hoverPaint : this.paint);
            if (this.state) {
                canvas.DrawLine(l1, l2, this.isHover ? this.hoverPaint : this.paint);
                canvas.DrawLine(l3, l4, this.isHover ? this.hoverPaint : this.paint);
            }
            text?.Render(game, canvas);
        }
        public void Dispose()
        {
        }
    }
}
