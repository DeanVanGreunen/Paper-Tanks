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
            levelName = levelName + ".json";
            if (!game.resources.Load(ResourceManagerFormat.Level, levelName)) {
                return false;
            }
            Level level = game.resources.Get(ResourceManagerFormat.Level, levelName) as Level;
            this.engine.LoadPlayerWithLevel(game.player.Load(game), level);
            return true;
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
