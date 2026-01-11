using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.UI;
using SFML.Graphics;
using SFML.Window;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PaperTanksV2Client.PageStates
{
    public class MultiplayerPage : PageState, IDisposable
    {
        // Finate State Machine
        private MultiplayerPageState CurrentMenu = MultiplayerPageState.MainMenu;

        // Main Menu
        private List<MenuItem> MainMenuItems = new List<MenuItem>();
        private SKTypeface MenuTypeface = null;
        private SKFont MenuFont = null;
        private readonly int PAGE_SIZE = 10;
        private int RedLineX = 1550;
        private bool ShowError = false;
        private string ErrorText = "";
        private string SelectedGameObjectID = null;
        private string DeleteSelectedGameObjectID = null;
        private float rotationCooldownTimer = 0f;
        private const float ROTATION_COOLDOWN = 0.25f;
        
        private SKPaint antiPaint = new SKPaint {
            IsAntialias = false,
            FilterQuality = SKFilterQuality.High
        };

        private SKPaint p = new SKPaint();

        private SKPaint RedLine = new SKPaint() {
            Color = SKColors.Red
        };

        private ViewPort viewPort;
        private BoundsData PopUp = new BoundsData(new Vector2Data(0, 0), new Vector2Data(0, 0));
        PaperPageRenderer paperRenderer;
        private bool NeedsUIRefresh = false;
        public void Dispose()
        {
        }

        public void init(Game game)
        {
            this.p.Color = SKColors.Red;
            bool loaded2 = game.resources.Load(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf");
            if (!loaded2) throw new Exception("Error Loading Menu Font");
            this.MenuTypeface =
                SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf"));
            this.MenuFont = new SKFont(this.MenuTypeface, 72);
            Vector2Data viewSize = new Vector2Data(
                game.bitmap.Width * 2,
                game.bitmap.Height
            );
            this.viewPort = new ViewPort(viewSize);
            this.paperRenderer = new PaperPageRenderer(
                pageWidth: game.bitmap.Width * 2,
                pageHeight: game.bitmap.Height,
                spacing: 20,
                totalLines: 60
            );
            int xSpacing = 50;
            int ySpacing = 100;
            this.PopUp = new BoundsData(
                new Vector2Data(xSpacing, ySpacing),
                new Vector2Data(
                    ( game.bitmap.Width ) - ( xSpacing * 2 ),
                    ( game.bitmap.Height ) - ( ySpacing * 2 )
                ));
            this.GenerateUI(game);
        }

        public void input(Game game)
        {
            foreach (MenuItem b in MainMenuItems) {
                b.Input(game);
            }

            if (NeedsUIRefresh) {
                NeedsUIRefresh = false;
                this.GenerateUI(game);
            }
        }

        public void update(Game game, float deltaTime)
        {
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            paperRenderer.Render(canvas, viewPort);
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            foreach (MenuItem b in MainMenuItems) {
                b.Render(game, canvas);
            }
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        private void GenerateUI(Game game)
        {
            MainMenuItems.Clear();
            // Setup Main Menu Items
            int topY = 48;
            int spacingY = 64;
            int spacingSmallY = 32;
            int indentX = 32;
            int leftX = 52;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Multiplayer", leftX, topY, SKColor.Parse("#58aff3"),
                this.MenuTypeface, this.MenuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Menu", leftX, topY, SKColor.Parse("#58aff3"),
                this.MenuTypeface, this.MenuFont, 48f, SKTextAlign.Left));
            topY += spacingY;
            MainMenuItems.Add(new Button("- New Server", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"),
                this.MenuTypeface, this.MenuFont, 32f, SKTextAlign.Left, (g) => {
                    GameMultiPage mmp = new GameMultiPage();
                    mmp.Connect(game, "127.0.0.1", 9091);
                    mmp.init(game);
                    game.states.Clear();
                    game.states.Add(mmp);
                }));
            topY += spacingSmallY;
            MainMenuItems.Add(new Button("- Back to main menu", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), this.MenuTypeface, this.MenuFont, 32f, SKTextAlign.Left, (g) => {
                    MainMenuPage mmp = new MainMenuPage();
                    mmp.init(game);
                    mmp.SetForceOpen();
                    game.states.Clear();
                    game.states.Add(mmp);
                }));
            topY += spacingY;
        }
    }

    internal enum MultiplayerPageState
    {
        MainMenu = 1
    }
}