using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.GameEngine.Client;
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
        private Client client = null;
        
        ViewPort viewPort;
        PaperPageRenderer paperRenderer;
        private Dictionary<Guid, GameObject> _gameObjects = new Dictionary<Guid, GameObject>();

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
        }

        public bool Connect(string ipAddress, short port)
        {
            this.client = new Client(ipAddress, port);
            this.client.OnConnected += socket => {

            };
            this.client.OnMessageReceived += (socket, message) => {

            };
            this.client.OnDisconnected += socket => {

            };
            if (!this.client.Connect()) {
                Console.WriteLine($"Client unable to connect to {ipAddress}:{port}");
                return false;
            }
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
            Dictionary<Guid, GameObject> gobjs = _gameObjects;
            viewPort.Render(game, canvas, gobjs);
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }
    }
}