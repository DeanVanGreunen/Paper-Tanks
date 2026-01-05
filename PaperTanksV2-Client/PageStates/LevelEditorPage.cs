using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.UI;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PaperTanksV2Client.PageStates
{
    public class LevelEditorPage : PageState, IDisposable
    {
        // Finate State Machine
        private LevelEditorPageState currentMenu = LevelEditorPageState.MainMenu;

        // Main Menu
        private List<MenuItem> MainMenuItems = new List<MenuItem>();
        private int CurrentPage = 0;
        private int TotalPages = 1;
        private List<MenuItem> LevelEditorMenuItems = new List<MenuItem>();
        private List<MenuItem> LevelEditorMenuPopUpItems = new List<MenuItem>();
        private List<Level> levels = new List<Level>();
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKTypeface secondMenuTypeface = null;
        private SKFont secondMenuFont = null;
        private readonly int PAGE_SIZE = 10;
        private int RedLineX = 1550;
        private bool showError = false;
        private string errorText = "";

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
            menuTypeface =
                SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf"));
            menuFont = new SKFont(menuTypeface, 72);
            bool loaded3 = game.resources.Load(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf");
            if (!loaded3) throw new Exception("Error Loading Menu Font");
            secondMenuTypeface =
                SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font,
                    "Aaa-Prachid-Hand-Written.ttf"));
            secondMenuFont = new SKFont(menuTypeface, 72);
            Vector2Data viewSize = new Vector2Data(
                game.bitmap.Width * 2,
                game.bitmap.Height
            );
            this.viewPort = new ViewPort(viewSize, new QuadTree(new BoundsData(new Vector2Data(0, 0), viewSize)));
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
            this.levels = new List<Level>();
            this.levels.Clear();
            List<string> levelNames = game.resources.GetList();
            foreach (string levelName in levelNames) {
                bool levelExtracted = game.resources.Load(ResourceManagerFormat.Level, Path.GetFileName(levelName));
                if (levelExtracted == true) {
                    Level level = game.resources.Get(ResourceManagerFormat.Level, levelName) as Level;
                    if (level != null) {
                        level.fileName = levelName;
                        this.levels.Add(level);
                    }
                }
            }
        }

        public void input(Game game)
        {
            if (currentMenu == LevelEditorPageState.MainMenu) {
                foreach (MenuItem b in MainMenuItems) {
                    b.Input(game);
                }

                if (NeedsUIRefresh) {
                    NeedsUIRefresh = false;
                    this.GenerateUI(game);
                }
            } else if (currentMenu == LevelEditorPageState.LevelEditor) {
                if (showSavePopUp == false) {
                    foreach (MenuItem b in LevelEditorMenuItems) {
                        b.Input(game);
                    }

                    if (NeedsUIRefresh) {
                        NeedsUIRefresh = false;
                        this.GenerateEditorMenu(game);
                    }
                } else {
                    foreach (MenuItem b in LevelEditorMenuPopUpItems) {
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
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            paperRenderer.Render(canvas, viewPort);
            // Show redline for when we are in the level editor level design ui / menu
            if (canvas != null && game != null && currentMenu == LevelEditorPageState.LevelEditor)
                canvas.DrawLine(RedLineX, 0, RedLineX, game.bitmap.Height, RedLine);
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            if (currentMenu == LevelEditorPageState.MainMenu) {
                foreach (MenuItem b in MainMenuItems) {
                    b.Render(game, canvas);
                }
            } else if (currentMenu == LevelEditorPageState.LevelEditor) {
                foreach (MenuItem b in LevelEditorMenuItems) {
                    b.Render(game, canvas);
                }

                if (this.showSavePopUp) {
                    paperRenderer.RenderInBounds(canvas, PopUp);
                    foreach (MenuItem b in this.LevelEditorMenuPopUpItems) {
                        b.Render(game, canvas);
                    }
                }
            }
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        private List<string> GetPaginationItems(int currentPage, int totalPages)
        {
            var items = new List<string>();

            if (totalPages <= 7) {
                // Show all pages if 7 or fewer
                for (int i = 1; i <= totalPages; i++) {
                    items.Add(i.ToString());
                }
            } else {
                // Always show first page
                items.Add("1");

                if (currentPage <= 3) {
                    // Near start: 1 2 3 4 ... last
                    for (int i = 2; i <= 4; i++) {
                        items.Add(i.ToString());
                    }

                    items.Add("...");
                    items.Add(totalPages.ToString());
                } else if (currentPage >= totalPages - 2) {
                    // Near end: 1 ... last-3 last-2 last-1 last
                    items.Add("...");
                    for (int i = totalPages - 3; i <= totalPages; i++) {
                        items.Add(i.ToString());
                    }
                } else {
                    // Middle: 1 ... current-1 current current+1 ... last
                    items.Add("...");
                    items.Add(( currentPage - 1 ).ToString());
                    items.Add(currentPage.ToString());
                    items.Add(( currentPage + 1 ).ToString());
                    items.Add("...");
                    items.Add(totalPages.ToString());
                }
            }

            return items;
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
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Level Editor", leftX, topY, SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Menu", leftX, topY, SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 48f, SKTextAlign.Left));
            topY += spacingY;
            MainMenuItems.Add(new Button("- New Level", leftX + indentX, topY, SKColors.Black, SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    saveLevelName = "Type name here";
                    // Create New Level
                    this.currentLevel = new Level();
                    this.currentLevel.levelName = "New Level";
                    this.currentLevel.gameObjects = new List<GameObject>().ToArray();
                    this.currentLevel.isMultiplayer = false;
                    this.currentLevel.playerPosition = new Vector2();
                    this.currentLevel.playerSpawnPoints = new List<Vector2>().ToArray();
                    this.showSavePopUp = false;
                    this.showError = false;
                    // Switch to Level Editor
                    this.currentMenu = LevelEditorPageState.LevelEditor;
                    this.NeedsUIRefresh = true;
                    this.GenerateEditorMenu(game);
                }));
            topY += spacingSmallY;
            MainMenuItems.Add(new Button("- Back to main menu", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    MainMenuPage mmp = new MainMenuPage();
                    mmp.init(game);
                    mmp.SetForceOpen();
                    game.states.Clear();
                    game.states.Add(mmp);
                }));
            topY += spacingY;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Levels:", leftX, topY, SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 64f, SKTextAlign.Left));
            topY += 0;
            int oldX = leftX + 180;
            int pagesLeftXSpacing = 48;
            // Pages
            var paginationItems =
                GetPaginationItems(this.CurrentPage + 1, this.TotalPages); // +1 if CurrentPage is 0-indexed
            int xOffset = 0;

            foreach (var item in paginationItems) {
                if (item == "...") {
                    // Add ellipsis (non-clickable)
                    MainMenuItems.Add(new ButtonWithCircle("...", oldX + xOffset, topY + 8, SKColors.Gray,
                        SKColors.Transparent, menuTypeface, menuFont, 48f, SKTextAlign.Left,
                        (g) => {
                        }, false, true));
                } else {
                    int pageNum = int.Parse(item);
                    MainMenuItems.Add(new ButtonWithCircle(item, oldX + xOffset, topY + 8, SKColors.Black,
                        SKColor.Parse("#58aff3"), menuTypeface, menuFont, 48f, SKTextAlign.Left,
                        (g) => {
                            // Handle page click
                            this.CurrentPage = pageNum - 1;
                            this.NeedsUIRefresh = true;
                        }, this.CurrentPage == pageNum - 1));
                }

                xOffset += pagesLeftXSpacing;
            }

            MainMenuItems.Add(new PaperTanksV2Client.UI.Text(
                "- pages",
                oldX + xOffset + ( ( pagesLeftXSpacing * ( paginationItems.Count) ) ),
                topY + 8,
                SKColors.Red,
                menuTypeface,
                menuFont,
                42f,
                SKTextAlign.Left));
            if (this.levels != null && this.levels.Count >= 1) {
                topY += spacingSmallY;
                MainMenuItems.Add(new PaperTanksV2Client.UI.TextWithRotation(
                    "click to edit",
                    pagesLeftXSpacing - 44,
                    topY + 220,
                    SKColors.Red,
                    menuTypeface,
                    menuFont,
                    42f,
                    SKTextAlign.Left,
                    -72));
                List<Level> paginatedLevels =
                    this.levels?.Skip(this.CurrentPage * PAGE_SIZE)?.Take(PAGE_SIZE)?.ToList() ?? new List<Level>();
                foreach (Level level in paginatedLevels) {
                    topY += spacingSmallY;
                    MainMenuItems.Add(new Button(level.levelName, leftX + indentX, topY, SKColors.Black,
                        SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                            this.currentLevel = level;
                            this.currentLevelFileName = this.currentLevel.fileName;
                            this.saveLevelName = level.levelName ?? "Unknown Level Name";
                            this.currentMenu = LevelEditorPageState.LevelEditor;
                            this.GenerateEditorMenu(game);
                            this.NeedsUIRefresh = true;
                        }));
                }
            } else {
                topY += spacingSmallY;
                MainMenuItems.Add(new PaperTanksV2Client.UI.TextWithRotation(
                    "No levels here, create one!",
                    pagesLeftXSpacing,
                    topY + spacingSmallY,
                    SKColors.Red,
                    menuTypeface,
                    menuFont,
                    42f,
                    SKTextAlign.Left,
                    0));
            }
        }

        private void GenerateEditorMenu(Game game)
        {
            this.LevelEditorMenuItems.Clear();
            // Setup Main Menu Items
            int topY = 48;
            int spacingY = 64;
            int spacingSmallY = 32;
            int indentX = 32;
            int leftX = 1600;
            LevelEditorMenuItems.Add(new PaperTanksV2Client.UI.Text("Level Editor", leftX, topY,
                SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            LevelEditorMenuItems.Add(new PaperTanksV2Client.UI.Text("Add:", leftX, topY, SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 48f, SKTextAlign.Left));
            topY += spacingSmallY + 8;
            LevelEditorMenuItems.Add(new Button("Enemy Tank", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Player Spawn Point", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Wall Short", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Wall Large", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingY;
            LevelEditorMenuItems.Add(new PaperTanksV2Client.UI.Text("Selected:", leftX, topY, SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 48f, SKTextAlign.Left));
            topY += spacingSmallY + 8;
            LevelEditorMenuItems.Add(new Button("Remove Item", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Rotate Item", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingY;
            LevelEditorMenuItems.Add(new PaperTanksV2Client.UI.Text("Level:", leftX, topY, SKColor.Parse("#58aff3"),
                menuTypeface, menuFont, 48f, SKTextAlign.Left));
            topY += spacingSmallY + 8;
            LevelEditorMenuItems.Add(new Button("Save Level", leftX + indentX, topY, SKColors.Black,
                SKColors.Green, menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                    this.showSavePopUp = true;
                    this.GenerateEditorMenuPopUp(game);
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Delete Level", leftX + indentX, topY, SKColors.Black,
                SKColors.Red, menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    if (this.currentLevelFileName != null && this.currentLevelFileName.Trim() != "") {
                        Level.DeleteLevel(game, this.currentLevelFileName);
                    }
                    this.currentLevel = null;
                    this.currentLevelFileName = "";
                    this.LoadLevels(game);
                    this.GenerateUI(game);
                    this.currentMenu = LevelEditorPageState.MainMenu;
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Clear Level", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Go Back", leftX + indentX, topY, SKColors.Black,
                SKColors.Orange, menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.currentMenu = LevelEditorPageState.MainMenu;
                    this.NeedsUIRefresh = true;
                    this.currentLevel = null;
                }));
        }

        private void GenerateEditorMenuPopUp(Game game)
        {
            this.LevelEditorMenuPopUpItems.Clear();
            // Setup PopUp UI Elements
            int topY = 24 + (int) this.PopUp.Position.Y;
            int bottomY = (int) this.PopUp.Position.Y + ( (int) this.PopUp.Size.Y - 82 );
            int spacingY = 64;
            int spacingSmallY = 32;
            int indentX = 32;
            int leftX = (int) this.PopUp.Position.X + indentX;
            int rightX = (int) this.PopUp.Position.X + ( (int) this.PopUp.Size.X - indentX ) - 125;
            this.LevelEditorMenuPopUpItems.Add(new PaperTanksV2Client.UI.Text("Save Level as", leftX, topY,
                SKColor.Parse("#707070"),
                menuTypeface, menuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            this.LevelEditorMenuPopUpItems.Add(new PaperTanksV2Client.UI.TextInput(
                saveLevelName,
                leftX,
                topY,
                (int) this.PopUp.Size.X - 82,
                72,
                SKColor.Parse("#707070"),
                menuTypeface, menuFont, 48f, SKTextAlign.Left, (Game game, string newText) => {
                    this.saveLevelName = newText;
                    this.NeedsUIRefresh = true;
                    this.showError = false;
                }));
            if (this.showError) {
                topY += spacingY + 32;
                this.LevelEditorMenuPopUpItems.Add(new PaperTanksV2Client.UI.Text(this.errorText, leftX,
                    topY, SKColors.Red,
                    menuTypeface, menuFont, 32f, SKTextAlign.Left));
            }

            topY += spacingY;
            // BOTTOM ROW
            LevelEditorMenuPopUpItems.Add(new Button("Back", leftX, bottomY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 72f, SKTextAlign.Left, (g) => {
                    this.showSavePopUp = false;
                    this.LoadLevels(game);
                    this.NeedsUIRefresh = true;
                }));
            LevelEditorMenuPopUpItems.Add(new Button("Save", rightX, bottomY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 72f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                    // Generate filename if one doesn't exists
                    if (this.currentLevel == null) {
                        this.showError = true;
                        this.errorText = "Error: Level is broken - Null Exception";
                        return;
                    }
                    // TODO: Apply changes to currentLevel then save to file
                    this.currentLevel.levelName = this.saveLevelName;
                    if (this.currentLevelFileName == null || this.currentLevelFileName.Trim() == "") {
                        this.currentLevelFileName = $"{Guid.NewGuid()}.json";
                    }

                    if (!Level.Save(game, this.currentLevel, this.currentLevelFileName)) {
                        this.showError = true;
                        this.errorText = "Error: Unable to save level";
                        return;
                    }
                    this.LoadLevels(game);
                    this.currentMenu = LevelEditorPageState.MainMenu;
                    this.NeedsUIRefresh = true;
                    this.currentLevel = null;
                }));
        }
    }
}