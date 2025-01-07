using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public enum NetworkMessageType
    {
        GameState,
        PlayerInput,
        ConnectionRequest,
        ConnectionResponse
    }
}
