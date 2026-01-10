using PaperTanksV2Client.GameEngine.Server.Data;
using System;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class MovementCommand
    {
        public Guid ClientId { get; set; }
        public Movement MovementData { get; set; }
    }
}