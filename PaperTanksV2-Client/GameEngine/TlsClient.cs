using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace PaperTanksV2Client.GameEngine
{
    public class TlsClient : IDisposable
    {
        private TcpClient _tcpClient;
        private SslStream _sslStream;
        private bool _isConnected;

        public async Task<bool> ConnectAsync(string serverName, int port, bool validateCertificate = true)
        {
            try {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(serverName, port).ConfigureAwait(true);

                // Create SSL stream
                _sslStream = new SslStream(
                    _tcpClient.GetStream(),
                    false,
                    ((object sender,
                        X509Certificate certificate,
                        X509Chain chain,
                        SslPolicyErrors sslPolicyErrors) => {
                            if (validateCertificate) {
                                return this.ValidateServerCertificate(sender, certificate, chain, sslPolicyErrors);
                            }
                            return false;
                    } )
                );

                // Authenticate as client
                await this._sslStream.AuthenticateAsClientAsync(serverName).ConfigureAwait(true);

                _isConnected = true;
                Console.WriteLine($"Connected to {serverName}:{port} with SSL/TLS");
                Console.WriteLine($"Cipher: {_sslStream.CipherAlgorithm}, Key Exchange: {_sslStream.KeyExchangeAlgorithm}");

                return true;
            } catch (Exception ex) {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task<string> SendMessageAsync(string message)
        {
            if (!_isConnected || _sslStream == null)
                throw new InvalidOperationException("Not connected to server");

            try {
                // Send message
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                await this._sslStream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(true);
                await this._sslStream.FlushAsync().ConfigureAwait(true);

                // Read response
                var buffer = new byte[4096];
                int bytesRead = await this._sslStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(true);

                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } catch (Exception ex) {
                Console.WriteLine($"Error sending message: {ex.Message}");
                throw;
            }
        }

        public async Task StartListeningAsync()
        {
            if (!_isConnected || _sslStream == null)
                return;

            var buffer = new byte[4096];

            try {
                while (_sslStream.IsAuthenticated && _sslStream.CanRead) {
                    int bytesRead = await this._sslStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(true);

                    if (bytesRead == 0) {
                        Console.WriteLine("Server disconnected");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");
                }
            } catch (IOException ex) {
                Console.WriteLine($"Connection lost: {ex.Message}");
            }
        }

        private bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine($"Certificate validation errors: {sslPolicyErrors}");

            // For production, implement proper certificate validation
            // For testing with self-signed certificates, you might want to:
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors ||
                sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch) {
                Console.WriteLine("Warning: Using certificate with validation errors (acceptable for testing)");
                return true; // Accept for testing - DO NOT do this in production
            }

            return false;
        }

        public void Disconnect()
        {
            this._isConnected = false;
            this._sslStream?.Close();
            this._tcpClient?.Close();
        }

        public void Dispose()
        {
            this.Disconnect();
            this._sslStream?.Dispose();
            this._tcpClient?.Dispose();
        }
    }
}
