using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class NetworkManager : INetworkManager
    {
        private readonly Queue<GameState> stateBuffer;
        private readonly float interpolationDelay = 0.1f; // 100ms delay for smoothing
        private readonly float updateRate = 1f / 20f; // 20Hz update rate
        private float accumulator = 0f;
        private uint currentSequence = 0;
        private Dictionary<uint, PlayerInput> pendingInputs;
        private INetworkTransport transport;
        private GameState latestServerState;
        private Action<GameState> stateUpdateCallback;

        public bool IsServer { get; }
        public bool IsConnected { get; private set; }

        public NetworkManager(bool isServer, INetworkTransport transport)
        {
            if (transport == null) {
                throw new Exception("Transport is null");
            }
            IsServer = isServer;
            this.transport = transport;
            stateBuffer = new Queue<GameState>();
            pendingInputs = new Dictionary<uint, PlayerInput>();

            // Setup network callbacks
            transport.OnDataReceived += HandleNetworkData;
            transport.OnConnectionStatusChanged += HandleConnectionStatus;
        }

        public void SendUpdate(GameState state)
        {
            if (state == null) {
                throw new Exception("State is null");
            }
            if (!IsServer) return;

            accumulator += Time.deltaTime;
            if (accumulator >= updateRate) {
                state.SequenceNumber = ++currentSequence;
                byte[] compressedState = StateCompressor.Compress(state);
                transport.SendReliable(NetworkMessageType.GameState, compressedState);
                accumulator = 0f;
            }
        }

        public void ReceiveUpdate(Action<GameState> updateCallback)
        {
            // Store the callback for later use when processing received states
            stateUpdateCallback = updateCallback;

            // If we have enough states in the buffer, process them immediately
            if (!IsServer && stateBuffer.Count >= 2) {
                var currentTime = DateTime.UtcNow;
                var renderTime = currentTime.AddSeconds(-interpolationDelay);

                while (stateBuffer.Count >= 2) {
                    var oldestState = stateBuffer.Peek();
                    if (oldestState.TimeStamp > renderTime)
                        break;

                    var previousState = stateBuffer.Dequeue();
                    var nextState = stateBuffer.Peek();

                    float t = (float) ( ( renderTime - previousState.TimeStamp ).TotalSeconds /
                                    ( nextState.TimeStamp - previousState.TimeStamp ).TotalSeconds );
                    t = Math.Clamp(t, 0f, 1f);

                    var interpolatedState = GameState.Interpolate(previousState, nextState, t);
                    updateCallback(interpolatedState);
                }
            }
        }

        public void SendPlayerInput(PlayerInput input)
        {
            if (input == null) {
                throw new Exception("Input is null");
            }
            if (IsServer) return;

            input.Sequence = ++currentSequence;
            pendingInputs[input.Sequence] = new PlayerInput(input); // Use deep copy

            byte[] data = input.Serialize();
            transport.SendUnreliable(NetworkMessageType.PlayerInput, data);
        }

        private void HandleNetworkData(NetworkMessageType messageType, byte[] data)
        {
            switch (messageType) {
                case NetworkMessageType.GameState:
                    HandleGameState(data);
                    break;

                case NetworkMessageType.PlayerInput:
                    HandlePlayerInput(data);
                    break;
            }
        }

        private void HandleGameState(byte[] data)
        {
            var state = StateCompressor.Decompress(data);

            if (!IsServer) {
                // Client-side prediction reconciliation
                if (latestServerState == null || state.SequenceNumber > latestServerState.SequenceNumber) {
                    latestServerState = state;
                    ReconcileClientPrediction(state);
                }

                stateBuffer.Enqueue(state);

                // Keep buffer size reasonable
                while (stateBuffer.Count > 10)
                    stateBuffer.Dequeue();

                // If we have a callback registered, try to process states
                if (stateUpdateCallback != null) {
                    ReceiveUpdate(stateUpdateCallback);
                }
            }
        }

        private void HandlePlayerInput(byte[] data)
        {
            if (!IsServer) return;

            var input = PlayerInput.Deserialize(data);
            ProcessPlayerInput(input);
        }

        private void HandleConnectionStatus(ConnectionStatus status)
        {
            IsConnected = status == ConnectionStatus.Connected;

            if (!IsConnected) {
                stateBuffer.Clear();
                pendingInputs.Clear();
                latestServerState = null;
            }
        }

        private void ReconcileClientPrediction(GameState serverState)
        {
            // Remove confirmed inputs
            var playerState = serverState.ObjectStates.Values
                .FirstOrDefault(x => x.LastProcessedInputSequence > 0);

            if (playerState != null) {
                pendingInputs = pendingInputs
                    .Where(x => x.Key > playerState.LastProcessedInputSequence)
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }

        private void ProcessPlayerInput(PlayerInput input)
        {
            // Server-side input processing
            // Implementation depends on game mechanics
        }

        public void Connect(string address, int port)
        {
            transport.Connect(address, port);
        }

        public void Disconnect()
        {
            transport.Disconnect();
            IsConnected = false;
            stateBuffer.Clear();
            pendingInputs.Clear();
            latestServerState = null;
        }
    }

}
