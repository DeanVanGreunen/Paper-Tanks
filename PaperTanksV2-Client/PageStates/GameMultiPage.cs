using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.GameEngine.Client;
using PaperTanksV2Client.GameEngine.data;
using PaperTanksV2Client.GameEngine.Server;
using PaperTanksV2Client.GameEngine.Server.Data;
using SFML.Graphics;
using SFML.Window;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace PaperTanksV2Client.PageStates
{
    public class GameMultiPage : PageState, IDisposable
    {
        private bool DEBUG_MODE = true;
        private Client client = null;
        ViewPort viewPort;
        PaperPageRenderer paperRenderer;
        private Dictionary<Guid, GameObject> _gameObjects = new Dictionary<Guid, GameObject>();
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKTypeface secondMenuTypeface = null;
        private SKFont secondMenuFont = null;
        public void Dispose()
        {
        }

        public GameMultiPage()
        {
            Vector2Data viewSize = new Vector2Data(
                1920, 
                1080
            );
            this.viewPort = new ViewPort(viewSize);
            this.paperRenderer = new PaperPageRenderer(
                pageWidth: 1920,
                pageHeight: 1080,
                spacing: 20,
                totalLines: 60
            );
        }

        public void init(Game game)
        {
            bool loaded2 = game.resources.Load(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf");
            if (!loaded2) throw new Exception("Error Loading Menu Font");
            menuTypeface = SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf"));
            menuFont = new SKFont(menuTypeface, 72);
            bool loaded3 = game.resources.Load(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf");
            if (!loaded3) throw new Exception("Error Loading Menu Font");
            secondMenuTypeface = SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf"));
            secondMenuFont = new SKFont(menuTypeface, 72);
        }

        public bool Connect(string ipAddress, short port)
        {
            this.client = new Client(ipAddress, port);

            this.client.OnConnected += socket => {
                if(TextData.DEBUG_MODE == true) Console.WriteLine("Connected to server!");
            };

            this.client.OnMessageReceived += (socket, message) => {
                try {
                    if (message == null) return;

                    if(TextData.DEBUG_MODE == true) Console.WriteLine($"[GameMultiPage] Received: {message.DataHeader.dataType}");

                    if (message.DataHeader.dataType == DataType.GameMode) {
                        // IMPORTANT: Use Big Endian conversion to match the server
                        ServerGameMode mode =
                            (ServerGameMode) BinaryHelper.ToInt32BigEndian(message.DataHeader.buffer, 0);
                        this.client.SetGMode(mode);
                        if(TextData.DEBUG_MODE == true) Console.WriteLine($"Game mode updated to: {mode}");
                    }

                    if (message.DataHeader.dataType == DataType.GameObjects) {
                        try {
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(
                                $"Processing game objects, buffer size: {message.DataHeader.buffer?.Length ?? 0}");

                            if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length == 0) {
                                if(TextData.DEBUG_MODE == true) Console.WriteLine("Empty game objects buffer");
                                return;
                            }

                            GameObjectArray gameObjectsList = BinaryHelper.ToGameObjectArray(message.DataHeader.buffer);

                            if (gameObjectsList?.gameObjectsData == null) {
                                if(TextData.DEBUG_MODE == true) Console.WriteLine("Failed to deserialize game objects");
                                return;
                            }

                            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Received {gameObjectsList.gameObjectsData.Count} game objects");

                            // Clear and update the game objects
                            this._gameObjects.Clear();

                            foreach (GameObject gobj in gameObjectsList.gameObjectsData) {
                                if (gobj != null && gobj.Id != Guid.Empty) {
                                    this._gameObjects[gobj.Id] = gobj;
                                    if(TextData.DEBUG_MODE == true) Console.WriteLine(
                                        $"Added: {gobj.GetType().Name} at ({gobj.Position.X}, {gobj.Position.Y})");
                                }
                            }

                            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Total game objects: {this._gameObjects.Count}");
                        } catch (Exception ex) {
                            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Error processing game objects: {ex.Message}");
                            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        }
                    }

                    if (message.DataHeader.dataType == DataType.Users) {
                        if(TextData.DEBUG_MODE == true) Console.WriteLine($"Received users update: {this.client.ClientConnections.Count} clients");
                    }
                } catch (Exception ex) {
                    if(TextData.DEBUG_MODE == true) Console.WriteLine($"[GameMultiPage] Error in message handler: {ex.Message}");
                    if(TextData.DEBUG_MODE == true) Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            };

            this.client.OnDisconnected += socket => {
                if(TextData.DEBUG_MODE == true) Console.WriteLine("Disconnected from server!");
            };

            if (!this.client.Connect()) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Client unable to connect to {ipAddress}:{port}");
                return false;
            }

            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Successfully connected to {ipAddress}:{port}");
            return true;
        }

        public void input(Game game)
        {
            if (game.keyboard.IsKeyPressed(Keyboard.Key.Left) ||
                game.keyboard.IsKeyPressed(Keyboard.Key.Right) ||
                game.keyboard.IsKeyPressed(Keyboard.Key.Up) ||
                game.keyboard.IsKeyPressed(Keyboard.Key.Down)
               ) {
                MovementCommand mc = null;
                if (game.keyboard.IsKeyPressed(Keyboard.Key.Left)) {
                    mc = new MovementCommand();
                    mc.MovementData = new Movement(PlayerInput.MOVE_LEFT);
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Right)) {
                    mc = new MovementCommand();
                    mc.MovementData = new Movement(PlayerInput.MOVE_RIGHT);
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Up)) {
                    mc = new MovementCommand();
                    mc.MovementData = new Movement(PlayerInput.MOVE_UP);
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Down)) {
                    mc = new MovementCommand();
                    mc.MovementData = new Movement(PlayerInput.MOVE_DOWN);
                }
                if (mc != null) {
                    byte[] bytes = mc.MovementData.ToBytes();
                    BinaryMessage m = new BinaryMessage(new DataHeader(DataType.Movement, bytes.Length, bytes));
                    this.client.SendMessage(m);
                }
            }
            if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Space)) {
                BinaryMessage m = new BinaryMessage(new DataHeader(DataType.Fire, 0, Array.Empty<byte>()));
                this.client.SendMessage(m);
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
            if (this.client.GetGameMode == ServerGameMode.Lobby) {
                // Setup Main Menu Items
                int topY = 128;
                int spacingY = 62;
                int spacingYSmall = 32;
                int xIndent = 62;
                int leftX = 48;
                new PaperTanksV2Client.UI.Text($"Multiplayer - Lobby", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 42f, SKTextAlign.Left).Render(game, canvas);
                topY += spacingY;
                foreach (var obj in this.client.ClientConnections) {
                    new PaperTanksV2Client.UI.Text($"{obj.Value.Id}", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 22f, SKTextAlign.Left).Render(game, canvas);
                    topY += spacingY;
                }
            } else if (this.client.GetGameMode == ServerGameMode.GamePlay) {
                foreach (var obj in this.client.GameObjects) {
                    obj.Value.Render(game, canvas);
                }
            } else if (this.client.GetGameMode == ServerGameMode.GameOverWin) {
                
            } else if (this.client.GetGameMode == ServerGameMode.GameOverLose) {
                
            }
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
            if (DEBUG_MODE) {
                using (var debugPaint = new SKPaint()) {
                    debugPaint.Color = SKColors.Red;
                    debugPaint.TextSize = 16;
                    debugPaint.IsAntialias = true;
                    canvas.DrawText($"Server: {this.client.GetIPAddress}", 10, 40, debugPaint);
                    canvas.DrawText($"Mode: {this.client.GetGameMode}", 10, 55, debugPaint);
                    canvas.DrawText(
                        $"Endian: {( BitConverter.IsLittleEndian == true ? "Little Endian" : "Big Endian" )}", 10, 70,
                        debugPaint);
                    canvas.DrawText(
                        $"Total Game Objects: {this._gameObjects.Count}", 10, 85,
                        debugPaint);
                }
            }
        }
    }
}