using PaperTanksV2Client.GameEngine.Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PaperTanksV2Client.GameEngine.Server.Data;

namespace PaperTanksV2Client.GameEngine.Client
{
    public class TCPClient
    {
        private string IPAddress = "127.0.0.1";
        private short Port = 9091;
        private Socket _socket;
        private bool _isConnected = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _receiveTask;
        private byte[] _receiveBuffer = new byte[0];

        public event Action<Socket> OnConnected;
        public event Action<Socket> OnDisconnected;
        public event Action<Socket, BinaryMessage> OnMessageReceived;

        public TCPClient(string IPAddress, short Port)
        {
            this.IPAddress = IPAddress;
            this.Port = Port;
        }

        public bool Connect()
        {
            try {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port);
                _socket.Connect(endPoint);
                _isConnected = true;
                _cancellationTokenSource = new CancellationTokenSource();
                _receiveTask = Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));
                OnConnected?.Invoke(_socket);
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"Connection failed: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            if (!_isConnected) return;
            try {
                _isConnected = false;
                _cancellationTokenSource?.Cancel();
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Close();
                _receiveTask?.Wait(TimeSpan.FromSeconds(2));

                OnDisconnected?.Invoke(_socket);
            } catch (Exception ex) {
                Console.WriteLine($"Disconnect error: {ex.Message}");
            }
        }

        public async Task<bool> SendAsync(BinaryMessage message)
        {
            if (!_isConnected || _socket == null) return false;

            try {
                byte[] data = message.ToBinaryArray();
                await Task.Run(() => _socket.Send(data));
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"Send error: {ex.Message}");
                return false;
            }
        }

        public bool Send(BinaryMessage message)
        {
            if (!_isConnected || _socket == null) return false;

            try {
                byte[] data = message.ToBinaryArray();
                _socket.Send(data);
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"Send error: {ex.Message}");
                return false;
            }
        }

        private void ReceiveLoop(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];

            while (!cancellationToken.IsCancellationRequested && _isConnected) {
                try {
                    if (_socket.Available > 0) {
                        int bytesRead = _socket.Receive(buffer);

                        if (bytesRead > 0) {
                            // Append to buffer
                            byte[] newData = new byte[_receiveBuffer.Length + bytesRead];
                            Array.Copy(_receiveBuffer, 0, newData, 0, _receiveBuffer.Length);
                            Array.Copy(buffer, 0, newData, _receiveBuffer.Length, bytesRead);
                            _receiveBuffer = newData;

                            // Try to extract complete messages
                            while (_receiveBuffer.Length >= 5) {
                                int messageLength = BinaryHelper.ToInt32BigEndian(_receiveBuffer, 1);
                                int totalLength = 5 + messageLength;

                                if (_receiveBuffer.Length >= totalLength) {
                                    // Extract complete message
                                    byte[] messageData = new byte[totalLength];
                                    Array.Copy(_receiveBuffer, messageData, totalLength);

                                    // Remove from buffer
                                    byte[] remaining = new byte[_receiveBuffer.Length - totalLength];
                                    Array.Copy(_receiveBuffer, totalLength, remaining, 0, remaining.Length);
                                    _receiveBuffer = remaining;

                                    BinaryMessage message = BinaryMessage.FromBinaryArray(messageData);
                                    if (message != null) {
                                        OnMessageReceived?.Invoke(_socket, message);
                                    }
                                } else {
                                    // Wait for more data
                                    break;
                                }
                            }
                        } else {
                            break;
                        }
                    }

                    Thread.Sleep(10);
                } catch (Exception ex) {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }

            if (_isConnected) {
                Disconnect();
            }
        }

        public bool IsConnected => _isConnected;
    }
}