using System;
using System.Net.Sockets;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class ClientConnection
    {
        public Socket Socket { get; }
        public Guid Id { get; set; }
        
        public bool isReady { get; set; }
            
        public ClientConnection(Socket socket)
        {
            Socket = socket;
        }
    }
}