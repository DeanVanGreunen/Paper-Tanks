using Gdk;
using Newtonsoft.Json;
using PaperTanksV2Client.PageStates;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keyboard = SFML.Window.Keyboard;

namespace PaperTanksV2Client.GameEngine
{
    class GamePlayMode : PageState, IDisposable
    {
        GameEngineInstance engine;
        ViewPort viewPort;
        PaperPageRenderer paperRenderer;
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKTypeface secondMenuTypeface = null;
        private SKFont secondMenuFont = null;

        private float movementSpeed = 100;

        private bool noEnemiesChecked = false;
        
        public Action<Game> creditsCallback;
        
        public void init(Game game)
        {
            bool loaded2 = game.resources.Load(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf");
            if (!loaded2) throw new Exception("Error Loading Menu Font");
            menuTypeface = SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf"));
            menuFont = new SKFont(menuTypeface, 72);
            bool loaded3 = game.resources.Load(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf");
            if (!loaded3) throw new Exception("Error Loading Menu Font");
            secondMenuTypeface = SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf"));
            secondMenuFont = new SKFont(menuTypeface, 72);
            BoundsData worldSpace = new BoundsData(new Vector2Data(0,0), new Vector2Data(4096,4096));
            this.engine = new GameEngineInstance(false, menuTypeface, menuFont, secondMenuTypeface, secondMenuFont);
            Vector2Data viewSize = new Vector2Data(
                game.bitmap.Width * 2, 
                game.bitmap.Height
            );
            this.viewPort = new ViewPort(viewSize);
            this.paperRenderer = new PaperPageRenderer(
                pageWidth: game.bitmap.Width * 2,
                pageHeight: game.bitmap.Height,
                spacing: 20,
                totalLines: 60
            );
        }

        public bool LoadLevel(Game game, string levelName, Action<Game> callback)
        {
            this.creditsCallback = callback;
            try {
                string levelName2 = CampaignManager.GetNextLevel(game, levelName);
                Level level = CampaignManager.LoadLevel(game, levelName2);
                PlayerData pData = PlayerData.Load(game);
                if (pData == null) {
                    Console.WriteLine("No Player Data Found");
                    pData = PlayerData.NewPlayer(game);
                }
                this.engine.LoadPlayerWithLevel(pData, level);
                return true;
            } catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
        }

        public void input(Game game)
        {
            
        }

        public void update(Game game, float deltaTime)
        {
            // Move Player (Locally and on via the Server)
            List<Tank> tanks = engine.GetObjectByType<Tank>();
            bool noEnemies = tanks.Count() >= 1 && tanks.Where(t => !t.IsPlayer).Count() == 0;
            GameObject player = engine.GetObject(engine.playerID);
            if (noEnemies && noEnemiesChecked == false) {
                noEnemiesChecked = true;
                this.creditsCallback?.Invoke(game);
                return;
            }
            if (player != null) {
                if (( player as Tank ).Health <= 0 && (player as Tank).deleteMe == false) {
                    ( player as Tank ).GetPlayerDiedCallback(game);
                }
                if (game.keyboard.IsKeyPressed(Keyboard.Key.Left)) {
                    player.MoveBy(-movementSpeed * deltaTime, 0 * deltaTime);
                    player.Rotation = -180;
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Right)) {
                    player.MoveBy(movementSpeed * deltaTime, 0 * deltaTime);
                    player.Rotation = 0;
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Up)) {
                    player.MoveBy(0 * deltaTime, -movementSpeed * deltaTime);
                    player.Rotation = -90;
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Down)) {
                    player.MoveBy(0 * deltaTime, movementSpeed * deltaTime);
                    player.Rotation = 90;
                }

                if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Space) && ( player as Tank ).Weapon0.AmmoCount >= 1) {
                    //player.Rotation;
                    Projectile projectile = new Projectile(SKColors.Red, player.Id);
                    Vector2Data size = new Vector2Data(8, 8);
                    if (player.Rotation == 0) {
                        projectile.Bounds =
                            new BoundsData(
                                new Vector2Data(player.Position.X + 100,
                                    player.Position.Y + ( player.Size.Y / 2 ) - ( size.Y / 2 )), size);
                        projectile.Velocity = new Vector2Data(this.movementSpeed, 0);
                    } else if (player.Rotation == -180) {
                        projectile.Bounds =
                            new BoundsData(
                                new Vector2Data(player.Position.X - 58,
                                    player.Position.Y + ( player.Size.Y / 2 ) - ( size.Y / 2 )), size);
                        projectile.Velocity = new Vector2Data(-this.movementSpeed, 0);
                    } else if (player.Rotation == -90) {
                        projectile.Bounds =
                            new BoundsData(
                                new Vector2Data(player.Position.X + ( player.Size.X / 2 ) - ( size.X / 2 ),
                                    player.Position.Y - 58), size);
                        projectile.Velocity = new Vector2Data(0, -this.movementSpeed);
                    } else if (player.Rotation == 90) {
                        projectile.Bounds =
                            new BoundsData(
                                new Vector2Data(player.Position.X + ( player.Size.X / 2 ) - ( size.X / 2 ),
                                    player.Position.Y + 100), size);
                        projectile.Velocity = new Vector2Data(0, this.movementSpeed);
                    }

                    ( player as Tank ).Weapon0.AmmoCount -= 1;
                    this.engine.QueueAddObject(projectile);
                }
            }
            engine.Update(game, deltaTime);
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            paperRenderer.Render(canvas, viewPort);
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            Dictionary<Guid, GameObject> gobjs = this.engine.GetObjects();
            viewPort.Render(game, canvas, gobjs.Values.ToList());
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            // render any debug data
            // Render debug data (FPS, player position, visible objects count, etc.)
            if (true) {
                using (var debugPaint = new SKPaint())
                {
                    debugPaint.Color = SKColors.Red;
                    debugPaint.TextSize = 16;
                    debugPaint.IsAntialias = true;
                    GameObject player = engine.GetObject(engine.playerID);
                    if (player != null) {
                        canvas.DrawText($"Player: ({player.Position.X:F1}, {player.Position.Y:F1})", 10, 40, debugPaint);
                    }
                    canvas.DrawText($"Total GameObjects: ({this.engine.GetObjectsCount:F1})", 10, 60, debugPaint);
                    canvas.DrawText($"Viewport: ({viewPort.View.Position.X:F1}, {viewPort.View.Position.Y:F1})", 10, 80, debugPaint);
                }
            }
        }
        public void Dispose()
        {
            if (this.engine != null) {
                this.engine.Dispose();
            }
        }
    }
}
