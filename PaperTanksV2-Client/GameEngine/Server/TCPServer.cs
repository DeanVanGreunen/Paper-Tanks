using PaperTanksV2Client.GameEngine.Server.Data;
using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class TCPServer
    {
        private TcpListener _listener;
        private ConcurrentDictionary<Socket, ClientConnection> _clients;
        private bool _isRunning;
        private readonly int _port;
        
        // Hard Limits
        private const Int32 BufferMaxSize = 8192;
        
        // Delegate definitions
        public delegate void ConnectionHandler(Socket socket);
        public delegate void DisconnectionHandler(Socket socket);
        public delegate void MessageReceivedHandler(Socket socket, BinaryMessage binary);
        
        // Events
        public event ConnectionHandler OnConnection;
        public event DisconnectionHandler OnDisconnection;
        public event MessageReceivedHandler OnMessageReceived;
        
        public TCPServer(int Port)
        {
            _port = Port;
            _clients = new ConcurrentDictionary<Socket, ClientConnection>();
        }
        
        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;
            
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    var socket = client.Client;
                    
                    var connection = new ClientConnection(socket);
                    _clients.TryAdd(socket, connection);
                    
                    OnConnection?.Invoke(socket);
                    
                    _ = Task.Run(() => HandleClientAsync(socket));
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        // Log error
                    }
                }
            }
        }
        
        private async Task HandleClientAsync(Socket socket)
        {
            var buffer = new byte[BufferMaxSize];
    
            try
            {
                while (_isRunning && socket.Connected)
                {
                    var bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            
                    if (bytesRead == 0)
                    {
                        break;
                    }
            
                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
            
                    // Deserialize the DataHeader from the received bytes
                    var dataHeader = BinaryMessage.FromBinaryArray(data);
                    OnMessageReceived?.Invoke(socket, dataHeader);
                }
            }
            catch (Exception ex)
            {
                // Connection error or malformed data
            }
            finally
            {
                DisconnectClient(socket);
            }
        }


        private void DisconnectClient(Socket socket)
        {
            if (_clients.TryRemove(socket, out var connection))
            {
                OnDisconnection?.Invoke(socket);
                
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch { }
                
                socket.Close();
            }
        }
        
        public async Task SendAsync(Socket socket, BinaryMessage message)
        {
            if (_clients.ContainsKey(socket))
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(message.ToBinaryArray()), SocketFlags.None);
                }
                catch
                {
                    DisconnectClient(socket);
                }
            }
        }
        
        public void SendBroadcastMessage(BinaryMessage message)
        {
            foreach (var client in  this._clients) {
                _ = this.SendAsync(client.Key, message);
            }
        }
        
        public void Stop()
        {
            _isRunning = false;
            
            foreach (var socket in _clients.Keys)
            {
                DisconnectClient(socket);
            }
            
            _listener?.Stop();
        }

        public ClientConnection GetBySocket(Socket socket)
        {
            return _clients[socket] ?? null;
        }

        public List<ClientConnection> GetAllClients()
        {
            return this._clients?.Values?.ToList() ?? new List<ClientConnection>();
        }
        public void SetClient(Socket socket, Action<ClientConnection> callback)
        {
            if (this._clients != null) {
                if (this._clients.ContainsKey(socket) == true) {
                    callback?.Invoke(this._clients[socket]);
                }
            }
        }
    }
}