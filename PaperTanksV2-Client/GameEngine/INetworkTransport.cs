using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public interface INetworkTransport
    {
        event Action<NetworkMessageType, byte[]> OnDataReceived;
        event Action<ConnectionStatus> OnConnectionStatusChanged;

        void SendReliable(NetworkMessageType type, byte[] data);
        void SendUnreliable(NetworkMessageType type, byte[] data);
        void Connect(string address, int port);
        void Disconnect();
    }
}
