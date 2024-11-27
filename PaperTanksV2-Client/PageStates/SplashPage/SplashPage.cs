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
        private int progressX = 457;
        private int progressY = 1974;
        private int progressW = 2932;
        private int progressH = 87;
        private SKPaint progressBoundingBoxPaintFill;
        private SKPaint progressBoundingBoxPaintInner;
        private SKPaint progressBoundingBoxPaintOutline;
        private SKRect progressBoundingBoxRect;
        private SKRect progressBoundingBoxRectLoaded;
        public void init(GameEngine game)
        {
            bool success_company_logo = game.resources.Load(ResourceManagerFormat.Image, this.CompanyLogoName);
            if (!success_company_logo) {
                throw new Exception("Unable to Load Company Logo");
            }
            this.CompanyLogo = (SkiaSharp.SKImage)game.resources.Get(ResourceManagerFormat.Image, this.CompanyLogoName);
            this.counter = 0;
            this.progressBoundingBoxRect = new SKRect(progressX, progressY, progressX + progressW, progressY + progressH);
            this.progressBoundingBoxRectLoaded = new SKRect(progressX, progressY, progressX, progressY + progressH);
            this.progressBoundingBoxPaintFill = new SKPaint
            {
                Style = SKPaintStyle.Fill, // Set the style to fill
                Color = SKColors.Black     // Set the color to black
            };
            this.progressBoundingBoxPaintInner = new SKPaint
            {
                Style = SKPaintStyle.Fill, // Set the style to fill
                Color = SKColors.White     // Set the color to black
            };
            this.progressBoundingBoxPaintOutline = new SKPaint
            {
                Style = SKPaintStyle.Stroke, // Set the style to stroke
                Color = SKColors.White,      // Set the color to white
                StrokeWidth = 2              // Set the desired stroke width
            };
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
            // this.progressBoundingBoxRectLoaded.Right;
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
            // Draw Progress Bar Outline
            canvas.DrawRect(this.progressBoundingBoxRect, progressBoundingBoxPaintFill);
            canvas.DrawRect(this.progressBoundingBoxRect, progressBoundingBoxPaintOutline);
            // Draw Progress Bar Outline
            canvas.DrawRect(this.progressBoundingBoxRect, progressBoundingBoxPaintInner);
        }

        public void postrender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
        }
    }
}
