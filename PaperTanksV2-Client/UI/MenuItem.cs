using SkiaSharp;
using System;

namespace PaperTanksV2Client.UI
{
    interface MenuItem : IDisposable
    {
        public void updateValue<T>(T value) { }
        public void manageValue<T>(Action<T> callback) { }
        public void updateText(string text) { }
        public void Input(GameEngine game) { }
        public void Render(GameEngine game, SKCanvas canvas) { }
    }
}
