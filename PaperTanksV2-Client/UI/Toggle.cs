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
        Action<GameEngine, bool> callback;
        public Toggle(string text, int x, int y, int w, int h, SKColor fontColor, SKColor fontHoverColor, SKTypeface face, SKFont font, float fontSize, bool state, Action<GameEngine, bool> callback) : base()
        {
            this.text = new Text(text, x + w + 8, y, fontColor, face, font, fontSize, SKTextAlign.Left);
            this.state = state;
            checkboxRect = new SKRect(x, y, x + w, y + h);
            callback = callback;
        }
        public void updateValue(bool value)
        {
            state = value;
        }
        public void Input(GameEngine game)
        {
            text?.Input(game);
            this.isHover =
                game.mouse.ScaledMousePosition.X >= checkboxRect.Left &&
                game.mouse.ScaledMousePosition.X < ( checkboxRect.Left + this.w ) &&
                game.mouse.ScaledMousePosition.Y >= checkboxRect.Top &&
                game.mouse.ScaledMousePosition.Y < ( checkboxRect.Top + this.h );
            if (this.isClicked == true && game.mouse.IsButtonJustReleased(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = false;
            } else if (this.isHover == true && this.isClicked == false && game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = true;
                this.state = !this.state;
                this.callback?.Invoke(game, this.state);
            }
        }
        public void Render(GameEngine game, SKCanvas canvas) {
            // Draw Rect, and Checked value (plus set color if is hovered)
            // -> canvas.DrawRect(this.checkboxRect);
            text?.Render(game, canvas);
        }
        public void Dispose()
        {
        }
    }
}
