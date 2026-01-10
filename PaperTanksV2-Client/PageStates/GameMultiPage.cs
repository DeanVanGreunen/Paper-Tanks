using PaperTanksV2Client.GameEngine.Client;
using SFML.Graphics;
using SkiaSharp;
using System;
using System.Net.Sockets;

namespace PaperTanksV2Client.PageStates
{
    public class GameMultiPage : PageState, IDisposable
    {
        private Client client = null;
        
        public void Dispose()
        {
        }

        public void init(Game game)
        {
        }

        public void Connect(string ipAddress, short port)
        {
            this.client = new Client(ipAddress, port);
            this.client.OnConnected += socket => {
                
            };
            this.client.OnMessageReceived += (socket, message) => {
                
            };
            this.client.OnDisconnected += socket => {
                
            };
        }

        public void input(Game game)
        {
        }

        public void update(Game game, float deltaTime)
        {
        }

        public void prerender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void render(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }

        public void postrender(Game game, SKCanvas canvas, RenderStates renderStates)
        {
        }
    }
}