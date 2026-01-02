using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.PageStates;
using PaperTanksV2Client.UI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace PaperTanksV2Client
{
    public class Game : IDisposable
    {
        /*
         * W: 3840 -> 1920 
         * H: 2160 -> 1080
         */
        protected const string version = "development mode";
        public const uint targetWidth = 1920;         // 4K width (Internal Render Output)
        public const uint targetHeight = 1080;        // 4K height (Internal Render Output)
        protected int displayWidth;                   // Screen width (User Screen Renderable Output)
        protected int displayHeight;                  // Screen height (User Screen Renderable Output)
        protected const float aspectRatio = 16f / 9f; // Game Designed For This Aspect Ratio
        protected const int bpp = 32;                 // Bits Per Pixel Window Output 
        protected const string title = "PaperTanks™ - SoftArt Studios";
        public RenderWindow window;
        public bool isRunning = false;
        public SKImageInfo info;
        public SKBitmap bitmap;
        public Texture texture;
        public Sprite sprite;
        public SKSurface surface;
        protected byte[] pixels;
        //private IntPtr pixelsHandle; // Pin the pixels array
        private RenderStates cachedRenderStates;
        public const int TARGET_FPS = 60;
        public const float FRAME_TIME = 1.0f / TARGET_FPS;
        private double currentFps;
        public KeyboardState keyboard;
        public MouseState mouse;
        public List<PageState> states;
        public ResourceManager resources;
        public FontManager fonts;
        public PlayerData player;
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
        protected bool renderFPS = true;
        protected bool renderDemoVersion = true;
        public RenderStates renderStates = RenderStates.Default;
        public MenuItem demoItem = null;
        public MenuItem fpsItem = null;
        public ConfigManager configs = new ConfigManager();
        public readonly static string UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PaperTanks");
        public readonly static string SettingsPath = Path.Combine(UserDataPath, "settings.json");
        public readonly static string SavePath = Path.Combine(UserDataPath, "save.json");
        public int run()
        {
            try {
                this.init();
                var frameTimer = new Stopwatch();
                frameTimer.Start();
                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                while (this.window.IsOpen && this.isRunning) {
                    frameTimer.Restart();
                    float deltaTime = (float) stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart();
                    this.currentFps = deltaTime;
                    window.DispatchEvents();
                    this.input();
                    if (!isRunning) break; // if game has been stopped, then break out this while loop
                    this.update(deltaTime);
                    // Only render if we need to update the frame
                    if (frameTimer.ElapsedMilliseconds < ( FRAME_TIME * 1000 )) {
                        unsafe {
                            this.render(surface.Canvas, cachedRenderStates);

                            // Copy pixels directly from bitmap to our pixels array
                            fixed (void* pixelsPtr = pixels) {
                                System.Buffer.MemoryCopy(
                                    bitmap.GetPixels().ToPointer(),
                                    pixelsPtr,
                                    pixels.Length,
                                    pixels.Length
                                );
                            }
                            // Update texture with the byte array
                            texture.Update(pixels);
                        }
                        window.Clear(Color.Black);
                        window.Draw(sprite, cachedRenderStates);
                        window.Display();
                    }
                    // Frame pacing - sleep if we're ahead of schedule
                    int sleepTime = (int) ( ( FRAME_TIME * 1000 ) - frameTimer.ElapsedMilliseconds );
                    if (sleepTime > 0) {
                        Thread.Sleep(sleepTime);
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
            this.displayWidth = (int) desktopMode.Width;
            this.displayHeight = (int) desktopMode.Height;
            this.info = new SKImageInfo((int) Game.targetWidth, (int) Game.targetHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            this.bitmap = new SKBitmap(info);
            this.texture = new Texture((uint) bitmap.Width, (uint) bitmap.Height);
            this.sprite = new Sprite(texture);
            this.surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes);
            this.pixels = new byte[Game.targetWidth * Game.targetHeight * 4];
            //pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject();
            cachedRenderStates = new RenderStates(BlendMode.Alpha);
            float scale = Math.Min(
                (float) this.displayWidth / Game.targetWidth,
                (float) this.displayHeight / Game.targetHeight
            );
            sprite.Scale = new SFML.System.Vector2f(scale, scale);
            this.window = new RenderWindow(new VideoMode((uint) this.displayWidth, (uint) this.displayHeight, (uint) Game.bpp), Game.title + " " + Game.version, Styles.Fullscreen);
            this.window.Clear(Color.Black);
            this.window.Display();
            window.SetVerticalSyncEnabled(true);
            window.SetFramerateLimit(TARGET_FPS);
            this.window.Closed += (sender, e) => {
                window.Close();
                this.isRunning = false;
            };
            this.keyboard = new KeyboardState(this.window);
            this.mouse = new MouseState(this.window, (int) Game.targetWidth, (int) Game.targetHeight);
            this.resources = new ResourceManager();
            this.isRunning = true;
            this.states = new List<PageState>();
            var splash = new SplashPage();
            splash.init(this);
            this.states.Add(splash);
            bool success_cursor_image = this.resources.Load(ResourceManagerFormat.Image, this.cursorImageFileName);
            if (!success_cursor_image) {
                throw new Exception("Unable to Load Cursor Image");
            }
            this.cursorImage = (SkiaSharp.SKImage) this.resources.Get(ResourceManagerFormat.Image, this.cursorImageFileName);
            SKImageInfo newImageInfo = new SKImageInfo(64, 64);
            SKPaint paint2 = new SKPaint {
                FilterQuality = SKFilterQuality.High,
                IsAntialias = false,
            };
            using (SKBitmap scaledBitmap = new SKBitmap(newImageInfo)) {
                using (SKCanvas canvas = new SKCanvas(scaledBitmap)) {
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawImage(cursorImage, new SKRect(0, 0, this.cursorImage.Width / 4, this.cursorImage.Height / 4), paint2);
                }
                this.cursorImage = SKImage.FromBitmap(scaledBitmap);
            }
            paint2.Dispose();
            this.cursorPositionSrc = new SKRect(0, 0, 64, 64);
            this.fonts = new FontManager();
            bool font_manager_init = this.fonts.init(this.resources);
            if (!font_manager_init) {
                throw new Exception("Unable to Load Font Manager");
            }
            this.cursorPaint = new SKPaint {
                IsAntialias = true,
                Color = SKColors.White,
                BlendMode = SKBlendMode.SrcOver,
                IsDither = true,
                ColorFilter = SKColorFilter.CreateBlendMode(SKColors.White, SKBlendMode.Modulate),
                FilterQuality = SKFilterQuality.High
            };
            this.drawWindowOutlinePaint = new SKPaint {
                Style = SKPaintStyle.Stroke, // Set the style to stroke
                Color = SKColors.White,      // Set the color to white
                StrokeWidth = 2,              // Set the desired stroke width
                FilterQuality = SKFilterQuality.High
            };
            demoItem = new PaperTanksV2Client.UI.Text(Game.version, 4, (int) Game.targetHeight - 28, SKColor.Parse("#58aff3"), SKTypeface.Default, SKTypeface.Default.ToFont(), 18f, SKTextAlign.Left);
            fpsItem = new PaperTanksV2Client.UI.Text("0 FPS", 4, 4, SKColor.Parse("#58aff3"), SKTypeface.Default, SKTypeface.Default.ToFont(), 18f, SKTextAlign.Left);
            configs.loadDefaults();
            configs.loadFromFile(SettingsPath);
        }
        protected void cleanup()
        {
            this.isRunning = false;
        }
        protected void input()
        {
            this.keyboard.Update();
            this.mouse.Update();
            if (this.states.Any()) {
                this.states.Last().input(this);
            }
        }
        protected void update(float deltaTime)
        {
            this.window.SetMouseCursorVisible(this.showRealCursor);
            if (this.states.Any()) {
                this.states.Last().update(this, deltaTime);
            }
            this.cursorPositionDest = new SKRect(
                mouse.RawMousePosition.X,
                mouse.RawMousePosition.Y - 64,
                mouse.RawMousePosition.X + 64,
                mouse.RawMousePosition.Y
            );
        }
        protected void render(SKCanvas canvas, RenderStates renderStates)
        {
            canvas.Clear(SKColors.Black);
            if (this.states.Any()) {
                PageState last = this.states.Last();
                last.prerender(this, canvas, renderStates);
                last.render(this, canvas, renderStates);
                last.postrender(this, canvas, renderStates);
            }
            if (!this.showRealCursor) {
                canvas.DrawImage(this.cursorImage, this.cursorPositionSrc, this.cursorPositionDest, this.cursorPaint);
            }
            if (this.renderFPS) {
                fpsItem.updateText($"{( 1 / this.currentFps ):F1} FPS");
                fpsItem.Render(this, canvas);
            }
            if (this.renderDemoVersion) {
                demoItem.Render(this, canvas);
            }
        }
        private Vector2i ScaleMousePosition(Vector2i mousePos)
        {
            int scaledX = (int) ( mousePos.X * (float) Game.targetWidth / this.displayWidth );
            int scaledY = (int) ( mousePos.Y * (float) Game.targetHeight / this.displayHeight );
            return new Vector2i(scaledX, scaledY);
        }

        public void Dispose()
        {
            //if (pixelsHandle != IntPtr.Zero) GCHandle.FromIntPtr(pixelsHandle).Free();
            this.cursorImage?.Dispose();
            this.window?.Close();
            this.states?.Clear();
            this.states = null;
            this.surface?.Dispose();
            this.sprite?.Dispose();
            this.texture?.Dispose();
            this.bitmap?.Dispose();
        }

        public void startNewGame()
        {

        }

        public void startLoadGame()
        {

        }

        public void startMultiplayerGame()
        {

        }
    }
}
