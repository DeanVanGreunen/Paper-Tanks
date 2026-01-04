using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.UI;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
        private int TotalPages = 10;
        private List<MenuItem> LevelEditorMenuItems = new List<MenuItem>();
        private List<MenuItem> LevelEditorMenuPopUpItems = new List<MenuItem>();
        private List<Level> levels = new List<Level>();
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKTypeface secondMenuTypeface = null;
        private SKFont secondMenuFont = null;
        private readonly int PAGE_SIZE = 10;
        private int RedLineX = 1550;
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

        private bool showSavePopUp = false;
        // Level Editor Stuff
        private Level currentLevel = null;
        
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
                    (game.bitmap.Width) - (xSpacing * 2),
                    (game.bitmap.Height) - (ySpacing * 2)
                ));
            this.loadLevels(game);
            this.GenerateUI(game);
        }

        private void loadLevels(Game game)
        {
            if (this.levels == null) this.levels = new List<Level>();
            this.levels.Clear();
            object levelsExtracted = game.resources.Get(ResourceManagerFormat.Levels, "levels.json");
            List<Level> levels =  new List<Level>();
            if (levelsExtracted != null) {
                levels =  levelsExtracted as List<Level>;
            }
            this.levels.AddRange(levels);
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
                if(canvas != null && game != null && currentMenu == LevelEditorPageState.LevelEditor) canvas.DrawLine(RedLineX, 0, RedLineX, game.bitmap.Height, RedLine);
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
                    // Create New Level
                    this.currentLevel = new Level();
                    this.currentLevel.levelName = "New Level";
                    this.currentLevel.gameObjects = new List<GameObject>().ToArray();
                    this.currentLevel.isMultiplayer = false;
                    this.currentLevel.playerPosition = new Vector2();
                    this.currentLevel.playerSpawnPoints = new List<Vector2>().ToArray();
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
                xOffset + ( ( pagesLeftXSpacing * ( paginationItems.Count ) ) ),
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
                List<Level> paginatedLevels = this.levels?.Skip(this.CurrentPage * PAGE_SIZE)?.Take(PAGE_SIZE)?.ToList() ?? new List<Level>();
                foreach (Level level in paginatedLevels) {
                    topY += spacingSmallY;
                    MainMenuItems.Add(new Button(level.levelName, leftX + indentX, topY, SKColors.Black,
                        SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                            this.currentLevel = level;
                            this.currentMenu = LevelEditorPageState.LevelEditor;
                            this.NeedsUIRefresh = true;
                        }));
                }
            } else {
                topY += spacingSmallY;
                MainMenuItems.Add(new PaperTanksV2Client.UI.TextWithRotation(
                    "No levels here, create one!",
                    pagesLeftXSpacing - 44,
                    topY + 380,
                    SKColors.Red,
                    menuTypeface,
                    menuFont,
                    42f,
                    SKTextAlign.Left,
                    -72));
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
            LevelEditorMenuItems.Add(new PaperTanksV2Client.UI.Text("Level Editor", leftX, topY, SKColor.Parse("#58aff3"),
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
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true;
                    this.showSavePopUp = true;
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Delete Level", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true; 
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Clear Level", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.NeedsUIRefresh = true; 
                }));
            topY += spacingSmallY;
            LevelEditorMenuItems.Add(new Button("Discard Changes and Go Back", leftX + indentX, topY, SKColors.Black,
                SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                    this.currentMenu = LevelEditorPageState.MainMenu;
                    this.NeedsUIRefresh = true;
                    this.currentLevel = null;
                }));
        }

        private void GenerateEditorMenuPopUp(Game game)
        {
            
        }
    }
}