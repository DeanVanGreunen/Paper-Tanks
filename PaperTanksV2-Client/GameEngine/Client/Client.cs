using PaperTanksV2Client.GameEngine.Server;
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
            this.tcpClient.OnMessageReceived += OnMessageReceive;
            this.tcpClient.OnDisconnected += OnDisconnection;
        }
        
        public void OnConnection(Socket socket)
        {
            Console.WriteLine("Client Connected");
        }
        
        public void OnMessageReceive(Socket socket, BinaryMessage message)
        {
            Console.WriteLine("Client Received");
        }

        public void OnDisconnection(Socket socket)
        {
            Console.WriteLine("Client Disconnected");
        }
        
        public event Action<Socket> OnConnected
        {
            add => this.tcpClient.OnConnected += value;
            remove => this.tcpClient.OnConnected -= value;
        }
        
        public event Action<Socket> OnDisconnected
        {
            add => this.tcpClient.OnDisconnected += value;
            remove => this.tcpClient.OnDisconnected -= value;
        }
        
        public event Action<Socket, BinaryMessage> OnMessageReceived
        {
            add => this.tcpClient.OnMessageReceived += value;
            remove => this.tcpClient.OnMessageReceived -= value;
        }
    }
}