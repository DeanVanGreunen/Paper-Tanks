using System.Net.Sockets;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class Server
    {
        private TCPServer tcpServer;
        private short Port;
        public Server(string IPAddress, short Port)
        {
            this.IPAddress = IPAddress;
            this.Port = Port;
            this.tcpServer = new TCPServer(this.Port);
            this.tcpServer.OnConnection += this.OnConnection;
            this.tcpServer.OnDisconnection += this.OnDisconnection;
            this.tcpServer.OnMessageReceived += this.OnMessageReceived;
        }

        public void OnConnection(Socket socket)
        {
                
        }
        public void OnDisconnection(Socket socket)
        {
            
        }
        public void OnMessageReceived(Socket socket,  BinaryMessage message)
        {
            
        }
    }
}