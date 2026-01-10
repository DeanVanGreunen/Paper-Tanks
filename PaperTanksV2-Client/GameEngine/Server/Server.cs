using Gtk;
using PaperTanksV2Client.GameEngine.Server.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Socket = System.Net.Sockets.Socket;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class Server : ServerRunner, IDisposable
    {
        private Dictionary<Guid, GameObject> _gameObjects;
        private TCPServer tcpServer;
        private short Port;
        private Task _serverTask;
        private ServerGameMode gMode = ServerGameMode.Lobby;
        private float _timeSinceLastHeartbeat = 0f;
        private float _timeSinceLastBroadcast = 0f;
        private const float HEARTBEAT_INTERVAL = 1f;
        private const float BROADCAST_INTERVAL = 2f;
        private float _lobbyCountdown = 0f;
        private bool _isCountdownActive = false;
        private const float LOBBY_COUNTDOWN_DURATION = 5f;
        private float movementSpeed = 100;
        private Queue<MovementCommand> _movementQueue = new Queue<MovementCommand>();
        private Queue<FireCommand> _fireQueue = new Queue<FireCommand>();
        private Queue<GameObject> _objectsToAdd = new Queue<GameObject>();
        private Level _level = null;
        public bool isRunning = false;
        public const int TARGET_FPS = 60;
        public const float FRAME_TIME = 1.0f / TARGET_FPS;
        private double currentFps;
        
        public Server(short Port)
        {
            this.Port = Port;
            this.tcpServer = new TCPServer(this.Port);
            this.tcpServer.OnConnection += this.OnConnection;
            this.tcpServer.OnDisconnection += this.OnDisconnection;
            this.tcpServer.OnMessageReceived += this.OnMessageReceived;
        }

        public void OnConnection(Socket socket)
        {
            // DEBUG: SHOW IP AND PORT
        }

        public void OnDisconnection(Socket socket)
        {
            // DEBUG: SHOW IP AND PORT
        }

        public void OnMessageReceived(Socket socket, BinaryMessage message)
        {
            if (message == null) return; // Discard Message if null
            if (message.DataHeader.dataType == DataType.HeartBeat) {
                _ = this.tcpServer.SendAsync(socket, BinaryMessage.HeartBeatMessage);
                return;
            }
            if (this.gMode == ServerGameMode.Lobby) {
                this.tcpServer.SetClient(socket, (ClientConnection c) => {
                    c.isReady = true;
                    this._gameObjects.Add(c.Id, new Tank(true, new Weapon(10, 100), null, null, null, null, null, null, null));
                });
            } else if (this.gMode == ServerGameMode.GamePlay) {
                switch (message.DataHeader.dataType) {
                    case DataType.HeartBeat:
                        _ = this.tcpServer.SendAsync(socket, BinaryMessage.HeartBeatMessage);
                        break;
                    case DataType.Movement:
                        ClientConnection client = this.tcpServer.GetBySocket(socket);
                        if (client != null && this._gameObjects.ContainsKey(client.Id)) {
                            Movement movementData = Movement.FromBytes(message.DataHeader.buffer);
                            _movementQueue.Enqueue(new MovementCommand {
                                ClientId = client.Id,
                                MovementData = movementData
                            });
                        }
                        break;
                    case DataType.Fire:
                        ClientConnection client1 = this.tcpServer.GetBySocket(socket);
                        if (client1 != null && this._gameObjects.ContainsKey(client1.Id)) {
                            _fireQueue.Enqueue(new FireCommand {
                                ClientId = client1.Id
                            });
                        }
                        break;
                }
            }
        }

        public void Init()
        {
            _serverTask = this.tcpServer.StartAsync();
        }

        public void Start()
        {
            this.gMode = ServerGameMode.Lobby;
            this.LoadRandomLevel();
        }

        public void LoadRandomLevel()
        {
            List<string> levels = MultiplayerManager.GetMultiPlayerList();
            string levelName = levels[new Random().Next(levels.Count)];
            this._level = MultiplayerManager.LoadLevel(levelName);
        }

        public void Update(float deltaTime)
        {
            // Accumulate time
            _timeSinceLastHeartbeat += deltaTime;
            _timeSinceLastBroadcast += deltaTime;

            // Check if we should switch from Lobby to GamePlay mode
            if (this.gMode == ServerGameMode.Lobby) {
                ClientConnection[] clients = this.tcpServer.GetAllClients().ToArray();

                if (clients.Length >= 4) {
                    if (!_isCountdownActive) {
                        // Start the countdown
                        _isCountdownActive = true;
                        _lobbyCountdown = 0f;
                    } else {
                        // Continue countdown
                        _lobbyCountdown += deltaTime;

                        if (_lobbyCountdown >= LOBBY_COUNTDOWN_DURATION) {
                            // Countdown complete, switch to GamePlay
                            this.gMode = ServerGameMode.GamePlay;
                            _isCountdownActive = false;
                            _lobbyCountdown = 0f;
                        }
                    }
                } else {
                    // Less than 4 clients, reset countdown
                    _isCountdownActive = false;
                    _lobbyCountdown = 0f;
                }
            }

            // Process movement queue
            if (this.gMode == ServerGameMode.GamePlay) {
                while (_movementQueue.Count > 0) {
                    MovementCommand cmd = _movementQueue.Dequeue();
                    if (this._gameObjects.ContainsKey(cmd.ClientId)) {
                        GameObject gameObject = this._gameObjects[cmd.ClientId];
                        switch (cmd.MovementData.input) {
                            case PlayerInput.MOVE_LEFT:
                                gameObject.MoveBy(-movementSpeed * deltaTime, 0);
                                gameObject.Rotation = -180;
                                break;
                            case PlayerInput.MOVE_RIGHT:
                                gameObject.MoveBy(movementSpeed * deltaTime, 0);
                                gameObject.Rotation = 0;
                                break;
                            case PlayerInput.MOVE_UP:
                                gameObject.MoveBy(0, -movementSpeed * deltaTime);
                                gameObject.Rotation = -90;
                                break;
                            case PlayerInput.MOVE_DOWN:
                                gameObject.MoveBy(0, movementSpeed * deltaTime);
                                gameObject.Rotation = 90;
                                break;
                        }
                    }
                }
                while (_objectsToAdd.Count > 0)
                {
                    GameObject obj = _objectsToAdd.Dequeue();
                    Guid guid = Guid.NewGuid();
                    _gameObjects.Add(guid, obj);
                    obj.Id = guid;
                }
                while (_fireQueue.Count > 0) {
                    FireCommand cmd = _fireQueue.Dequeue();
                    if (this._gameObjects.ContainsKey(cmd.ClientId)) {
                        GameObject player = this._gameObjects[cmd.ClientId];
                        if(( player as Tank ).Weapon0.AmmoCount >= 1) {
                            Projectile projectile = new Projectile(SKColors.Red, player.Id);
                            Vector2Data size = new Vector2Data(8, 8);
                            if (player.Rotation == 0) {
                                projectile.Bounds =
                                    new BoundsData(
                                        new Vector2Data(player.Position.X + 100,
                                            player.Position.Y + ( player.Size.Y / 2 ) - ( size.Y / 2 )), size);
                                projectile.Velocity = new Vector2Data(this.movementSpeed, 0);
                            } else if (player.Rotation == -180) {
                                projectile.Bounds =
                                    new BoundsData(
                                        new Vector2Data(player.Position.X - 58,
                                            player.Position.Y + ( player.Size.Y / 2 ) - ( size.Y / 2 )), size);
                                projectile.Velocity = new Vector2Data(-this.movementSpeed, 0);
                            } else if (player.Rotation == -90) {
                                projectile.Bounds =
                                    new BoundsData(
                                        new Vector2Data(player.Position.X + ( player.Size.X / 2 ) - ( size.X / 2 ),
                                            player.Position.Y - 58), size);
                                projectile.Velocity = new Vector2Data(0, -this.movementSpeed);
                            } else if (player.Rotation == 90) {
                                projectile.Bounds =
                                    new BoundsData(
                                        new Vector2Data(player.Position.X + ( player.Size.X / 2 ) - ( size.X / 2 ),
                                            player.Position.Y + 100), size);
                                projectile.Velocity = new Vector2Data(0, this.movementSpeed);
                            }
                            ( player as Tank ).Weapon0.AmmoCount -= 1;
                            this.QueueAddObject(projectile);
                        }
                    }
                }
            }
            if (_timeSinceLastHeartbeat >= HEARTBEAT_INTERVAL) {
                this.tcpServer.SendBroadcastMessage(BinaryMessage.HeartBeatMessage);
                _timeSinceLastHeartbeat = 0f;
            }
            if (this.gMode == ServerGameMode.Lobby) {
                if (_timeSinceLastBroadcast >= BROADCAST_INTERVAL) {
                    ClientConnection[] clients = this.tcpServer.GetAllClients().ToArray();
                    Users users = new Users {
                        usersData = clients.Select(c => new UsersData { guid = c.Id }).ToList()
                    };
                    byte[] usersData = users.GetBytes();
                    DataHeader usersDataHeader = new DataHeader(
                        DataType.Users,
                        usersData.Length,
                        usersData
                    );
                    BinaryMessage usersBinaryMessage = new BinaryMessage(usersDataHeader);
                    this.tcpServer.SendBroadcastMessage(usersBinaryMessage);
                    _timeSinceLastBroadcast = 0f;
                }
            } else if (this.gMode == ServerGameMode.GamePlay) {
                if (_timeSinceLastBroadcast >= BROADCAST_INTERVAL) {
                    GameObjectArray gameObjectsList = new GameObjectArray();
                    gameObjectsList.gameObjectsData = this._gameObjects.Values.ToList();
                    byte[] usersData = gameObjectsList.GetBytes();
                    DataHeader gameObjectsDataHeader = new DataHeader(
                        DataType.GameObjects,
                        usersData.Length,
                        usersData
                    );
                    BinaryMessage gameObjectsBinaryMessage = new BinaryMessage(gameObjectsDataHeader);
                    this.tcpServer.SendBroadcastMessage(gameObjectsBinaryMessage);
                    _timeSinceLastBroadcast = 0f;
                }
            }
        }

        private void QueueAddObject(GameObject obj)
        {
            this._objectsToAdd.Enqueue(obj);
        }

        public void Stop()
        {
            this.tcpServer.Stop();
            _serverTask?.Wait(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            this._serverTask?.Dispose();
        }

        public int Run()
        {
            try {
                var frameTimer = new Stopwatch();
                frameTimer.Start();
                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                while (this.isRunning) {
                    frameTimer.Restart();
                    float deltaTime = (float) stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart();
                    this.currentFps = deltaTime;
                    if (!isRunning) break; // if game has been stopped, then break out this while loop
                    this.Update(deltaTime);
                    // Frame pacing - sleep if we're ahead of schedule
                    int sleepTime = (int) ( ( FRAME_TIME * 1000 ) - frameTimer.ElapsedMilliseconds );
                    if (sleepTime > 0) {
                        Thread.Sleep(sleepTime);
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
    }
}