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
        private int CurrentPage = 0;
        private int TotalPages = 1;
        private List<MenuItem> MultiplayerMenuItems = new List<MenuItem>();
        private List<MenuItem> MultiplayerMenuPopUpItems = new List<MenuItem>();
        private List<Level> Levels = new List<Level>();
        private SKTypeface MenuTypeface = null;
        private SKFont MenuFont = null;
        private SKTypeface SecondMenuTypeface = null;
        private SKFont SecondMenuFont = null;
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
        private string saveLevelName = "Type name here";
        private BoundsData PopUp = new BoundsData(new Vector2Data(0, 0), new Vector2Data(0, 0));
        PaperPageRenderer paperRenderer;
        private bool NeedsUIRefresh = false;

        private bool showSavePopUp = false;

        // Level Editor Stuff
        private Level currentLevel = null;
        private string currentLevelFileName = null;

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
            bool loaded3 = game.resources.Load(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf");
            if (!loaded3) throw new Exception("Error Loading Menu Font");
            this.SecondMenuTypeface =
                SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font,
                    "Aaa-Prachid-Hand-Written.ttf"));
            this.SecondMenuFont = new SKFont(this.MenuTypeface, 72);
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
            this.LoadLevels(game);
            this.GenerateUI(game);
        }

        private void LoadLevels(Game game)
        {
            this.Levels = new List<Level>();
            this.Levels.Clear();
            List<string> levelNames = game.resources.GetList().OrderBy(s => Guid.Parse(s.Split("\\").Last().Replace(".json", ""))).ToList();
            foreach (string levelName in levelNames) {
                bool levelExtracted = game.resources.Load(ResourceManagerFormat.Level, Path.GetFileName(levelName));
                if (levelExtracted == true) {
                    Level level = game.resources.Get(ResourceManagerFormat.Level, levelName) as Level;
                    if (level != null) {
                        level.fileName = levelName;
                        this.Levels.Add(level);
                    }
                }
            }
        }

        public void input(Game game)
        {
            if (this.CurrentMenu == LevelEditorPageState.MainMenu) {
                foreach (MenuItem b in MainMenuItems) {
                    b.Input(game);
                }

                if (NeedsUIRefresh) {
                    NeedsUIRefresh = false;
                    this.GenerateUI(game);
                }
            } else if (this.CurrentMenu == LevelEditorPageState.LevelEditor) {
                if (showSavePopUp == false) {
                    foreach (MenuItem b in this.MultiplayerMenuItems) {
                        b.Input(game);
                    }
                    if (this.currentLevel != null) {
                            if (this.currentLevel.gameObjects != null) {
                                foreach (GameObject obj in this.currentLevel.gameObjects) {
                                    bool isInRect =
                                        game.mouse.ScaledMousePosition.X >= obj.Bounds.Position.X &&
                                        game.mouse.ScaledMousePosition.X < (obj.Bounds.Position.X + obj.Bounds.Size.X) &&
                                        game.mouse.ScaledMousePosition.Y >= obj.Bounds.Position.Y &&
                                        game.mouse.ScaledMousePosition.Y < (obj.Bounds.Position.Y + obj.Bounds.Size.Y);

                                    // Select with left-click OR middle-click for moving/rotating
                                    if (isInRect && (game.mouse.IsButtonPressed(Mouse.Button.Left) || game.mouse.IsButtonPressed(Mouse.Button.Middle)) && this.SelectedGameObjectID == null) {
                                        this.SelectedGameObjectID = obj.Id.ToString();
                                    }

                                    // Rotate with middle-click (limited to once per second)
                                    if (this.SelectedGameObjectID == obj.Id.ToString() && isInRect && game.mouse.IsButtonPressed(Mouse.Button.Middle)) {
                                        if (rotationCooldownTimer <= 0f) {
                                            obj.Rotation = (obj.Rotation + 45) % 360;
                                            rotationCooldownTimer = ROTATION_COOLDOWN;
                                        }
                                    }

                                    // Delete with right-click
                                    if (isInRect && game.mouse.IsButtonPressed(Mouse.Button.Right)) {
                                        this.DeleteSelectedGameObjectID = obj.Id.ToString();
                                    }

                                    // Move with left button only
                                    if (this.SelectedGameObjectID == obj.Id.ToString() && game.mouse.IsButtonPressed(Mouse.Button.Left)) {
                                        obj.Bounds.Position = new Vector2Data(
                                            game.mouse.ScaledMousePosition.X - (obj.Bounds.Size.X / 2),
                                            game.mouse.ScaledMousePosition.Y - (obj.Bounds.Size.Y / 2)
                                        );
                                    }
                                }

                                // Deselect when both left AND middle buttons are released
                                if (!game.mouse.IsButtonPressed(Mouse.Button.Left) && !game.mouse.IsButtonPressed(Mouse.Button.Middle)) {
                                    this.SelectedGameObjectID = null;
                                }
                            }
                        }
                    if (NeedsUIRefresh) {
                        NeedsUIRefresh = false;
                        this.GenerateEditorMenu(game);
                    }
                } else {
                    foreach (MenuItem b in this.MultiplayerMenuPopUpItems) {
                        b.Input(game);
                    }

                    if (NeedsUIRefresh) {
                        NeedsUIRefresh = false;
                        this.GenerateEditorMenu(game);
                        this.GenerateEditorMenuPopUp(game);
                    }
                }
            }
        }

        public void update(Game game, float deltaTime)
        {
            if (rotationCooldownTimer > 0f) {
                rotationCooldownTimer -= deltaTime;
            }
            if (this.DeleteSelectedGameObjectID != null) {
                this.currentLevel.gameObjects =
                    this.currentLevel.gameObjects.Where(o => o.Id.ToString() != this.DeleteSelectedGameObjectID).ToList();
                this.DeleteSelectedGameObjectID = null;
            }
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            paperRenderer.Render(canvas, viewPort);
            // Show redline for when we are in the level editor level design ui / menu
            if (canvas != null && game != null && this.CurrentMenu == LevelEditorPageState.LevelEditor)
                canvas.DrawLine(RedLineX, 0, RedLineX, game.bitmap.Height, RedLine);
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            if (this.CurrentMenu == LevelEditorPageState.MainMenu) {
                foreach (MenuItem b in MainMenuItems) {
                    b.Render(game, canvas);
                }
            } else if (this.CurrentMenu == LevelEditorPageState.LevelEditor) {
                foreach (MenuItem b in this.MultiplayerMenuItems) {
                    b.Render(game, canvas);
                }
                if(this.currentLevel != null){
                    foreach (GameObject b in this.currentLevel.gameObjects) {
                        b.InternalRender(game, canvas);
                    }
                }
                if (this.showSavePopUp) {   
                    paperRenderer.RenderInBounds(canvas, PopUp);
                    foreach (MenuItem b in this.MultiplayerMenuPopUpItems) {
                        b.Render(game, canvas);
                    }
                }
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
                    mmp.init(game);
                    mmp.Connect("127.0.0.1", 9091);
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