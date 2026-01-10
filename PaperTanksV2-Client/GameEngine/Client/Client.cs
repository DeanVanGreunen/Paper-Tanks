using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace PaperTanksV2Client.GameEngine.Client
{
    public class Client
    {
        private Dictionary<Guid, GameObject> _gameObjects;
        private TCPClient tcpClient;
        public Client(string IPAddress, short Port)
        {
            this._gameObjects = new Dictionary<Guid, GameObject>();
            this.tcpClient = new TCPClient(IPAddress, Port);

            this.tcpClient.OnConnected += OnConnection;
            this.tcpClient.OnDisconnected += OnDisconnection;
        }
        public void OnConnection(Socket socket)
        {
            Console.WriteLine("Client Connected");
        }

        public void OnDisconnection(Socket socket)
        {
            Console.WriteLine("Client Disconnected");
        }
    }
}