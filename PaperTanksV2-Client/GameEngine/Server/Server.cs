using Gtk;
using PaperTanksV2Client.GameEngine.AI;
using PaperTanksV2Client.GameEngine.data;
using PaperTanksV2Client.GameEngine.Server.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Socket = System.Net.Sockets.Socket;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class Server : ServerRunner, IDisposable
    {
        private Dictionary<Guid, GameObject> _gameObjects = new Dictionary<Guid, GameObject>();
        private TCPServer tcpServer;
        private GameEngineInstance engine;
        private short Port;
        private Task _serverTask;
        private ServerGameMode gMode = ServerGameMode.Lobby;
        private float _timeSinceLastHeartbeat = 0f;
        private float _timeSinceLastDeltaBroadcast = 0f;
        private const float HEARTBEAT_INTERVAL = 1f;
        private const float DELTA_BROADCAST_INTERVAL = 0.033f; // ~30 updates per second
        private float _lobbyCountdown = 0f;
        private bool _isCountdownActive = false;
        private const float LOBBY_COUNTDOWN_DURATION = 5f;
        private float movementSpeed = 100;
        private Queue<MovementCommand> _movementQueue = new Queue<MovementCommand>();
        private Queue<FireCommand> _fireQueue = new Queue<FireCommand>();
        private Queue<GameObject> _objectsToAdd = new Queue<GameObject>();
        private List<GameObject> _objectsAddedThisFrame = new List<GameObject>();
        private Level _level = null;
        public bool isRunning = true;
        public const int TARGET_FPS = 60;
        public const float FRAME_TIME = 1.0f / TARGET_FPS;
        private double currentFps;
        private int CLIENT_MIN_COUNT = 1;
        private bool _initialGameObjectsSent = false;
        private readonly object _engineLock = new object();
        
        public Server(short Port)
        {
            this.Port = Port;
            this.tcpServer = new TCPServer(this.Port);
            this.tcpServer.OnConnection += this.OnConnection;
            this.tcpServer.OnDisconnection += this.OnDisconnection;
            this.tcpServer.OnMessageReceived += this.OnMessageReceived;
            this.engine = new GameEngineInstance(true, null, null, null, null);
        }

        public void OnConnection(Socket socket)
        {
            IPEndPoint remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            if (remoteEndPoint != null) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine(
                    $"New Client Connected To Server from {remoteEndPoint.Address}:{remoteEndPoint.Port}");
            } else {
                if(TextData.DEBUG_MODE == true) Console.WriteLine("New Client Connected To Server (unknown endpoint)");
            }
    
            try
            {
                byte[] clientConnectionBytes = BinaryHelper.GetBytesBigEndian(this.tcpServer.GetAllClients().ToArray());
                DataHeader clientConnectionDataHeader = new DataHeader(
                    DataType.Users,
                    clientConnectionBytes.Length,
                    clientConnectionBytes
                );
                BinaryMessage clientConnectionBinaryMessage = new BinaryMessage(clientConnectionDataHeader);
                _ = this.tcpServer.SendAsync(socket, clientConnectionBinaryMessage);

                byte[] gModeBytes = BinaryHelper.GetBytesBigEndian((int)this.gMode);
                DataHeader gModeDataHeader = new DataHeader(
                    DataType.GameMode,
                    gModeBytes.Length,
                    gModeBytes
                );
                BinaryMessage gModeBinaryMessage = new BinaryMessage(gModeDataHeader);
                _ = this.tcpServer.SendAsync(socket, gModeBinaryMessage);
        
                // If already in GamePlay mode, send full game objects to new client
                if (this.gMode == ServerGameMode.GamePlay)
                {
                    SendFullGameObjectsToClient(socket);
                }
                
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Sent game mode {this.gMode} ({(int)this.gMode}) to new client");
            }
            catch (Exception ex)
            {
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Error in OnConnection: {ex.Message}");
            }
        }

        public void OnDisconnection(Socket socket)
        {
            IPEndPoint remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            if (remoteEndPoint != null) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine(
                    $"Client Disconnected To Server from {remoteEndPoint.Address}:{remoteEndPoint.Port}");
            } else {
                if(TextData.DEBUG_MODE == true) Console.WriteLine("Client Disconnected To Server (unknown endpoint)");
            }

            ClientConnection c = this.tcpServer.GetBySocket(socket);
            this.engine.DeleteObject(c.Id);
        }

        public void OnMessageReceived(Socket socket, BinaryMessage message)
        {
            if (message == null) return;
            if (message.DataHeader.dataType == DataType.HeartBeat) {
                _ = this.tcpServer.SendAsync(socket, BinaryMessage.HeartBeatMessage);
                return;
            }
            if (this.gMode == ServerGameMode.Lobby) {
                this.tcpServer.SetClient(socket, (ClientConnection c) => {
                    c.isReady = true;
                    Tank tank = new Tank(true, new Weapon(10, 100), null, null, null, null, null, null, null);
                    tank.Id = c.Id;
                    tank.Bounds.Position = new Vector2Data(50, 50);
                    tank.Bounds.Size = new Vector2Data(50, 50);
                    this.engine.AddObject(tank);
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
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(client.Id);
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
            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Loading Level");
            if (!this.LoadRandomLevel()) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Unable To Load Level");
                return;
            }
            this._level.fileName = $"{this._level.fileName}.json";
            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Level Loaded -> {this._level.fileName} -> {this._level.levelName}");
            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Starting Server");
            _serverTask = this.tcpServer.StartAsync();
            if(TextData.DEBUG_MODE == true) Console.WriteLine($"Server Started Successfully");
            this.gMode = ServerGameMode.Lobby;
        }

        public bool LoadRandomLevel()
        {
            List<string> levels = MultiplayerManager.GetMultiPlayerList();
            if (levels.Count == 0) return false;
            string levelName = levels[new Random().Next(levels.Count)];
            this._level = MultiplayerManager.LoadLevel(levelName);
            return true;
        }

        private void LoadCurrentLevel()
        {
            if (this._level != null) {
                if (this._level.gameObjects != null) {
                    foreach (var obj in this._level.gameObjects) {
                        Guid guid = Guid.NewGuid();
                        if (obj is Tank) {
                            if (( obj as Tank ).Weapon0 == null) {
                                ( obj as Tank ).Weapon0 = new Weapon(10, 100);
                                if (!( obj as Tank ).IsPlayer) {
                                    ( obj as Tank ).AiAgent = AIAgent.GetRandomAI();
                                }
                            }
                        }

                        if (obj is AmmoPickup) {
                            ( obj as AmmoPickup ).AmmoCount = 20;
                            obj.IsStatic = true;
                        }

                        if (obj is HealthPickup) {
                            ( obj as HealthPickup ).Health = 50;
                            obj.IsStatic = true;
                        }

                        if (obj is Wall) {
                            obj.IsStatic = true;
                        }

                        this.QueueAddObject(obj);
                    }
                }
            }
            
            if(TextData.DEBUG_MODE == true) 
                Console.WriteLine($"[Server] Level loaded with {this._level?.gameObjects?.Count ?? 0} objects");
        }

        public void Update(float deltaTime)
        {
            // Accumulate time
            _timeSinceLastHeartbeat += deltaTime;
            _timeSinceLastDeltaBroadcast += deltaTime;

            // Check if we should switch from Lobby to GamePlay mode
            if (this.gMode == ServerGameMode.Lobby) {
                ClientConnection[] clients2 = this.tcpServer.GetAllClients().ToArray();
                if (clients2.Length >= CLIENT_MIN_COUNT) {
                    if (!_isCountdownActive) {
                        _isCountdownActive = true;
                        _lobbyCountdown = 0f;

                        // Send countdown start message
                        SendCountdownMessage(true, LOBBY_COUNTDOWN_DURATION);
                        if (TextData.DEBUG_MODE == true) Console.WriteLine("[Server] Lobby countdown started");
                    } else {
                        _lobbyCountdown += deltaTime;

                        if (_lobbyCountdown >= LOBBY_COUNTDOWN_DURATION) {
                            this.gMode = ServerGameMode.GamePlay;
                            _isCountdownActive = false;
                            _lobbyCountdown = 0f;
                            _initialGameObjectsSent = false;
                            this.LoadCurrentLevel();
                            this.LoadClients();

                            // Send countdown end message
                            SendCountdownMessage(false, 0f);
                            if (TextData.DEBUG_MODE == true)
                                Console.WriteLine("[Server] Lobby countdown ended - starting game");

                            byte[] gModeBytes = BinaryHelper.GetBytesBigEndian((int) this.gMode);
                            DataHeader gModeDataHeader = new DataHeader(
                                DataType.GameMode,
                                gModeBytes.Length,
                                gModeBytes
                            );
                            BinaryMessage gModeBinaryMessage = new BinaryMessage(gModeDataHeader);
                            this.tcpServer.SendBroadcastMessage(gModeBinaryMessage);
                        }
                    }
                } else {
                    if (_isCountdownActive) {
                        // Countdown was active but player count dropped, cancel it
                        SendCountdownMessage(false, 0f);
                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine("[Server] Lobby countdown cancelled - not enough players");
                    }

                    _isCountdownActive = false;
                    _lobbyCountdown = 0f;
                }

                // Broadcast lobby users periodically
                if (_timeSinceLastDeltaBroadcast >= 2f) {
                    ClientConnection[] clients = this.tcpServer.GetAllClients().ToArray();
                    byte[] usersData = BinaryHelper.GetBytesBigEndian(clients);
                    DataHeader usersDataHeader = new DataHeader(
                        DataType.Users,
                        usersData.Length,
                        usersData
                    );
                    BinaryMessage usersBinaryMessage = new BinaryMessage(usersDataHeader);
                    this.tcpServer.SendBroadcastMessage(usersBinaryMessage);
                    _timeSinceLastDeltaBroadcast = 0f;
                }
            }

            // Process movement queue
            if (this.gMode == ServerGameMode.GamePlay) {
                _objectsAddedThisFrame.Clear();

                // ====== STEP 1: Add queued objects FIRST ======
                while (_objectsToAdd.Count > 0) {
                    GameObject obj = _objectsToAdd.Dequeue();
                    _gameObjects.Add(obj.Id, obj);
                    this.engine.AddObject(obj);
                    _objectsAddedThisFrame.Add(obj);
                }

                // Broadcast new objects
                if (_objectsAddedThisFrame.Count > 0) {
                    if (TextData.DEBUG_MODE == true)
                        Console.WriteLine($"[Server] Broadcasting {_objectsAddedThisFrame.Count} newly added objects");

                    foreach (var newObj in _objectsAddedThisFrame) {
                        SendNewObjectBroadcast(newObj);
                    }
                }

                // Send initial full state
                if (!_initialGameObjectsSent && this._gameObjects.Count > 0) {
                    SendFullGameObjects();
                }

                // ====== STEP 2: NOW process movement commands ======
                while (_movementQueue.Count > 0) {
                    MovementCommand cmd = _movementQueue.Dequeue();

                    if (this._gameObjects.ContainsKey(cmd.ClientId)) {
                        GameObject gameObject = this._gameObjects[cmd.ClientId];
                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine(
                                $"[Server] Processing movement for {cmd.ClientId}: {cmd.MovementData.input}");

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
                    } else {
                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine($"[Server] WARNING: Movement command for unknown client {cmd.ClientId}");
                    }
                }

                // ====== STEP 3: Process fire commands ======
                while (_fireQueue.Count > 0) {
                    FireCommand cmd = _fireQueue.Dequeue();

                    if (this._gameObjects.ContainsKey(cmd.ClientId)) {
                        GameObject player = this._gameObjects[cmd.ClientId];

                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine($"[Server] Processing fire command for {cmd.ClientId}");

                        if (player is Tank tank && tank.Weapon0 != null && tank.Weapon0.AmmoCount >= 1) {
                            Projectile projectile = new Projectile(SKColors.Red, player.Id);
                            Vector2Data size = new Vector2Data(8, 8);

                            // Initialize velocity
                            projectile.Velocity = new Vector2Data(0, 0);

                            // Set position and velocity based on player rotation
                            if (player.Rotation == 0) {
                                projectile.Bounds = new BoundsData(
                                    new Vector2Data(player.Position.X + 100,
                                        player.Position.Y + ( player.Size.Y / 2 ) - ( size.Y / 2 )), size);
                                projectile.Velocity.X = this.movementSpeed;
                                projectile.Velocity.Y = 0;
                            } else if (player.Rotation == -180) {
                                projectile.Bounds = new BoundsData(
                                    new Vector2Data(player.Position.X - 58,
                                        player.Position.Y + ( player.Size.Y / 2 ) - ( size.Y / 2 )), size);
                                projectile.Velocity.X = -this.movementSpeed;
                                projectile.Velocity.Y = 0;
                            } else if (player.Rotation == -90) {
                                projectile.Bounds = new BoundsData(
                                    new Vector2Data(player.Position.X + ( player.Size.X / 2 ) - ( size.X / 2 ),
                                        player.Position.Y - 58), size);
                                projectile.Velocity.X = 0;
                                projectile.Velocity.Y = -this.movementSpeed;
                            } else if (player.Rotation == 90) {
                                projectile.Bounds = new BoundsData(
                                    new Vector2Data(player.Position.X + ( player.Size.X / 2 ) - ( size.X / 2 ),
                                        player.Position.Y + 100), size);
                                projectile.Velocity.X = 0;
                                projectile.Velocity.Y = this.movementSpeed;
                            }

                            if (TextData.DEBUG_MODE == true) {
                                Console.WriteLine(
                                    $"[Server] Created projectile at ({projectile.Position.X:F1}, {projectile.Position.Y:F1}) with velocity ({projectile.Velocity.X:F1}, {projectile.Velocity.Y:F1})");
                            }

                            tank.Weapon0.AmmoCount -= 1;
                            this.QueueAddObject(projectile);
                        }
                    } else {
                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine($"[Server] WARNING: Fire command for unknown client {cmd.ClientId}");
                    }
                }

                // ====== STEP 4: Update all game objects ======
                // ====== STEP 4: Update all game objects ======
                if (this._gameObjects.Values.Count >= 1) {
                    var objectsList1 = this.engine.GetObjects().Values.ToList(); // Use snapshot
                    var objectsToDelete = new List<Guid>();

                    // Update all objects
                    foreach (var obj in objectsList1) {
                        obj.Update(this.engine, deltaTime);
                        if (obj.IsOutOfBounds(1920, 1080)) {
                            obj.deleteMe = true;
                        }

                        if (obj.deleteMe) {
                            objectsToDelete.Add(obj.Id);
                        }
                    }

                    // ====== COLLISION DETECTION - MOVED OUTSIDE DELETION BLOCK ======
                    // Create a snapshot to avoid modification during enumeration
                    var objectsListForCollision = this._gameObjects.Values.ToList();

                    if (TextData.DEBUG_MODE == true && objectsListForCollision.Count > 1)
                        Console.WriteLine($"[Server] Checking collisions for {objectsListForCollision.Count} objects");

                    for (int i = 0; i < objectsListForCollision.Count; i++) {
                        var obj1 = objectsListForCollision[i];

                        // Skip if already marked for deletion
                        if (obj1.deleteMe) continue;

                        for (int j = i + 1; j < objectsListForCollision.Count; j++) {
                            var obj2 = objectsListForCollision[j];

                            // Skip if already marked for deletion
                            if (obj2.deleteMe) continue;

                            // HandleCollision does its own collision checking
                            obj1.HandleCollisionEngine(this.engine, obj2);
                            obj2.HandleCollisionEngine(this.engine, obj1);

                            // Check if either object was marked for deletion after collision
                            if (obj1.deleteMe && !objectsToDelete.Contains(obj1.Id)) {
                                objectsToDelete.Add(obj1.Id);
                                if (TextData.DEBUG_MODE == true)
                                    Console.WriteLine(
                                        $"[Server] Collision: {obj1.GetType().Name} ({obj1.Id}) marked for deletion");
                            }

                            if (obj2.deleteMe && !objectsToDelete.Contains(obj2.Id)) {
                                objectsToDelete.Add(obj2.Id);
                                if (TextData.DEBUG_MODE == true)
                                    Console.WriteLine(
                                        $"[Server] Collision: {obj2.GetType().Name} ({obj2.Id}) marked for deletion");
                            }
                        }
                    }

                    // Delete marked objects
                    if (objectsToDelete.Count > 0) {
                        if (TextData.DEBUG_MODE == true)
                            Console.WriteLine($"[Server] Deleting {objectsToDelete.Count} objects");

                        foreach (var id in objectsToDelete) {
                            this._gameObjects.Remove(id);
                            this.engine.DeleteObject(id);
                        }

                        SendObjectDeletionBroadcast(objectsToDelete);
                    }
                }

                // ====== STEP 5: Send delta updates ======
                if (_timeSinceLastDeltaBroadcast >= DELTA_BROADCAST_INTERVAL) {
                    SendDeltaUpdates();
                    _timeSinceLastDeltaBroadcast = 0f;
                }
            }

            // Heartbeat
            if (_timeSinceLastHeartbeat >= HEARTBEAT_INTERVAL) {
                this.tcpServer.SendBroadcastMessage(BinaryMessage.HeartBeatMessage);
                _timeSinceLastHeartbeat = 0f;
            }
        }

        private void SendObjectDeletionBroadcast(List<Guid> objectIds)
        {
            try {
                if (objectIds == null || objectIds.Count == 0) return;

                if(TextData.DEBUG_MODE == true) 
                    Console.WriteLine($"Broadcasting deletion of {objectIds.Count} objects");

                // Create deletion message: [count][guid1][guid2]...
                var deletionData = new List<byte>();
                
                // Write count of objects to delete
                deletionData.AddRange(BitConverter.GetBytes(objectIds.Count));
                
                // Write each GUID
                foreach (var id in objectIds) {
                    deletionData.AddRange(id.ToByteArray());
                    if(TextData.DEBUG_MODE == true) 
                        Console.WriteLine($"  Deleting object ID: {id}");
                }

                DataHeader deletionHeader = new DataHeader(
                    DataType.DeleteGameObject,
                    deletionData.Count,
                    deletionData.ToArray()
                );
                BinaryMessage deletionMessage = new BinaryMessage(deletionHeader);
                this.tcpServer.SendBroadcastMessage(deletionMessage);

                if(TextData.DEBUG_MODE == true) 
                    Console.WriteLine($"Successfully broadcast deletion of {objectIds.Count} objects");
            } catch (Exception ex) {
                if(TextData.DEBUG_MODE == true) 
                    Console.WriteLine($"Error broadcasting object deletion: {ex.Message}");
            }
        }

        private void SendNewObjectBroadcast(GameObject newObject)
        {
            try {
                if (newObject == null || newObject.Id == Guid.Empty) return;
                if (newObject.Bounds == null || newObject.Bounds.Position == null || newObject.Bounds.Size == null) return;

                // Validate the object
                if (newObject is Tank tank && tank.Weapon0 == null) {
                    tank.Weapon0 = new Weapon(0, 0);
                }

                if(TextData.DEBUG_MODE == true) 
                    Console.WriteLine($"Broadcasting new object: {newObject.GetType().Name} ID={newObject.Id}");

                // Send the full object data for the new object
                var objectList = new List<GameObject> { newObject };
                GameObjectArray gameObjectsList = new GameObjectArray(objectList);
                byte[] objectData = gameObjectsList.GetBytes();

                if (objectData == null || objectData.Length <= 0) {
                    if(TextData.DEBUG_MODE == true) Console.WriteLine("Failed to serialize new object");
                    return;
                }

                DataHeader newObjectHeader = new DataHeader(
                    DataType.NewGameObject,
                    objectData.Length,
                    objectData
                );
                BinaryMessage newObjectMessage = new BinaryMessage(newObjectHeader);
                this.tcpServer.SendBroadcastMessage(newObjectMessage);

                if(TextData.DEBUG_MODE == true) 
                    Console.WriteLine($"Successfully broadcast new object: {newObject.GetType().Name}");
            } catch (Exception ex) {
                if(TextData.DEBUG_MODE == true) 
                    Console.WriteLine($"Error broadcasting new object: {ex.Message}");
            }
        }

        private void SendCountdownMessage(bool isStarting, float duration)
        {
            try {
                // Create message: [isStarting (1 byte)][duration (4 bytes)]
                byte[] messageData = new byte[5];
                messageData[0] = isStarting ? (byte)1 : (byte)0;
                byte[] durationBytes = BitConverter.GetBytes(duration);
                Array.Copy(durationBytes, 0, messageData, 1, 4);
                
                DataHeader countdownHeader = new DataHeader(
                    DataType.LobbyCountdown,
                    messageData.Length,
                    messageData
                );
                BinaryMessage countdownMessage = new BinaryMessage(countdownHeader);
                this.tcpServer.SendBroadcastMessage(countdownMessage);
            } catch (Exception ex) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Error sending countdown message: {ex.Message}");
            }
        }

        private void LoadClients()
        {
            List<ClientConnection> clients = this.tcpServer.GetAllClients();
            foreach (var client in clients) {
                Tank tank = new Tank(true, new Weapon(10, 100), null, null, null, null, null, null, game => {
                    if(TextData.DEBUG_MODE == true) Console.WriteLine($"Tank Died - {client.Id}");
                });
                tank.Id = client.Id;
                tank.Bounds.Position = new Vector2Data(50, 50);
                this.QueueAddObject(tank);
                if(TextData.DEBUG_MODE == true) 
                    Console.WriteLine($"[Server] Queued player tank for client {client.Id}");
            }
            this.gMode = ServerGameMode.GamePlay;
        }

        private void SendFullGameObjects()
        {
            try {
                var gameObjects = this.engine.GetObjects().Values.ToList();

                if (gameObjects == null || gameObjects.Count == 0) {
                    if(TextData.DEBUG_MODE == true) Console.WriteLine("No game objects to send");
                    return;
                }

                var validObjects = new List<GameObject>();
                foreach (var obj in gameObjects) {
                    if (obj == null || obj.Id == Guid.Empty) continue;
                    if (obj.Bounds == null || obj.Bounds.Position == null || obj.Bounds.Size == null) continue;

                    if (obj is Tank tank && tank.Weapon0 == null) {
                        tank.Weapon0 = new Weapon(0, 0);
                    }

                    validObjects.Add(obj);
                }

                if (validObjects.Count == 0) return;

                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Sending initial {validObjects.Count} game objects");

                GameObjectArray gameObjectsList = new GameObjectArray(validObjects);
                byte[] usersData = gameObjectsList.GetBytes();
                DataHeader gameObjectsDataHeader = new DataHeader(
                    DataType.GameObjects,
                    usersData.Length,
                    usersData
                );
                BinaryMessage gameObjectsBinaryMessage = new BinaryMessage(gameObjectsDataHeader);
                this.tcpServer.SendBroadcastMessage(gameObjectsBinaryMessage);
                
                _initialGameObjectsSent = true;
            } catch (Exception ex) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Error in SendFullGameObjects: {ex.Message}");
            }
        }

        private void SendFullGameObjectsToClient(Socket socket)
        {
            try {
                var gameObjects = this.engine.GetObjects().Values.ToList();
                if (gameObjects == null || gameObjects.Count == 0) return;

                var validObjects = new List<GameObject>();
                foreach (var obj in gameObjects) {
                    if (obj == null || obj.Id == Guid.Empty) continue;
                    if (obj.Bounds == null || obj.Bounds.Position == null || obj.Bounds.Size == null) continue;
                    if (obj is Tank tank && tank.Weapon0 == null) {
                        tank.Weapon0 = new Weapon(0, 0);
                    }
                    validObjects.Add(obj);
                }

                if (validObjects.Count == 0) return;

                GameObjectArray gameObjectsList = new GameObjectArray(validObjects);
                byte[] usersData = gameObjectsList.GetBytes();
                DataHeader gameObjectsDataHeader = new DataHeader(
                    DataType.GameObjects,
                    usersData.Length,
                    usersData
                );
                BinaryMessage gameObjectsBinaryMessage = new BinaryMessage(gameObjectsDataHeader);
                _ = this.tcpServer.SendAsync(socket, gameObjectsBinaryMessage);
            } catch (Exception ex) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine($"Error sending full objects to client: {ex.Message}");
            }
        }

        private void SendDeltaUpdates()
        {
            try {
                var gameObjects = this.engine.GetObjects().Values.ToList();
                if (gameObjects == null || gameObjects.Count == 0) return;

                // Create delta update message (positions, rotations, and tank-specific data)
                var deltaData = new List<byte>();

                // Count only objects that will be included (valid objects)
                int validObjectCount = 0;
                var validObjectData = new List<byte>();

                foreach (var obj in gameObjects) {
                    if (obj == null || obj.Id == Guid.Empty) continue;
                    if (obj.Bounds == null || obj.Bounds.Position == null) continue;

                    // Write object ID (16 bytes)
                    validObjectData.AddRange(obj.Id.ToByteArray());

                    // Write position (8 bytes: 2 floats)
                    validObjectData.AddRange(BitConverter.GetBytes(obj.Position.X));
                    validObjectData.AddRange(BitConverter.GetBytes(obj.Position.Y));

                    // Write rotation (4 bytes: 1 float)
                    validObjectData.AddRange(BitConverter.GetBytes(obj.Rotation));

                    // Write tank-specific data if this is a tank
                    if (obj is Tank tank) {
                        // Write flag indicating this is a tank (1 byte)
                        validObjectData.Add(1);

                        // Write health (4 bytes: 1 float)
                        validObjectData.AddRange(BitConverter.GetBytes(tank.Health));

                        // Write ammo count (4 bytes: 1 int)
                        int ammoCount = tank.Weapon0?.AmmoCount ?? 0;
                        validObjectData.AddRange(BitConverter.GetBytes(ammoCount));
                    } else {
                        // Write flag indicating this is NOT a tank (1 byte)
                        validObjectData.Add(0);
                    }

                    validObjectCount++;
                }

                // Write count of valid objects at the start
                deltaData.AddRange(BitConverter.GetBytes(validObjectCount));
                deltaData.AddRange(validObjectData);

                if (deltaData.Count > 4 && validObjectCount > 0) {
                    DataHeader deltaHeader = new DataHeader(
                        DataType.PositionUpdate,
                        deltaData.Count,
                        deltaData.ToArray()
                    );
                    BinaryMessage deltaMessage = new BinaryMessage(deltaHeader);
                    this.tcpServer.SendBroadcastMessage(deltaMessage);

                    if (TextData.DEBUG_MODE == true && validObjectCount > 0)
                        Console.WriteLine($"[Server] Sent delta update for {validObjectCount} objects");
                }
            } catch (Exception ex) {
                if (TextData.DEBUG_MODE == true) Console.WriteLine($"Error in SendDeltaUpdates: {ex.Message}");
            }
        }

        private void QueueAddObject(GameObject obj)
        {
            this._objectsToAdd.Enqueue(obj);
        }

        public void Dispose()
        {
            _serverTask?.Wait(TimeSpan.FromSeconds(5));
            this._serverTask?.Dispose();
            this.tcpServer.Stop();
            this.tcpServer = null;
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
                    if (!isRunning) break;
                    this.Update(deltaTime);
                    int sleepTime = (int) ( ( FRAME_TIME * 1000 ) - frameTimer.ElapsedMilliseconds );
                    if (sleepTime > 0) {
                        Thread.Sleep(sleepTime);
                    }
                }
            }
            catch (Exception e)
            {
                if(TextData.DEBUG_MODE == true) Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
    }
}