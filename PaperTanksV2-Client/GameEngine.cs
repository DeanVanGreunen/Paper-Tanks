using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using PaperTanksV2_Client.PageStates;
using System.Linq;

namespace PaperTanksV2_Client
{
    class GameEngine : IDisposable
    {
        protected string version = "v0.0.1-beta";
        public uint targetWidth = 3840;   // 4K width
        public uint targetHeight = 2160;  // 4K height
        protected int displayWidth;          // Screen width
        protected int displayHeight;         // Screen height
        protected float ratio = 16f / 9f;
        protected string title = "PaperTanks™ - SoftArt Studios";
        protected RenderWindow window;
        public bool isRunning = false;
        protected byte[] pixels;
        public KeyboardState keyboard;
        public MouseState mouse;
        public List<PageState> pages;
        public ResourceManager resources;
        public int run()
        {
            try
            {
                this.init();
                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                SKImageInfo info = new SKImageInfo((int)this.targetWidth, (int)this.targetHeight);
                using (SKBitmap bitmap = new SKBitmap(info))
                {
                    using (Texture texture = new Texture((uint)bitmap.Width, (uint)bitmap.Height))
                    {
                        using (Sprite sprite = new Sprite(texture))
                        {
                            sprite.Scale = new SFML.System.Vector2f(
                                (float)displayWidth / targetWidth,
                                (float)displayHeight / targetHeight
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
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
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
            this.pixels = new byte[this.targetWidth * this.targetHeight * 4];
            // Initialize SFML window with the display resolution
            this.window = new RenderWindow(new VideoMode((uint)this.displayWidth, (uint)this.displayHeight), this.title + " " + this.version);
            this.window.Closed += (sender, e) =>
            {
                window.Close();
                this.isRunning = false;
            };
            this.keyboard = new KeyboardState(this.window);
            this.mouse = new MouseState(this.window, (int)this.targetWidth, (int)this.targetHeight);
            this.resources = new ResourceManager();
            this.isRunning = true;
            this.pages = new List<PageState>();
            this.pages.Add(new SplashPage());
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
        }
        protected void render(SKCanvas canvas)
        {
            canvas.Clear(SKColors.Black);

            if (this.pages.Any())
            {
                this.pages.Last().prerender(this, canvas);
                this.pages.Last().render(this, canvas);
                this.pages.Last().postrender(this, canvas);
            }
        }
        private Vector2i ScaleMousePosition(Vector2i mousePos)
        {
            // Scale the mouse position from display resolution to 4K resolution
            int scaledX = (int)(mousePos.X * (float)this.targetWidth / this.displayWidth);
            int scaledY = (int)(mousePos.Y * (float)this.targetHeight / this.displayHeight);
            return new Vector2i(scaledX, scaledY);
        }

        public void Dispose()
        {
            if (this.pages.Any())
            {
                this.pages.Clear();
            }
        }
    }
}
