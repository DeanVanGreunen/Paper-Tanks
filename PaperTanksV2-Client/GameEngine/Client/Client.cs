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
        private Dictionary<Guid, ClientConnection> clientConnections = new Dictionary<Guid, ClientConnection>();
        private TCPClient tcpClient;
        private ServerGameMode gMode = ServerGameMode.Lobby;
        private string _ipAddress = "";
        private short _port = 0;
        
        public ServerGameMode GetGameMode => this.gMode;


        public void SetGMode(ServerGameMode value)
        {
            this.gMode = value;
        }

        public string GetIPAddress => $"{this._ipAddress}:{this._port}";
        
        public Dictionary<Guid, ClientConnection> ClientConnections => this.clientConnections;
        
        
        
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
            try {
                if (message == null) {
                    Console.WriteLine("[Client] Received null message");
                    return;
                }

                Console.WriteLine(
                    $"[Client] Received message type: {message.DataHeader.dataType}, buffer length: {message.DataHeader.buffer?.Length ?? 0}");

                if (message.DataHeader.dataType == DataType.Users) {
                    Console.WriteLine("[Client] Processing Users message...");

                    if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length == 0) {
                        Console.WriteLine("[Client] ERROR: Empty Users buffer");
                        return;
                    }

                    ClientConnection[] _clientConnections =
                        BinaryHelper.ToClientConnectionArrayBigEndian(message.DataHeader.buffer, 0);
                    Console.WriteLine($"[Client] Deserialized {_clientConnections?.Length ?? 0} client connections");

                    this.ClientConnections.Clear();
                    foreach (var cc in _clientConnections) {
                        if (cc != null && cc.Id != Guid.Empty) {
                            this.ClientConnections.Add(cc.Id, cc);
                        }
                    }

                    Console.WriteLine($"[Client] Client connections updated: {this.ClientConnections.Count} clients");
                    return;
                } else if (message.DataHeader.dataType == DataType.GameMode) {
                    Console.WriteLine("[Client] Processing GameMode message...");

                    if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length < 4) {
                        Console.WriteLine(
                            $"[Client] ERROR: Invalid GameMode buffer length: {message.DataHeader.buffer?.Length ?? 0}");
                        return;
                    }

                    ServerGameMode gMode = (ServerGameMode) BinaryHelper.ToInt32BigEndian(message.DataHeader.buffer, 0);
                    this.gMode = gMode;
                    Console.WriteLine($"[Client] Game mode changed to: {gMode}");
                    return;
                } else if (message.DataHeader.dataType == DataType.GameObjects) {
                    Console.WriteLine("[Client] Processing GameObjects message...");

                    if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length == 0) {
                        Console.WriteLine("[Client] ERROR: Empty GameObjects buffer");
                        return;
                    }

                    GameObjectArray gameObjectsList = BinaryHelper.ToGameObjectArray(message.DataHeader.buffer);

                    if (gameObjectsList?.gameObjectsData == null) {
                        Console.WriteLine("[Client] ERROR: Failed to deserialize game objects");
                        return;
                    }

                    Console.WriteLine($"[Client] Deserialized {gameObjectsList.gameObjectsData.Count} game objects");

                    // Clear and repopulate
                    this._gameObjects.Clear();

                    foreach (GameObject gobj in gameObjectsList.gameObjectsData) {
                        if (gobj != null && gobj.Id != Guid.Empty) {
                            this._gameObjects[gobj.Id] = gobj;
                        }
                    }

                    Console.WriteLine($"[Client] Game objects updated: {this._gameObjects.Count} objects");
                    return;
                } else if (message.DataHeader.dataType == DataType.HeartBeat) {
                    // Silent heartbeat
                    return;
                } else {
                    Console.WriteLine($"[Client] Unknown message type: {message.DataHeader.dataType}");
                }
            } catch (Exception ex) {
                Console.WriteLine("=== [Client] ERROR IN OnMessageReceive ===");
                Console.WriteLine($"Message Type: {message?.DataHeader.dataType}");
                Console.WriteLine($"Buffer Length: {message?.DataHeader.buffer?.Length ?? -1}");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null) {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                Console.WriteLine("==========================================");
            }
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