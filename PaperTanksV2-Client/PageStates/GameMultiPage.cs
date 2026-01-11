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
            menuTypeface =
                SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font, "QuickPencil-Regular.ttf"));
            menuFont = new SKFont(menuTypeface, 72);
            bool loaded3 = game.resources.Load(ResourceManagerFormat.Font, "Aaa-Prachid-Hand-Written.ttf");
            if (!loaded3) throw new Exception("Error Loading Menu Font");
            secondMenuTypeface =
                SKTypeface.FromData((SKData) game.resources.Get(ResourceManagerFormat.Font,
                    "Aaa-Prachid-Hand-Written.ttf"));
            secondMenuFont = new SKFont(menuTypeface, 72);
        }

        public bool Connect(string ipAddress, short port)
        {
            this.client = new Client(ipAddress, port);

            this.client.OnConnected += socket => {
                Console.WriteLine("Connected to server!");
            };

            this.client.OnMessageReceived += (socket, message) => {
                try {
                    if (message == null) return;

                    Console.WriteLine($"[GameMultiPage] Received: {message.DataHeader.dataType}");

                    if (message.DataHeader.dataType == DataType.GameMode) {
                        ServerGameMode mode =
                            (ServerGameMode) BinaryHelper.ToInt32BigEndian(message.DataHeader.buffer, 0);
                        this.client.SetGMode(mode);
                        Console.WriteLine($"Game mode updated to: {mode}");
                    }

                    if (message.DataHeader.dataType == DataType.GameObjects) {
                        try {
                            Console.WriteLine(
                                $"Processing game objects, buffer size: {message.DataHeader.buffer?.Length ?? 0}");

                            if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length == 0) {
                                Console.WriteLine("Empty game objects buffer");
                                return;
                            }

                            GameObjectArray gameObjectsList = BinaryHelper.ToGameObjectArray(message.DataHeader.buffer);

                            if (gameObjectsList?.gameObjectsData == null) {
                                Console.WriteLine("Failed to deserialize game objects");
                                return;
                            }

                            Console.WriteLine($"Received {gameObjectsList.gameObjectsData.Count} game objects");

                            // IMPORTANT: Clear and update the GameMultiPage's _gameObjects
                            this._gameObjects.Clear();

                            foreach (GameObject gobj in gameObjectsList.gameObjectsData) {
                                if (gobj != null && gobj.Id != Guid.Empty) {
                                    this._gameObjects[gobj.Id] = gobj;

                                    // Log what type we got
                                    string typeName = gobj.GetType().Name;
                                    Console.WriteLine(
                                        $"Added {typeName}: ID={gobj.Id}, Pos=({gobj.Position.X}, {gobj.Position.Y})");
                                }
                            }

                            Console.WriteLine($"Total game objects in GameMultiPage: {this._gameObjects.Count}");
                        } catch (Exception ex) {
                            Console.WriteLine($"Error processing game objects: {ex.Message}");
                            Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        }
                    }

                    if (message.DataHeader.dataType == DataType.Users) {
                        Console.WriteLine($"Received users update");
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"[GameMultiPage] Error in message handler: {ex.Message}");
                }
            };

            this.client.OnDisconnected += socket => {
                Console.WriteLine("Disconnected from server!");
            };

            if (!this.client.Connect()) {
                Console.WriteLine($"Client unable to connect to {ipAddress}:{port}");
                return false;
            }

            Console.WriteLine($"Successfully connected to {ipAddress}:{port}");
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
                // Lobby UI
                int topY = 128;
                int spacingY = 62;
                int leftX = 48;
                new PaperTanksV2Client.UI.Text($"Multiplayer - Lobby", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 42f, SKTextAlign.Left).Render(game, canvas);
                topY += spacingY;
                foreach (var obj in this.client.ClientConnections) {
                    new PaperTanksV2Client.UI.Text($"{obj.Value.Id}", leftX, topY, SKColor.Parse("#58aff3"), menuTypeface, menuFont, 22f, SKTextAlign.Left).Render(game, canvas);
                    topY += spacingY;
                }
            } 
            else if (this.client.GetGameMode == ServerGameMode.GamePlay) {
                Console.WriteLine($"Rendering {this._gameObjects.Count} game objects");
        
                foreach (var obj in this._gameObjects) {
                    if (obj.Value != null) {
                        string typeName = obj.Value.GetType().Name;
                        Console.WriteLine($"Rendering {typeName} at ({obj.Value.Position.X}, {obj.Value.Position.Y})");
                
                        // Use InternalRender which handles rotation
                        obj.Value.InternalRender(game, canvas);
                    }
                }
            } 
            else if (this.client.GetGameMode == ServerGameMode.GameOverWin) {
                // Win screen
            } 
            else if (this.client.GetGameMode == ServerGameMode.GameOverLose) {
                // Lose screen
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