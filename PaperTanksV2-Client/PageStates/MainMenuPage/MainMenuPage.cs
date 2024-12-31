using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaperTanksV2Client.UI;
using SFML.Graphics;
using SkiaSharp;

namespace PaperTanksV2Client.PageStates
{
    public enum MainMenuEnum
    {
        MAIN = 0,
        MULTIPLAYER = 1,
        SETTINGS = 2
    }
    class MainMenuPage : PageState, IDisposable
    {
        private SKPaint p = new SKPaint();
        private SKImage backgroundTable = null;
        private SKImage coverPage = null;
        private SKImage leftPage = null;
        private SKImage rightPage = null;
        private bool isOpenned;
        private MainMenuEnum currentMenu = MainMenuEnum.MAIN;
        private List<Button> MainMenuButtons = new List<Button>();
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKPaint antiPaint = new SKPaint { 
            IsAntialias = false,
            FilterQuality = SKFilterQuality.High
        };
        public void init(GameEngine game)
        {
            this.p.Color = SKColors.Red;
            bool loaded = game.resources.Load(ResourceManagerFormat.Image, "background-table.jpeg");
            if (!loaded) throw new Exception("Error Loading Background Table");
            this.backgroundTable = (SKImage)game.resources.Get(ResourceManagerFormat.Image, "background-table.jpeg");
            bool loaded2 = game.resources.Load(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf");
            if (!loaded2) throw new Exception("Error Loading Menu Font");
            menuTypeface = SKTypeface.FromData((SKData)game.resources.Get(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf"));
            menuFont = new SKFont(menuTypeface, 72);
            coverPage = Helper.DrawCoverPageAsImage((int)(GameEngine.targetWidth / 2), (int)GameEngine.targetHeight);
            leftPage = Helper.DrawPageAsImage(true, (int)(GameEngine.targetWidth / 2), (int)GameEngine.targetHeight);
            rightPage = Helper.DrawPageAsImage(false, (int)(GameEngine.targetWidth / 2), (int)GameEngine.targetHeight);
            MainMenuButtons.Add(new Button("New Game", 1139, 200, 473, 72, SKColors.Black, SKColor.Parse("#58aff3"), menuFont, () => { }));
            MainMenuButtons.Add(new Button("Load Game", 1139, 320, 473, 72, SKColors.Black, SKColor.Parse("#58aff3"), menuFont, () => { }));
            MainMenuButtons.Add(new Button("Multiplayer", 1109, 440, 473, 72, SKColors.Black, SKColor.Parse("#58aff3"), menuFont, () => { }));
            MainMenuButtons.Add(new Button("Settings", 1139, 546, 473, 72, SKColors.Black, SKColor.Parse("#58aff3"), menuFont, () => { }));
            MainMenuButtons.Add(new Button("Quit Game", 1139, 665, 473, 72, SKColors.Black, SKColor.Parse("#58aff3"), menuFont, () => {
                game.isRunning = false; // Exit Game
            }));
        }

        public void input(GameEngine game)
        {
            // TODO:
            // REGISTER FIRST CLICK TO START COVER FLIP ANIMATION
            // REGISTER MAIN MENU ONCE IT IS SHOWN
            if (currentMenu == MainMenuEnum.MAIN)
            {
                foreach (Button b in MainMenuButtons) {
                    b.Input(game);
                }
            }
        }
        public void update(GameEngine game, double deltaTime)
        {
            // HANDLE START COVER FLIPPING TRANSITION
            // HANDLE MAIN MENU INTERACTIONS (ALSO SHOW SUBMENU'S AND HANDLE INPUTS/UPDATES FOR IT TOO)
        }
        public void prerender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
            // ALWAYS RENDER THE TABLE BELOW
            if(backgroundTable != null) canvas.DrawImage(this.backgroundTable, new SKRect(0, 0, GameEngine.targetWidth, GameEngine.targetHeight));
        }

        public void render(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
            if (!isOpenned)
            {
                canvas.DrawImage(rightPage, GameEngine.targetWidth / 2, 0, antiPaint);
            }
            // RENDER THE PAGES AND MENUS WHICH ARE ON TOP OF THEM
            if (currentMenu == MainMenuEnum.MAIN)
            {
                foreach (Button b in MainMenuButtons)
                {
                    b.Render(game, canvas);
                }                
            }
        }

        public void postrender(GameEngine game, SKCanvas canvas, RenderStates renderStates)
        {
            // RENDER ANY OTHER DEBUGGING INFORMTAION HERE
        }

        public void Dispose()
        {
            // CLEAN UP
            if (p != null) p.Dispose();
            if (backgroundTable != null) backgroundTable.Dispose();
            if (coverPage != null) coverPage.Dispose();
            if (leftPage != null) leftPage.Dispose();
            if (rightPage != null) rightPage.Dispose();
        }
    }
}
