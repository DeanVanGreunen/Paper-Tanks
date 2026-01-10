using System;
using System.Net.Sockets;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class ClientConnection
    {
        public Socket Socket { get; }
        public Guid Id { get; }
            
        public ClientConnection(Socket socket)
        {
            Socket = socket;
            Id = Guid.NewGuid();
        }
    }
}