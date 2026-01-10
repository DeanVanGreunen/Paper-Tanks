using Gtk;
using PaperTanksV2Client.GameEngine.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Socket = System.Net.Sockets.Socket;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class Server : ServerRunner
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
            if (this.gMode == ServerGameMode.Lobby) {
                this.tcpServer.SetClient(socket, (ClientConnection c) => {
                    c.isReady = true;
                });
            } else if (this.gMode == ServerGameMode.GamePlay) {
                switch (message.DataHeader.dataType) {
                    case DataType.HeartBeat:
                        _ = this.tcpServer.SendAsync(socket, BinaryMessage.HeartBeatMessage);
                        break;
                    case DataType.Movement:
                        // TODO: Handle Movement
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
            // TODO: SET MODE TO LOBBY
            // ONCE LOBBY IS READY, SWITCH TO GAMEPLAY MODE
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

            // Push Heart Beat To Clients every 5 seconds
            if (_timeSinceLastHeartbeat >= HEARTBEAT_INTERVAL) {
                this.tcpServer.SendBroadcastMessage(BinaryMessage.HeartBeatMessage);
                _timeSinceLastHeartbeat = 0f;
            }

            // Broadcast game state every 5 seconds
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
                }

                _timeSinceLastBroadcast = 0f;
            } else if (this.gMode == ServerGameMode.GamePlay) {
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
            }

            // TODO: Update GameObjects with Lerping
            // TODO: Send GameObjects
            //this.tcpServer.SendBroadcastMessage();
            // TODO: PUSH CHANGES
        }

        public void Stop()
        {
            this.tcpServer.Stop();
            _serverTask?.Wait(TimeSpan.FromSeconds(5));
        }
    }
}