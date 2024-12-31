using PaperTanksV2Client;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.UI
{
    interface MenuItem : IDisposable
    {
        public void Input(GameEngine game) { }
        public void Render(GameEngine game, SKCanvas canvas) { }
    }
}
