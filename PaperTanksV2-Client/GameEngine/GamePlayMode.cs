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

        public void init(Game game)
        {
            this.engine = new GameEngineInstance(false, null);
        }

        public void loadLevel(int levelNumber)
        {
            this.loadLevel(levelNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public void loadLevel(string levelName) {
            // TODO: Hanlde Loading the level here
            // - Load Level Data
            // - Create Level by adding objects to game objects by user (first)
            // - Load Player Game Data
            // - Add Player Object to game. (last)
        }

        public void input(Game game)
        {
            // Check for keyboard and mouse input actions
            // Send inputs to game engine
        }

        public void update(Game game, Double deltaTime)
        {
            // tell the game engine to update
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            // render background table
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
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
