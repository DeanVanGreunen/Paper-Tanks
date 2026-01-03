using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.UI;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.PageStates
{
    public class LevelEditorPage : PageState, IDisposable
    {
        private LevelEditorPageState currentMenu = LevelEditorPageState.MainMenu;
        
        private List<MenuItem> MainMenuItems = new List<MenuItem>();
        private int CurrentPage = 0;
        private int TotalPages = 10;
        private List<MenuItem> LevelEditorMenuItems = new List<MenuItem>();
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKTypeface secondMenuTypeface = null;
        private SKFont secondMenuFont = null;
        private SKPaint antiPaint = new SKPaint {
            IsAntialias = false,
            FilterQuality = SKFilterQuality.High
        };
        private SKPaint p = new SKPaint();
        ViewPort viewPort;
        PaperPageRenderer paperRenderer;
        private bool needsUIRefresh = false;
        public void Dispose()
        {
            
        }

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
            this.GenerateUI(game);
        }

        public void input(Game game)
        {
            if (currentMenu == LevelEditorPageState.MainMenu) {
                foreach (MenuItem b in MainMenuItems) {
                    b.Input(game);
                }
                if (needsUIRefresh) {
                    needsUIRefresh = false;
                    GenerateUI(game);
                }
            } else if (currentMenu == LevelEditorPageState.LevelEditor) {
                foreach (MenuItem b in LevelEditorMenuItems) {
                    b.Input(game);
                }                
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
            if (currentMenu == LevelEditorPageState.MainMenu) {
                foreach (MenuItem b in MainMenuItems) {
                    b.Render(game, canvas);
                }
            } else if (currentMenu == LevelEditorPageState.LevelEditor) {
                foreach (MenuItem b in LevelEditorMenuItems) {
                    b.Render(game, canvas);
                }
            }
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }
        private List<string> GetPaginationItems(int currentPage, int totalPages)
        {
            var items = new List<string>();
    
            if (totalPages <= 7)
            {
                // Show all pages if 7 or fewer
                for (int i = 1; i <= totalPages; i++)
                {
                    items.Add(i.ToString());
                }
            }
            else
            {
                // Always show first page
                items.Add("1");
        
                if (currentPage <= 3)
                {
                    // Near start: 1 2 3 4 ... last
                    for (int i = 2; i <= 4; i++)
                    {
                        items.Add(i.ToString());
                    }
                    items.Add("...");
                    items.Add(totalPages.ToString());
                }
                else if (currentPage >= totalPages - 2)
                {
                    // Near end: 1 ... last-3 last-2 last-1 last
                    items.Add("...");
                    for (int i = totalPages - 3; i <= totalPages; i++)
                    {
                        items.Add(i.ToString());
                    }
                }
                else
                {
                    // Middle: 1 ... current-1 current current+1 ... last
                    items.Add("...");
                    items.Add((currentPage - 1).ToString());
                    items.Add(currentPage.ToString());
                    items.Add((currentPage + 1).ToString());
                    items.Add("...");
                    items.Add(totalPages.ToString());
                }
            }
    
            return items;
        }
        public void GenerateUI(Game game)
        {
            MainMenuItems.Clear();
            // Setup Main Menu Items
            int topY = 48;
            int spacingY = 64;
            int spacingSmallY = 32;
            int indentX = 32;
            int leftX = 52;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Level Editor", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 72f, SKTextAlign.Left));
            topY += spacingY;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Menu", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 48f, SKTextAlign.Left));
            topY += spacingY;
            MainMenuItems.Add(new Button("- New Level", leftX + indentX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                
            }));
            topY += spacingSmallY;
            MainMenuItems.Add(new Button("- Back to main menu", leftX + indentX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                PageState mmp = new MainMenuPage();
                mmp.init(game);
                game.states.Clear();
                game.states.Add(mmp);
            }));
            topY += spacingY;
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text("Levels:", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 64f, SKTextAlign.Left));
            topY += 0;
            int oldX = leftX + 180;
            int pagesLeftXSpacing = 48;
            // Pages
            var paginationItems = GetPaginationItems(this.CurrentPage + 1, this.TotalPages); // +1 if CurrentPage is 0-indexed
            int xOffset = 0;

            foreach (var item in paginationItems)
            {
                if (item == "...")
                {
                    // Add ellipsis (non-clickable)
                    MainMenuItems.Add(new ButtonWithCircle("...", oldX + xOffset, topY + 8, SKColors.Gray,
                        SKColors.Transparent, menuTypeface, menuFont, 48f, SKTextAlign.Left,
                        (g) => {
                        }, false, true));
                }
                else
                {
                    int pageNum = int.Parse(item);
                    MainMenuItems.Add(new ButtonWithCircle(item, oldX + xOffset, topY + 8, SKColors.Black,
                        SKColor.Parse("#58aff3"), menuTypeface, menuFont, 48f, SKTextAlign.Left,
                        (g) => {
                            // Handle page click
                            this.CurrentPage = pageNum - 1;
                            this.needsUIRefresh = true;
                        }, this.CurrentPage == pageNum - 1));
                }

                xOffset += pagesLeftXSpacing;
            }
            MainMenuItems.Add(new PaperTanksV2Client.UI.Text(
                "- pages", 
                xOffset + ((pagesLeftXSpacing * (paginationItems.Count))),
                topY + 8,
                SKColors.Red,
                menuTypeface,
                menuFont,
                42f,
                SKTextAlign.Left));
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
            for (int i=0;i<10;i++) {
                topY += spacingSmallY;
                MainMenuItems.Add(new Button("Demo Level", leftX + indentX, topY, SKColors.Black, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 32f, SKTextAlign.Left, (g) => {
                }));
            }
        }
    }
}