namespace PaperTanksV2_Client.PageStates
{
    interface PageState
    {
        void init();
        void input();
        void update();
        void prerender();
        void render();
        void postrender();
    }
}
