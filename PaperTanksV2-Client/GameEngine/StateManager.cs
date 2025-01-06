using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class StateManager
    {
        private readonly Dictionary<Guid, GameObjectState> previousState;
        private readonly Dictionary<Guid, GameObjectState> currentState;
        private readonly List<StateChange> pendingChanges;
        private readonly bool isAuthority;

        public StateManager(bool isAuthority)
        {
            this.isAuthority = isAuthority;
            previousState = new Dictionary<Guid, GameObjectState>();
            currentState = new Dictionary<Guid, GameObjectState>();
            pendingChanges = new List<StateChange>();
        }

        public void UpdateState(GameObject obj)
        {
            GameObjectState state = obj.GetState();

            if (previousState.TryGetValue(obj.Id, out var prevState)) {
                if (!StateEquals(prevState, state)) {
                    var change = new StateChange(obj.Id, prevState, state);
                    pendingChanges.Add(change);
                }
            }

            previousState[obj.Id] = currentState[obj.Id];
            currentState[obj.Id] = state;
        }

        public void ApplyChanges(IEnumerable<StateChange> changes)
        {
            if (!isAuthority) {
                foreach (var change in changes) {
                    if (currentState.ContainsKey(change.ObjectId)) {
                        currentState[change.ObjectId] = change.NewState;
                    }
                }
            }
        }

        public IEnumerable<StateChange> GetPendingChanges()
        {
            var changes = pendingChanges.ToList();
            pendingChanges.Clear();
            return changes;
        }

        private bool StateEquals(GameObjectState a, GameObjectState b)
        {
            const float epsilon = 0.0001f;
            return Vector2.Distance(a.Position, b.Position) < epsilon &&
                   Vector2.Distance(a.Velocity, b.Velocity) < epsilon;
        }
    }
}
