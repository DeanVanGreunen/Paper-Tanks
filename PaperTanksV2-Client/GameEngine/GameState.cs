using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class GameState
    {
        public DateTime TimeStamp { get; set; }
        public Dictionary<Guid, GameObjectState> ObjectStates { get; set; }
        public uint SequenceNumber { get; set; }
        public Dictionary<string, object> WorldState { get; set; }

        public GameState()
        {
            TimeStamp = DateTime.UtcNow;
            ObjectStates = new Dictionary<Guid, GameObjectState>();
            WorldState = new Dictionary<string, object>();
        }

        // Create a snapshot of the entire game world
        public static GameState CreateSnapshot(IEnumerable<GameObject> gameObjects)
        {
            var state = new GameState {
                TimeStamp = DateTime.UtcNow,
                ObjectStates = new Dictionary<Guid, GameObjectState>()
            };

            foreach (var obj in gameObjects) {
                state.ObjectStates[obj.Id] = obj.GetState();
            }

            return state;
        }

        // Interpolate between two game states
        public static GameState Interpolate(GameState from, GameState to, float t)
        {
            var interpolated = new GameState {
                TimeStamp = DateTime.UtcNow,
                ObjectStates = new Dictionary<Guid, GameObjectState>()
            };

            // Interpolate states for all objects that exist in both states
            foreach (var kvp in from.ObjectStates) {
                if (to.ObjectStates.TryGetValue(kvp.Key, out var toState)) {
                    interpolated.ObjectStates[kvp.Key] = GameObjectState.Lerp(kvp.Value, toState, t);
                }
            }

            return interpolated;
        }
    }

}
