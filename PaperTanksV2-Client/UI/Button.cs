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
        }

        public void Input(MouseState m, KeyboardState k)
        {
            this.isHover = m.RawMousePosition.X >= this.x && m.RawMousePosition.Y >= this.y && m.RawMousePosition.X <= this.x + this.w && m.RawMousePosition.Y <= this.y + this.h;
            if (isClicked == false) {
                this.isClicked = true;
                this.callback();
            }
        }

        public void Render()
        {
            // Draw Button with text here using font, fontColor and fontHoverColor if isHover is true
        }
    }
}
