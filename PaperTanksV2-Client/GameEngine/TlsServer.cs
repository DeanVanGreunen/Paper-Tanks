
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace PaperTanksV2Client.GameEngine
{

    public class TlsServer
    {
        private TcpListener _listener;
        private X509Certificate2 _serverCertificate;
        private bool _isRunning;

        public TlsServer(int port, X509Certificate2 certificate)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _serverCertificate = certificate;
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"TLS Server started on port {( (IPEndPoint) _listener.LocalEndpoint ).Port}");

            while (_isRunning) {
                try {
#pragma warning disable CA2000 // Dispose objects before losing scope
                    var tcpClient = await this._listener.AcceptTcpClientAsync().ConfigureAwait(true);
#pragma warning restore CA2000 // Dispose objects before losing scope
                    _ = Task.Run(() => HandleClientAsync(tcpClient));
                } catch (ObjectDisposedException) {
                    break; // Server stopped
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            SslStream sslStream = null;
            try {
                Console.WriteLine($"Client connected: {tcpClient.Client.RemoteEndPoint}");

                // Create SSL stream
                sslStream = new SslStream(tcpClient.GetStream());

                // Authenticate as server
                await sslStream.AuthenticateAsServerAsync(this._serverCertificate).ConfigureAwait(true);

                Console.WriteLine($"SSL connection established. Cipher: {sslStream.CipherAlgorithm}");

                // Handle client communication
                await ProcessClientMessagesAsync(sslStream);
            } catch (Exception ex) {
                Console.WriteLine($"Error handling client: {ex.Message}");
            } finally {
                sslStream?.Close();
                tcpClient?.Close();
            }
        }

        private async Task ProcessClientMessagesAsync(SslStream sslStream)
        {
            var buffer = new byte[4096];

            while (sslStream.IsAuthenticated && sslStream.CanRead) {
                try {
                    int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(true);

                    if (bytesRead == 0) {
                        Console.WriteLine("Client disconnected");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    // Echo the message back
                    string response = $"Server received: {message}";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length).ConfigureAwait(true);
                    await sslStream.FlushAsync().ConfigureAwait(true);
                } catch (IOException ex) {
                    Console.WriteLine($"Connection lost: {ex.Message}");
                    break;
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }

        // Helper method to create a self-signed certificate for testing
        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            // For production, use proper certificates from a CA
            // This is for testing only
            using (var rsa = System.Security.Cryptography.RSA.Create(2048)) {
                var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(
                    $"CN={subjectName}", rsa, System.Security.Cryptography.HashAlgorithmName.SHA256,
                    System.Security.Cryptography.RSASignaturePadding.Pkcs1);

                var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
                return new X509Certificate2(certificate.Export(X509ContentType.Pfx), (string) null,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            }
        }
    }
}
