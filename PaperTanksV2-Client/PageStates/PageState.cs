using SkiaSharp;

namespace PaperTanksV2Client.PageStates
{
    interface PageState
    {
        void init(GameEngine game);
        void input(GameEngine game);
        void update(GameEngine game, double deltaTime);
        void prerender(GameEngine game, SKCanvas canvas);
        void render(GameEngine game, SKCanvas canvas);
        void postrender(GameEngine game, SKCanvas canvas);
    }
}
