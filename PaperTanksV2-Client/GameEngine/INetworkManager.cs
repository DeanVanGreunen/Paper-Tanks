using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public interface INetworkManager
    {
        void SendUpdate(GameState state);
        void ReceiveUpdate(Action<GameState> updateCallback);
        void SendPlayerInput(PlayerInput input);
        bool IsServer { get; }
        bool IsConnected { get; }
    }
}
