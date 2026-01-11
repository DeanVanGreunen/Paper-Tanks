using Gtk;
using PaperTanksV2Client.GameEngine.data;
using PaperTanksV2Client.GameEngine.Server;
using PaperTanksV2Client.GameEngine.Server.Data;
using SkiaSharp;
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
        public string _Id = "";

        // Add UI element storage
        private SKTypeface menuTypeface = null;
        private SKFont menuFont = null;
        private SKTypeface secondMenuTypeface = null;
        private SKFont secondMenuFont = null;
        private Action<Game> playerDiedCallback = null;

        public ServerGameMode GetGameMode => this.gMode;
        public Dictionary<Guid, GameObject> GameObjects => _gameObjects;

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

        // Add method to set UI elements
        public void SetUIElements(SKTypeface menuTypeface, SKFont menuFont,
            SKTypeface secondMenuTypeface, SKFont secondMenuFont,
            Action<Game> playerDiedCallback = null)
        {
            this.menuTypeface = menuTypeface;
            this.menuFont = menuFont;
            this.secondMenuTypeface = secondMenuTypeface;
            this.secondMenuFont = secondMenuFont;
            this.playerDiedCallback = playerDiedCallback;
        }

        public bool Connect()
        {
            return this.tcpClient.Connect();
        }

        public void OnConnection(Socket socket)
        {
            if (TextData.DEBUG_MODE == true) Console.WriteLine("Client Connected");
        }

        public void OnMessageReceive(Socket socket, BinaryMessage message)
        {
            try {
                if (message == null) {
                    if (TextData.DEBUG_MODE == true) Console.WriteLine("[Client] Received null message");
                    return;
                }

                if (TextData.DEBUG_MODE == true)
                    Console.WriteLine(
                        $"[Client] Received message type: {message.DataHeader.dataType}, buffer length: {message.DataHeader.buffer?.Length ?? 0}");

                if (message.DataHeader.dataType == DataType.Users) {
                    if (TextData.DEBUG_MODE == true) Console.WriteLine("[Client] Processing Users message...");

                    if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length == 0) {
                        if (TextData.DEBUG_MODE == true) Console.WriteLine("[Client] ERROR: Empty Users buffer");
                        return;
                    }

                    ClientConnection[] _clientConnections =
                        BinaryHelper.ToClientConnectionArrayBigEndian(message.DataHeader.buffer, 0);
                    if (TextData.DEBUG_MODE == true)
                        Console.WriteLine(
                            $"[Client] Deserialized {_clientConnections?.Length ?? 0} client connections");

                    this.ClientConnections.Clear();
                    foreach (var cc in _clientConnections) {
                        if (cc != null && cc.Id != Guid.Empty) {
                            this.ClientConnections.Add(cc.Id, cc);
                        }
                    }

                    if (TextData.DEBUG_MODE == true)
                        Console.WriteLine(
                            $"[Client] Client connections updated: {this.ClientConnections.Count} clients");
                    return;
                } else if (message.DataHeader.dataType == DataType.GameMode) {
                    if (TextData.DEBUG_MODE == true) Console.WriteLine("[Client] Processing GameMode message...");

                    if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length < 4) {
                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine(
                                $"[Client] ERROR: Invalid GameMode buffer length: {message.DataHeader.buffer?.Length ?? 0}");
                        return;
                    }

                    ServerGameMode gMode = (ServerGameMode) BinaryHelper.ToInt32BigEndian(message.DataHeader.buffer, 0);
                    this.gMode = gMode;
                    if (TextData.DEBUG_MODE == true) Console.WriteLine($"[Client] Game mode changed to: {gMode}");
                    return;
                } else if (message.DataHeader.dataType == DataType.GameObjects) {
                    if (TextData.DEBUG_MODE == true) Console.WriteLine("[Client] Processing GameObjects message...");

                    if (message.DataHeader.buffer == null || message.DataHeader.buffer.Length == 0) {
                        if (TextData.DEBUG_MODE == true) Console.WriteLine("[Client] ERROR: Empty GameObjects buffer");
                        return;
                    }

                    GameObjectArray gameObjectsList = BinaryHelper.ToGameObjectArray(message.DataHeader.buffer);

                    if (gameObjectsList?.gameObjectsData == null) {
                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine("[Client] ERROR: Failed to deserialize game objects");
                        return;
                    }

                    if (TextData.DEBUG_MODE == true)
                        Console.WriteLine(
                            $"[Client] Deserialized {gameObjectsList.gameObjectsData.Count} game objects");

                    // Clear and repopulate
                    this._gameObjects.Clear();

                    foreach (GameObject gobj in gameObjectsList.gameObjectsData) {
                        if (gobj != null && gobj.Id != Guid.Empty) {
                            // Initialize UI elements based on object type
                            InitializeUIElements(gobj);

                            this._gameObjects[gobj.Id] = gobj;
                            if (TextData.DEBUG_MODE == true)
                                Console.WriteLine(
                                    $"[Client] Received {gobj.GetType().Name}: ID={gobj.Id}, Pos=({gobj.Position.X}, {gobj.Position.Y}), Size=({gobj.Size.X}, {gobj.Size.Y})");
                        }
                    }

                    if (TextData.DEBUG_MODE == true)
                        Console.WriteLine($"[Client] Game objects updated: {this._gameObjects.Count} objects");
                    return;
                } else if (message.DataHeader.dataType == DataType.HeartBeat) {
                    // Silent heartbeat
                    return;
                } else {
                    if (TextData.DEBUG_MODE == true)
                        Console.WriteLine($"[Client] Unknown message type: {message.DataHeader.dataType}");
                }
            } catch (Exception ex) {
                if (TextData.DEBUG_MODE == true) Console.WriteLine("=== [Client] ERROR IN OnMessageReceive ===");
                if (TextData.DEBUG_MODE == true) Console.WriteLine($"Message Type: {message?.DataHeader.dataType}");
                if (TextData.DEBUG_MODE == true)
                    Console.WriteLine($"Buffer Length: {message?.DataHeader.buffer?.Length ?? -1}");
                if (TextData.DEBUG_MODE == true) Console.WriteLine($"Error: {ex.Message}");
                if (TextData.DEBUG_MODE == true) Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null) {
                    if (TextData.DEBUG_MODE == true) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                if (TextData.DEBUG_MODE == true) Console.WriteLine("==========================================");
            }
        }

        // Add helper method to initialize UI elements
        private void InitializeUIElements(GameObject gobj)
        {
            if (gobj is Tank tank) {
                tank.SetUIElements(menuTypeface, menuFont, secondMenuTypeface, secondMenuFont, playerDiedCallback);
            } else if (gobj is AmmoPickup ammoPickup) {
                ammoPickup.SetUIElements(menuTypeface, menuFont, secondMenuTypeface, secondMenuFont);
            } else if (gobj is HealthPickup healthPickup) {
                healthPickup.SetUIElements(menuTypeface, menuFont, secondMenuTypeface, secondMenuFont);
            }
            // Projectile and Wall don't need UI elements
        }

        public void OnDisconnection(Socket socket)
        {
            if (TextData.DEBUG_MODE == true) Console.WriteLine("Client Disconnected");
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