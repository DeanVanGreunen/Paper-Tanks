using SFML.Graphics;
using SkiaSharp;

namespace PaperTanksV2Client.PageStates
{
    interface PageState
    {
        void init(Game game);
        void input(Game game);
        void update(Game game, double deltaTime);
        void prerender(Game game, SKCanvas canvas, RenderStates renderStates);
        void render(Game game, SKCanvas canvas, RenderStates renderStates);
        void postrender(Game game, SKCanvas canvas, RenderStates renderStates);
    }
}
