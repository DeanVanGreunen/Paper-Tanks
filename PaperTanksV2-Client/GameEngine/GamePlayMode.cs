using Newtonsoft.Json;
using PaperTanksV2Client.PageStates;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    class GamePlayMode : PageState, IDisposable
    {
        GameEngineInstance engine;
        ViewPort viewPort;

        public void init(Game game)
        {
            this.engine = new GameEngineInstance(false, null);
            this.viewPort = new ViewPort();
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
            // Check for keyboard and mouse input actions
            // Send inputs to game engine
        }

        public void update(Game game, float deltaTime)
        {
            engine.Update(deltaTime);
            viewPort.CenterAround(engine.GetObject(engine.playerID));
            // Center Viewport around player
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            // render background table
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            // pass rendering the gameobjects to the viewport
            // then render the viewport with the paper background scalled correctly.
            // render game objects
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            // render any debug data
        }
        public void Dispose()
        {
            if (this.engine != null) {
                this.engine.Dispose();
            }
        }
    }
}
