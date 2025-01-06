using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class StateChange
    {
        public Guid ObjectId { get; }
        public GameObjectState PreviousState { get; }
        public GameObjectState NewState { get; }

        public StateChange(Guid objectId, GameObjectState previousState, GameObjectState newState)
        {
            ObjectId = objectId;
            PreviousState = previousState;
            NewState = newState;
        }
    }
}
