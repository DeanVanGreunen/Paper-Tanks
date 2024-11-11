namespace PaperTanksV2Client
{
    class Program
    {
        static int Main(string[] args)
        {
            GameEngine game = new GameEngine();
            int exit_code = game.run();
            return exit_code;
        }
    }
}


/* TODO List:
 * [ ] Create Game Loop (init, input, update, render)
 * [ ] Create Resource Manager (Images, Fonts, Audio)
 * [ ] Create PageState Interface
 * [ ] Create Splash Page (single page)
 * [ ] Create Main Menu Page (double page) [New Game, Load Game, MultiPlayer (Client<->Server), Downloadable Content, Settings, About, Exit]
 * [ ] Create Downloadable Content Page (double page) [A List of downloadable content, multiplayer map packs, campaign level extensions]
 * [ ] Create Settings Page (double page) [Input Bindings, SFX Volume, Music Volume, Voice Over Volume, SpeedRun Timer Enabled]
 * [ ] Create About Page (double page) [Game Build Version)
 * [ ] Create Credits Page (double page) [Only Shown when the base campaign is completed]
 * 
 * [ ] Create GamePlayer (Support Campaign and Multiplayer Modes) [Multiplayer Models will eventually extend to have Peer-To-Peer Support Eventually]
 * 
 * Future Planned Feature:
 * [ ] Level Editor
 */
