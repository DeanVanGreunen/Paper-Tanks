using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SkiaSharp;

namespace PaperTanksV2Client.PageStates
{
    class MainMenuPage : PageState, IDisposable
    {
        private SKPaint p = new SKPaint();
        public void init(GameEngine game)
        {
            p.Color = SKColors.Red;
        }

        public void input(GameEngine game)
        {
        }
        public void update(GameEngine game, double deltaTime)
        {
        }
        public void prerender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void render(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void postrender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void Dispose()
        {
            p.Dispose();
        }
    }
}
