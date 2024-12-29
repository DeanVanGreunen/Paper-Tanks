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
        private SKImage backgroundTable = null;
        private bool isOpenned;
        public void init(GameEngine game)
        {
            this.p.Color = SKColors.Red;
            bool loaded = game.resources.Load(ResourceManagerFormat.Image, "background-table.jpeg");
            if (!loaded) throw new Exception("Error Loading Background Table");
            this.backgroundTable = (SKImage)game.resources.Get(ResourceManagerFormat.Image, "background-table.jpeg");
        }

        public void input(GameEngine game)
        {

        }
        public void update(GameEngine game, double deltaTime)
        {
        }
        public void prerender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
            if(backgroundTable != null) canvas.DrawImage(this.backgroundTable, 0, 0);
        }

        public void render(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void postrender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void Dispose()
        {
            if (p != null) p.Dispose();
            if (backgroundTable != null) backgroundTable.Dispose();
        }
    }
}
