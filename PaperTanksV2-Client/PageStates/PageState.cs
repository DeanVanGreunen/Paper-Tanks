using SFML.Graphics;
using SkiaSharp;

namespace PaperTanksV2Client.PageStates
{
    interface PageState
    {
        void init(GameEngine game);
        void input(GameEngine game);
        void update(GameEngine game, double deltaTime);
        void prerender(GameEngine game, SKCanvas canvas, RenderStates renderStates);
        void render(GameEngine game, SKCanvas canvas, RenderStates renderStates);
        void postrender(GameEngine game, SKCanvas canvas, RenderStates renderStates);
    }
}
