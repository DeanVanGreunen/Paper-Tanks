using SkiaSharp;
using System;

namespace PaperTanksV2Client.UI
{
    interface MenuItem : IDisposable
    {
        public void Input(GameEngine game) { }
        public void Render(GameEngine game, SKCanvas canvas) { }
    }
}
