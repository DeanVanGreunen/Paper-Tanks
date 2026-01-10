using Gtk;
using PaperTanksV2Client.GameEngine.Server;
using PaperTanksV2Client.GameEngine.Server.Data;
using System;
using System.Collections.Generic;
using Socket = System.Net.Sockets.Socket;

namespace PaperTanksV2Client.GameEngine.Client
{
    public class Client
    {
        private Dictionary<Guid, GameObject> _gameObjects;
        private TCPClient tcpClient;
        private ServerGameMode gMode = ServerGameMode.Lobby;
        private string _ipAddress = "";
        private short _port = 0;
        
        public ServerGameMode GetGameMode => this.gMode;
        public string GetIPAddress => $"{this._ipAddress}:{this._port}";
        
        public Client(string IPAddress, short Port)
        {
            this._ipAddress = IPAddress;
            this._port = Port;
            this._gameObjects = new Dictionary<Guid, GameObject>();
            this.tcpClient = new TCPClient(IPAddress, Port);
            this.tcpClient.OnConnected += OnConnection;
            this.tcpClient.OnMessageReceived += OnMessageReceive;
            this.tcpClient.OnDisconnected += OnDisconnection;
        }

        public bool Connect()
        {
            return this.tcpClient.Connect();
        }

        public void OnConnection(Socket socket)
        {
            Console.WriteLine("Client Connected");
        }
        
        public void OnMessageReceive(Socket socket, BinaryMessage message)
        {
            if (message.DataHeader.dataType == DataType.GameMode) {
                this.gMode = (ServerGameMode)BitConverter.ToInt32(message.DataHeader.buffer, 0);
            }
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

        public void SendMessage(BinaryMessage message)
        {
            this.tcpClient.Send(message);
        }
    }
}