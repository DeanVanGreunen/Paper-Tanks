using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace PaperTanksV2_Client.PageStates
{
    class SplashPage : PageState
    {
        SkiaSharp.SKImage CompanyLogo;
        public void init(GameEngine game)
        {
            bool success_company_logo = game.resources.Load(ResourceManagerFormat.Image, "company_logo.png");
            if (!success_company_logo) {
                throw new Exception("Unable to Load Company Logo");
            }
            this.CompanyLogo = (SkiaSharp.SKImage)game.resources.Get(ResourceManagerFormat.Image, "company_logo.png");
        }

        public void input(GameEngine game)
        {
        }
        public void update(GameEngine game, double deltaTime)
        {
        }
        public void prerender(GameEngine game, SKCanvas canvas)
        {
        }

        public void render(GameEngine game, SKCanvas canvas)
        {
            if (this.CompanyLogo != null)
            {
                canvas.DrawImage(this.CompanyLogo, 0, 0);
            }
        }

        public void postrender(GameEngine game, SKCanvas canvas)
        {
        }
    }
}
