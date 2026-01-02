using Gdk;
using Newtonsoft.Json;
using PaperTanksV2Client.PageStates;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Keyboard = SFML.Window.Keyboard;

namespace PaperTanksV2Client.GameEngine
{
    class GamePlayMode : PageState, IDisposable
    {
        GameEngineInstance engine;
        ViewPort viewPort;
        PaperPageRenderer paperRenderer;
        
        public void init(Game game)
        {
            BoundsData worldSpace = new BoundsData(new Vector2Data(0,0), new Vector2Data(4096,4096));
            this.engine = new GameEngineInstance(false, null, new QuadTree(worldSpace));
            Vector2Data viewSize = new Vector2Data(
                game.bitmap.Width * 2, 
                game.bitmap.Height
            );
            this.viewPort = new ViewPort(viewSize, engine.quadTree);
            this.paperRenderer = new PaperPageRenderer(
                pageWidth: game.bitmap.Width * 2,
                pageHeight: game.bitmap.Height,
                spacing: 20,
                totalLines: 60
            );
        }

        public bool LoadLevel(Game game, string levelName) {
            try {
                Console.WriteLine(game.resources.GetResourcePath(ResourceManagerFormat.Level, levelName + ".json"));
                if (!game.resources.Load(ResourceManagerFormat.Level, levelName + ".json")) {
                    Console.WriteLine("No Level File Found");
                    return false;
                }
                Level level = game.resources.Get(ResourceManagerFormat.Level, levelName + ".json") as Level;
                if (level == null) {
                    Console.WriteLine("No Level Found");
                    return false;
                }
                PlayerData pData = PlayerData.Load(game);
                if (pData == null) {
                    Console.WriteLine("No Player Data Found");
                     pData = PlayerData.NewPlayer(game);
                }
                Console.WriteLine(pData.ToString());
                this.engine.LoadPlayerWithLevel(pData, level);
                return true;
            } catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
        }

        public void input(Game game)
        {
            // Move Player (Locally and on via the Server)
            if (game.keyboard.IsKeyPressed(Keyboard.Key.Left)) {
                this.viewPort.MoveBy(-1, 0);
            } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Right)) {
                this.viewPort.MoveBy(1, 0);
            } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Up)) {
                this.viewPort.MoveBy(0, -1);
            } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Down)) {
                this.viewPort.MoveBy(0, 1);
            }
        }

        public void update(Game game, float deltaTime)
        {
            engine.Update(deltaTime);
            GameObject player = engine.GetObject(engine.playerID);
            if (player != null) {
                viewPort.CenterAround(player);
            }
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            paperRenderer.Render(canvas, viewPort);
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            Rectangle viewRect = this.viewPort.View.getRectangle();
            List<GameObject> gobjs = this.engine.GetObjects();
            viewPort.Render(game, canvas, gobjs);
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
