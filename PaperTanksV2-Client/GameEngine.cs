using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using PaperTanksV2Client.PageStates;
using System.Linq;

namespace PaperTanksV2Client
{
    class GameEngine : IDisposable
    {
        protected const string version = "v0.0.1-beta";
        public const uint targetWidth = 3840;         // 4K width (Internal Render Output)
        public const uint targetHeight = 2160;        // 4K height (Internal Render Output)
        protected int displayWidth;                   // Screen width (User Screen Renderable Output)
        protected int displayHeight;                  // Screen height (User Screen Renderable Output)
        protected const float aspectRatio = 16f / 9f; // Game Designed For This Aspect Ratio
        protected const int bpp = 32;                 // Bits Per Pixel Window Output 
        protected const string title = "PaperTanks™ - SoftArt Studios";
        protected RenderWindow window;
        public bool isRunning = false;
        protected byte[] pixels;
        public KeyboardState keyboard;
        public MouseState mouse;
        public List<PageState> pages;
        public ResourceManager resources;
        public bool showCursor = false;
        protected SKImage cursorImage = null;
        protected string cursorImageFileName = "pencil.png";
        protected SKRect cursorPositionSrc = SKRect.Empty;
        protected SKRect cursorPositionDest = SKRect.Empty;
        public int run()
        {
            try
            {
                this.init();
                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                SKImageInfo info = new SKImageInfo((int)GameEngine.targetWidth, (int)GameEngine.targetHeight);
                using (SKBitmap bitmap = new SKBitmap(info))
                {
                    using (Texture texture = new Texture((uint)bitmap.Width, (uint)bitmap.Height))
                    {
                        using (Sprite sprite = new Sprite(texture))
                        {
                            sprite.Scale = new SFML.System.Vector2f(
                                (float)this.displayWidth / GameEngine.targetWidth,
                                (float)this.displayHeight / GameEngine.targetHeight
                            );
                            while (this.window.IsOpen && this.isRunning)
                            {
                                double deltaTime = stopwatch.Elapsed.TotalSeconds;
                                stopwatch.Restart();
                                window.DispatchEvents();
                                this.update(deltaTime);
                                using (var surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes))
                                {
                                    this.render(surface.Canvas);
                                }
                                Marshal.Copy(bitmap.GetPixels(), pixels, 0, pixels.Length);
                                texture.Update(pixels);
                                window.Clear();
                                window.Draw(sprite);
                                window.Display();
                            }
                        }
                    }
                }
                this.cleanup();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
        protected void init()
        {
            VideoMode desktopMode = VideoMode.DesktopMode;
            this.displayWidth = (int)desktopMode.Width;
            this.displayHeight = (int)desktopMode.Height;
            // Update pixels array for 4K resolution
            this.pixels = new byte[GameEngine.targetWidth * GameEngine.targetHeight * 4];
            // Initialize SFML window with the display resolution
            this.window = new RenderWindow(new VideoMode((uint)this.displayWidth, (uint)this.displayHeight, (uint)GameEngine.bpp), GameEngine.title + " " + GameEngine.version, Styles.Fullscreen);
            this.window.Closed += (sender, e) =>
            {
                window.Close();
                this.isRunning = false;
            };
            this.keyboard = new KeyboardState(this.window);
            this.mouse = new MouseState(this.window, (int)GameEngine.targetWidth, (int)GameEngine.targetHeight);
            this.resources = new ResourceManager();
            this.isRunning = true;
            this.pages = new List<PageState>();
            this.pages.Add(new SplashPage());
            this.pages.Last().init(this);

            bool success_cursor_image = this.resources.Load(ResourceManagerFormat.Image, this.cursorImageFileName);
            if (!success_cursor_image)
            {
                throw new Exception("Unable to Load Cursor Image");
            }
            this.cursorImage = (SkiaSharp.SKImage)this.resources.Get(ResourceManagerFormat.Image, this.cursorImageFileName);
            this.cursorPositionSrc = new SKRect(0, 0, this.cursorImage.Width, this.cursorImage.Height);
        }
        protected void cleanup()
        {
            this.isRunning = false;
        }
        protected void input()
        {
            this.keyboard.Update();
            this.mouse.Update();
            if (this.pages.Any())
            {
                this.pages.Last().input(this);
            }
        }
        protected void update(double deltaTime)
        {
            if (this.pages.Any())
            {
                this.pages.Last().update(this, deltaTime);
            }
            this.cursorPositionDest = new SKRect(this.mouse.ScaledMousePosition.X, this.mouse.ScaledMousePosition.Y, this.mouse.ScaledMousePosition.X + this.cursorImage.Width, this.mouse.ScaledMousePosition.Y + this.cursorImage.Height);
        }
        protected void render(SKCanvas canvas)
        {
            canvas.Clear(SKColors.Black);

            if (this.pages.Any())
            {
                PageState last = this.pages.Last();
                last.prerender(this, canvas);
                last.render(this, canvas);
                last.postrender(this, canvas);
            }

            if (this.showCursor)
            {
                canvas.DrawImage(this.cursorImage, this.cursorPositionSrc, this.cursorPositionDest);
            }
        }
        private Vector2i ScaleMousePosition(Vector2i mousePos)
        {
            // Scale the mouse position from display resolution to 4K resolution
            int scaledX = (int)(mousePos.X * (float)GameEngine.targetWidth / this.displayWidth);
            int scaledY = (int)(mousePos.Y * (float)GameEngine.targetHeight / this.displayHeight);
            return new Vector2i(scaledX, scaledY);
        }

        public void Dispose()
        {
            this.cursorImage.Dispose();
            this.window.Close();
            if (this.pages.Any())
            {
                this.pages.Clear();
            }
        }
    }
}
