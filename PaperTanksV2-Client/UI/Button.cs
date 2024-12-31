using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
namespace PaperTanksV2Client.UI
{
    class Button
    {
        string text;
        int x;
        int y;
        int w;
        int h;
        SKColor fontColor;
        SKColor fontHoverColor;
        SKFont font;
        Action callback;
        bool isHover = false;
        bool isClicked = false;
        SKPaint paint = null;
        SKPaint paintHover = null;
        public Button(string text, int x, int y, int w, int h, SKColor fontColor, SKColor fontHoverColor, SKFont font, Action callback)
        {
            this.text = text;
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.fontColor = fontColor;
            this.fontHoverColor = fontHoverColor;
            this.font = font;
            this.callback = callback;
            this.isHover = false;
            this.isClicked = false;
            this.paint = new SKPaint
            {
                Color = fontColor,
                //IsAntialias = true,
            };
            this.paintHover = new SKPaint
            {
                Color = fontHoverColor,
            };
        }

        public void Input(GameEngine game)
        {
            // show if hovered
            this.isHover =
                game.mouse.ScaledMousePosition.X >= this.x &&
                game.mouse.ScaledMousePosition.X < this.x + this.w && // `<` instead of `<=` to ensure proper boundary behavior
                game.mouse.ScaledMousePosition.Y >= this.y &&
                game.mouse.ScaledMousePosition.Y < this.y + this.h;
            // if not clicked and button is down, then invooke callback and marked as clicked
            if (this.isClicked == false && game.mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = true;
                this.callback();
            }
            // if on next frame and is clicked and button release, then mark as unclicked
            if (this.isClicked == true && game.mouse.IsButtonJustReleased(SFML.Window.Mouse.Button.Left)) {
                this.isClicked = false;
            }
        }

        public void Render(GameEngine game, SKCanvas canvas)
        {
            // Draw Button with text here using font, fontColor and fontHoverColor if isHover is true
            Helper.DrawCenteredText(canvas, this.text, new SKRect(x, y, x+w, y+h), font, isHover ? paintHover : paint);
        }
    }
}
