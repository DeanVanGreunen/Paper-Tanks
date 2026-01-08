using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.GameEngine.data;
using PaperTanksV2Client.UI;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.PageStates
{
    public enum MainMenuEnum
    {
        MAIN = 0,
        SETTINGS = 1,
        LOADCAMPAIGN = 2,
        MULTIPLAYER = 3,
        CREDITS = 4,
    }
    class MainMenuPage : PageState, IDisposable
    {
        private SKPaint p = new SKPaint();
        private SKImage coverPage = null;
        private SKImage leftPage = null;
        private SKImage rightPage = null;
        private SKImage table = null;
        private bool isOpenned;
        private float timePassed = 0f;
        private float waitTime = 0f;
        private readonly float totalWaitTime = 0.90f;
        private float totalTime = 1.75f;    
        private float t = 0f;
        private MainMenuEnum currentMenu = MainMenuEnum.MAIN;
        private List<MenuItem> MainMenuItems = new List<MenuItem>();
        private List<MenuItem> SettingsMenuItems = new List<MenuItem>();
        private List<MenuItem> CreditMenuItems = new List<MenuItem>();
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKTypeface secondMenuTypeface = null;
        private SKFont secondMenuFont = null;
        private SKPaint antiPaint = new SKPaint {
            IsAntialias = false,
            FilterQuality = SKFilterQuality.High
        };
        public void init(Game game)
        {
            this.p.Color = SKColors.Red;
            bool loaded2 = game.resources.Load(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf");
            if (!loaded2) throw new Exception("Error Loading Menu Font");
            menuTypeface = SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf"));
            menuFont = new SKFont(menuTypeface, 72);
            bool loaded3 = game.resources.Load(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf");
            if (!loaded3) throw new Exception("Error Loading Menu Font");
            secondMenuTypeface = SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf"));
            secondMenuFont = new SKFont(menuTypeface, 72);
            bool loaded4 = game.resources.Load(ResourceManagerFormat.Image, "cover.png");
            if (!loaded4) throw new Exception("Error Loading Menu Cover Page");
            coverPage = (SKImage) game.resources.Get(ResourceManagerFormat.Image, "cover.png");
            bool loaded5 = game.resources.Load(ResourceManagerFormat.Image, "table.png");
            if (!loaded5) throw new Exception("Error Loading Menu Table Page");
            table = (SKImage) game.resources.Get(ResourceManagerFormat.Image, "table.png");
            leftPage = Helper.DrawPageAsImage(true, (int) ( Game.targetWidth / 2 ), (int) Game.targetHeight);
            rightPage = Helper.DrawPageAsImage(false, (int) ( Game.targetWidth / 2 ), (int) Game.targetHeight);
            int leftX = ( (int) Game.targetWidth / 2 ) + 128;
            // Setup Main Menu Items
            int topY = 48;
            int spacingY = 62;
            int spacingYSmall = 32;
            int xIndent = 62;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Paper Tanks", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            MainMenuItems.Add(new Button("- New Game", leftX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left, (g) => {
                var campaign = new GamePlayMode();
                campaign.init(game);
                campaign.LoadLevel(game, null, (Game game) => {
                    var creditsPage = new MainMenuPage();
                    creditsPage.init(game);
                    game.states.Clear();
                    game.states.Add(creditsPage);
                });
                game.states.Clear();
                game.states.Add(campaign);
            }));
            topY += spacingY;
            MainMenuItems.Add(new Button("- Load Game", leftX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left, (g) => { currentMenu = MainMenuEnum.LOADCAMPAIGN; }, true));
            topY += spacingY;
            MainMenuItems.Add(new Button("- Multiplayer", leftX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left, (g) => { currentMenu = MainMenuEnum.MULTIPLAYER; }, true)); // TODO: ENABLE ONCE THIS FEATURE HAS BEEN COMPLETED
            topY += spacingY;
            MainMenuItems.Add(new Button("- Level Editor", leftX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left,
                (g) => {
                    var levelEditor = new LevelEditorPage();
                    levelEditor.init(game);
                    game.states.Clear();
                    game.states.Add(levelEditor);
                }, false));
            topY += spacingY;
            MainMenuItems.Add(new Button("- Settings", leftX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left, (g) => { currentMenu = MainMenuEnum.SETTINGS; }));
            topY += spacingY;
            MainMenuItems.Add(new Button("- Credits", leftX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left,
                (g) => {
                    currentMenu = MainMenuEnum.CREDITS;
                }));
            topY += spacingY;
            MainMenuItems.Add(new Button("- Quit Game", leftX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left, (g) => { g.isRunning = false; }));
            // # Setup Setting's Menu Items
            // - Music Toggle
            topY = 48;
            spacingY = 62;
            SettingsMenuItems.Add(new PaperTanksV2Client.UI.Button("<", leftX - 28, topY - 20, SKColors.Black, SKColor.Parse("#58aff3"), secondMenuTypeface, secondMenuFont, 82f, SKTextAlign.Left, (g) => {
                game.configs.saveToFile(Game.SettingsPath);
                currentMenu = MainMenuEnum.MAIN;
            }));
            SettingsMenuItems.Add(new PaperTanksV2Client.UI.Text("Settings", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            SettingsMenuItems.Add(new Toggle("Music", leftX, topY, 32, 32, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, game.configs.get("Music", true), (g, v) => {
                game.configs.set("Music", v);
            }));
            topY += spacingY;
            SettingsMenuItems.Add(new Toggle("Sound SFX", leftX, topY, 32, 32, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, game.configs.get("SFX", true), (g, v) => {
                game.configs.set("SFX", v);
            }));
            // # Setup Setting's Menu Items
            // - Music Toggle
            topY = 48;
            spacingY = 62;
            CreditMenuItems.Add(new PaperTanksV2Client.UI.Button("<", leftX - 28, topY - 20, SKColors.Black, SKColor.Parse("#58aff3"), secondMenuTypeface, secondMenuFont, 82f, SKTextAlign.Left, (g) => {
                game.configs.saveToFile(Game.SettingsPath);
                currentMenu = MainMenuEnum.MAIN;
            }));
            CreditMenuItems.Add(new PaperTanksV2Client.UI.Text("Credits", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            foreach (string credit in TextData.Credits) {
                CreditMenuItems.Add(new PaperTanksV2Client.UI.Text(credit, leftX + xIndent, topY, SKColors.Black, menuTypeface, menuFont, 42f, SKTextAlign.Left));
                topY += spacingYSmall;
            }
        }

        public void SetForceOpen()
        {
            this.waitTime = this.totalWaitTime;
            this.isOpenned = true;
        }

        public void input(Game game)
        {
            if (!this.isOpenned) {
                return;
            }
            if (currentMenu == MainMenuEnum.MAIN) {
                foreach (MenuItem b in MainMenuItems) {
                    b.Input(game);
                }
            } else if (currentMenu == MainMenuEnum.SETTINGS) {
                foreach (MenuItem b in SettingsMenuItems) {
                    b.Input(game);
                }                
            } else if (this.currentMenu == MainMenuEnum.CREDITS) {
                foreach (MenuItem b in CreditMenuItems) {
                    b.Input(game);
                }
            }
        }
        public void update(Game game, float deltaTime)
        {
            // HANDLE START COVER FLIPPING TRANSITION
            this.waitTime += (float) deltaTime;
            if (!this.isOpenned && this.waitTime > this.totalWaitTime) {
                this.timePassed += (float) deltaTime;
                this.t = this.timePassed / this.totalTime;
                if (t > 1) this.isOpenned = true;
                return;
            }
            // HANDLE MAIN MENU INTERACTIONS (ALSO SHOW SUBMENU'S AND HANDLE INPUTS/UPDATES FOR IT TOO)
        }
        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            // ALWAYS RENDER THE TABLE BELOW
            canvas.Clear(SKColors.White);
            canvas.DrawImage(table, new SKRect(0, 0, table.Width, table.Height));
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            if (!isOpenned) {
                Helper.RenderPageFlipFromBitmapsAndCallbackToRenderRightSide(canvas, SKBitmap.FromImage(this.coverPage), SKBitmap.FromImage(this.leftPage), SKBitmap.FromImage(this.rightPage), t, game, (g, c) => {
                    foreach (MenuItem b in MainMenuItems) {
                        b.Render(g, c);
                    }
                });
            } else {
                SKBitmap rightImage = SKBitmap.FromImage(this.rightPage);
                SKMatrix leftMatrix = SKMatrix.CreateTranslation(0, 0);
                SKMatrix rightMatrix = SKMatrix.CreateTranslation(rightImage.Width, 0);
                canvas.Save();
                canvas.SetMatrix(leftMatrix);
                canvas.DrawBitmap(rightImage, new SKRect(0, 0, rightImage.Width, rightImage.Height));
                canvas.Restore();
                canvas.Save();
                canvas.SetMatrix(rightMatrix);
                canvas.DrawBitmap(rightImage, new SKRect(0, 0, rightImage.Width, rightImage.Height));
                canvas.Restore();
                canvas.Save();
                canvas.Save();
                canvas.DrawLine(rightImage.Width, 0, rightImage.Width, rightImage.Height, Helper.greyLinePaint);
                canvas.Restore();
                if (currentMenu == MainMenuEnum.MAIN) {
                    foreach (MenuItem b in MainMenuItems) {
                        b.Render(game, canvas);
                    }
                } else if (currentMenu == MainMenuEnum.SETTINGS) {
                    foreach (MenuItem b in SettingsMenuItems) {
                        b.Render(game, canvas);
                    }
                } else if (this.currentMenu == MainMenuEnum.CREDITS) {
                    foreach (MenuItem b in CreditMenuItems) {
                        b.Render(game, canvas);
                    }
                }
                canvas.Restore();
            }
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            // RENDER ANY OTHER DEBUGGING INFORMTAION HERE
        }

        public void Dispose()
        {
            // CLEAN UP
            if (p != null) p.Dispose();
            if (coverPage != null) coverPage.Dispose();
            if (leftPage != null) leftPage.Dispose();
            if (rightPage != null) rightPage.Dispose();
        }
    }
}
