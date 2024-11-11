using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SkiaSharp;

namespace PaperTanksV2Client.PageStates
{
    class SplashPage : PageState
    {
        private string CompanyLogoName = "company_logo.png";
        private SkiaSharp.SKImage CompanyLogo;
        private double counter;
        private double loadMenuAfterSeconds = 3f;
        private bool changeInitiated = false;
        public void init(GameEngine game)
        {
            bool success_company_logo = game.resources.Load(ResourceManagerFormat.Image, this.CompanyLogoName);
            if (!success_company_logo) {
                throw new Exception("Unable to Load Company Logo");
            }
            this.CompanyLogo = (SkiaSharp.SKImage)game.resources.Get(ResourceManagerFormat.Image, this.CompanyLogoName);
            this.counter = 0;
        }

        public void input(GameEngine game)
        {
        }
        public void update(GameEngine game, double deltaTime)
        {
            if(!this.changeInitiated && this.counter >= this.loadMenuAfterSeconds)
            {
                this.changeInitiated = true;
                game.showCursor = true;
                game.pages.RemoveAt(game.pages.Count() - 1);
                PageState mmp = new MainMenuPage();
                mmp.init(game);
                game.pages.Add(mmp);
            }
            this.counter += deltaTime;
        }
        public void prerender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void render(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
            if (this.CompanyLogo != null)
            {
                canvas.DrawImage(this.CompanyLogo, 0, 0);
            }
        }

        public void postrender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }
    }
}
