using SFML.Graphics;
using SkiaSharp;
using System;
using System.Linq;

namespace PaperTanksV2Client.PageStates
{
    class SplashPage : PageState
    {
        private string CompanyLogoName = "company_logo.png";
        private SkiaSharp.SKImage CompanyLogo;
        private double counter;
        private double loadMenuAfterSeconds = 1.5f; // change to 3f
        private bool changeInitiated = false;
        private int progressX = 600;
        private int progressY = 800;
        private int progressW = 732;
        private int progressH = 8;
        private SKPaint progressBoundingBoxPaintFill;
        private SKPaint progressBoundingBoxPaintInner;
        private SKPaint progressBoundingBoxPaintOutline;
        private Int32 progressBoundingBoxPaintOutlineStrokeWidth = 1;
        private SKRect progressBoundingBoxRect;
        private SKRect progressBoundingBoxRectLoaded;
        public void init(Game game)
        {
            bool success_company_logo = game.resources.Load(ResourceManagerFormat.Image, this.CompanyLogoName);
            if (!success_company_logo) {
                throw new Exception("Unable to Load Company Logo");
            }
            this.CompanyLogo = (SkiaSharp.SKImage) game.resources.Get(ResourceManagerFormat.Image, this.CompanyLogoName);
            this.counter = 0;
            this.progressBoundingBoxRect = new SKRect(progressX, progressY, progressX + progressW, progressY + progressH);
            this.progressBoundingBoxRectLoaded = new SKRect(progressX, progressY, progressX, progressY + progressH);
            this.progressBoundingBoxPaintFill = new SKPaint {
                Style = SKPaintStyle.Fill, // Set the style to fill
                Color = SKColors.Black     // Set the color to black
            };
            this.progressBoundingBoxPaintInner = new SKPaint {
                Style = SKPaintStyle.Fill, // Set the style to fill
                Color = SKColors.White     // Set the color to black
            };
            this.progressBoundingBoxPaintOutline = new SKPaint {
                Style = SKPaintStyle.Stroke, // Set the style to stroke
                Color = SKColors.White,      // Set the color to white
                StrokeWidth = progressBoundingBoxPaintOutlineStrokeWidth              // Set the desired stroke width
            };
        }

        public void input(Game game)
        {
        }
        public void update(Game game, float deltaTime)
        {
            if (!this.changeInitiated && this.counter >= this.loadMenuAfterSeconds) {
                this.changeInitiated = true;
                game.showRealCursor = false;
                game.states.RemoveAt(game.states.Count() - 1);
                PageState mmp = new MainMenuPage();
                mmp.init(game);
                game.states.Add(mmp);
            }
            this.counter += deltaTime;
            this.progressBoundingBoxRectLoaded.Right = progressX + ( progressW * (float) ( ( this.counter > this.loadMenuAfterSeconds ? this.loadMenuAfterSeconds : this.counter ) / this.loadMenuAfterSeconds ) );
        }
        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            if (this.CompanyLogo != null) {
                canvas.DrawImage(this.CompanyLogo, new SKRect(0, 0, Game.targetWidth, Game.targetHeight));
            }
            canvas.DrawRect(this.progressBoundingBoxRect, progressBoundingBoxPaintFill);
            this.progressBoundingBoxRect.Top -= progressBoundingBoxPaintOutlineStrokeWidth;
            canvas.DrawRect(this.progressBoundingBoxRect, progressBoundingBoxPaintOutline);
            this.progressBoundingBoxRect.Top += progressBoundingBoxPaintOutlineStrokeWidth;
            canvas.DrawRect(this.progressBoundingBoxRectLoaded, progressBoundingBoxPaintInner);
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }
    }
}
