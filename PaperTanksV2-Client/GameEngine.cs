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
        /*
         * W: 3840 -> 1920 
         * H: 2160 -> 1080
         */
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
        public List<PageState> states;
        public ResourceManager resources;
        public FontManager fonts;
        public bool showCursor = false;
        public bool showRealCursor = true;
        protected SKImage cursorImage = null;
#pragma warning disable IDE0069 // Disposable fields should be disposed
        protected SKPaint cursorPaint = null;
#pragma warning restore IDE0069 // Disposable fields should be disposed
        protected string cursorImageFileName = "pencil.png";
        protected SKRect cursorPositionSrc = SKRect.Empty;
        protected SKRect cursorPositionDest = SKRect.Empty;
        protected SKRect drawWindowOutlineRect = SKRect.Empty;
        protected SKPaint drawWindowOutlinePaint = new SKPaint();
        protected bool renderDemoVersion = true;
        public RenderStates renderStates = RenderStates.Default;
        public int run()
        {
            try
            {
                this.init();
                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                SKImageInfo info = new SKImageInfo((int)GameEngine.targetWidth, (int)GameEngine.targetHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                using (SKBitmap bitmap = new SKBitmap(info))
                {
                    using (Texture texture = new Texture((uint)bitmap.Width, (uint)bitmap.Height))
                    {
                        using (Sprite sprite = new Sprite(texture))
                        {
                            float scale = Math.Min(
                                (float)this.displayWidth / GameEngine.targetWidth,
                                (float)this.displayHeight / GameEngine.targetHeight
                            );
                            sprite.Scale = new SFML.System.Vector2f(scale, scale);
                            while (this.window.IsOpen && this.isRunning)
                            {
                                double deltaTime = stopwatch.Elapsed.TotalSeconds;
                                stopwatch.Restart();
                                window.DispatchEvents();
                                this.update(deltaTime);
                                using (SKSurface surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes))
                                {
                                    this.render(surface.Canvas, renderStates);
                                }
                                Marshal.Copy(bitmap.GetPixels(), pixels, 0, pixels.Length);
                                texture.Update(pixels);
                                window.Clear();
                                window.Draw(sprite, renderStates);
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
            this.pixels = new byte[GameEngine.targetWidth * GameEngine.targetHeight * 4];
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
            this.states = new List<PageState>();
            this.states.Add(new SplashPage());
            this.states.Last().init(this);
            bool success_cursor_image = this.resources.Load(ResourceManagerFormat.Image, this.cursorImageFileName);
            if (!success_cursor_image)
            {
                throw new Exception("Unable to Load Cursor Image");
            }
            this.cursorImage = (SkiaSharp.SKImage)this.resources.Get(ResourceManagerFormat.Image, this.cursorImageFileName);
            SKImageInfo newImageInfo = new SKImageInfo(this.cursorImage.Width, this.cursorImage.Height);
            using (SKBitmap scaledBitmap = new SKBitmap(newImageInfo))
            {
                using (SKCanvas canvas = new SKCanvas(scaledBitmap))
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawImage(cursorImage, new SKRect(0, 0, this.cursorImage.Width / 2, this.cursorImage.Height / 2));
                }
                this.cursorImage = SKImage.FromBitmap(scaledBitmap);
            }
            this.cursorPositionSrc = new SKRect(0, 0, this.cursorImage.Width, this.cursorImage.Height);
            this.fonts = new FontManager();
            bool font_manager_init = this.fonts.init(this.resources);
            if (!font_manager_init)
            {
                throw new Exception("Unable to Load Font Manager");
            }
            this.cursorPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White,
                BlendMode = SKBlendMode.SrcOver,
                IsDither = true,
                ColorFilter = SKColorFilter.CreateBlendMode(SKColors.White, SKBlendMode.Modulate)
            };
            this.drawWindowOutlinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke, // Set the style to stroke
                Color = SKColors.White,      // Set the color to white
                StrokeWidth = 2              // Set the desired stroke width
            };
        }
        protected void cleanup()
        {
            this.isRunning = false;
        }
        protected void input()
        {
            this.keyboard.Update();
            this.mouse.Update();
            if (this.states.Any())
            {
                this.states.Last().input(this);
            }
        }
        protected void update(double deltaTime)
        {
            if(this.showCursor == this.showRealCursor)
            {
                this.showRealCursor = !this.showCursor;
                this.window.SetMouseCursorVisible(true); // this.showRealCursor);
            }
            if (this.states.Any())
            {
                this.states.Last().update(this, deltaTime);
            }
            this.cursorPositionDest = new SKRect(
                this.mouse.ScaledMousePosition.X,
                this.mouse.ScaledMousePosition.Y - (this.cursorImage.Height / 2),
                this.mouse.ScaledMousePosition.X + this.cursorImage.Width,
                this.mouse.ScaledMousePosition.Y + (this.cursorImage.Height / 2)
            );
        }
        protected void render(SKCanvas canvas, RenderStates renderStates)
        {
            canvas.Clear(SKColors.Black);
            if (this.states.Any())
            {
                PageState last = this.states.Last();
                last.prerender(this, canvas, renderStates);
                last.render(this, canvas, renderStates);
                last.postrender(this, canvas, renderStates);
            }
            if (this.showCursor)
            {
                canvas.DrawImage(this.cursorImage, this.cursorPositionSrc, this.cursorPositionDest, this.cursorPaint);
            }
            if (this.renderDemoVersion)
            {
                // TODO: DRAW GAME VERSION using default text rendering of this grpahics library
            }
        }
        private Vector2i ScaleMousePosition(Vector2i mousePos)
        {
            int scaledX = (int)(mousePos.X * (float)GameEngine.targetWidth / this.displayWidth);
            int scaledY = (int)(mousePos.Y * (float)GameEngine.targetHeight / this.displayHeight);
            return new Vector2i(scaledX, scaledY);
        }

        public void Dispose()
        {
            if (this.cursorImage != null)
            {
                this.cursorImage.Dispose();
                this.cursorImage = null;
            }
            if (this.window != null)
            {
                this.window.Close();
            }
            if (this.states != null && this.states.Any())
            {
                this.states.Clear();
                this.states = null;
            }
        }
    }
}
