using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace PaperTanksV2Client.GameEngine
{
    public sealed class GameEngineInstance : IDisposable
    {
        private readonly Dictionary<Guid, GameObject> gameObjects;
        private readonly PhysicsSystem physicsSystem;
        private readonly InputManager inputManager;
        private readonly INetworkManager networkManager;
        private GameState currentState;
        private bool isMultiplayer;
        public Guid playerID;
        private Level level;
        public QuadTree quadTree;
        
        private SKTypeface MenuTypeface = null;
        private SKFont MenuFont = null;
        private SKTypeface SecondMenuTypeface = null;
        private SKFont SecondMenuFont = null;

        public GameEngineInstance(bool isMultiplayer = false, INetworkManager networkManager = null, QuadTree quadTree = null, 
            SKTypeface MenuTypeface = null,
        SKFont MenuFont = null,
        SKTypeface SecondMenuTypeface = null,
        SKFont SecondMenuFont = null)
        {
            this.gameObjects = new Dictionary<Guid, GameObject>();
            this.physicsSystem = new PhysicsSystem(PhysicsSystem.MaxVector);
            this.inputManager = new InputManager();
            this.isMultiplayer = isMultiplayer;
            this.networkManager = networkManager;
            this.playerID = Guid.NewGuid();
            this.quadTree = quadTree;
            this.MenuTypeface = MenuTypeface;
            this.MenuFont = MenuFont;
            this.SecondMenuTypeface = SecondMenuTypeface;
            this.SecondMenuFont = SecondMenuFont;
            if (isMultiplayer && networkManager != null) {
                SetupNetworking();
            }
        }

        public void LoadPlayerWithLevel(PlayerData playerData, Level level)
        {
            if (playerData == null) {
                Console.WriteLine("GameEngineInstance - LoadPlayerWithLevel - Player Data Null");
                return;
            }
            if (level == null) {
                Console.WriteLine("GameEngineInstance - LoadPlayerWithLevel - Level Data Null");
                return;
            }
            this.level = level;
            if (this.level != null) {
                if (this.level.gameObjects != null) {
                    foreach (var obj in this.level.gameObjects) {
                        this.gameObjects.Add(Guid.NewGuid(), obj);
                    }
                }
            }
            GameObject player = new Tank(true, playerData.Weapon0, playerData.Weapon1, playerData.Weapon2,
                this.MenuTypeface,
                this.MenuFont,
                this.SecondMenuTypeface,
                this.SecondMenuFont
                ) {
                Bounds = new BoundsData(level.playerPosition, new Vector2Data(200, 200))
            };
            this.playerID = player.Id;
            this.gameObjects.Add(player.Id, player);
        }

        public void Update(float deltaTime)
        {
            // Handle input
            var playerInput = inputManager.GetCurrentInput();

            if (isMultiplayer && !networkManager.IsServer) {
                // In multiplayer client mode, send input to server
                networkManager.SendPlayerInput(playerInput);
            } else {
                // In single player or server mode, process input directly
                ProcessInput(playerInput);
            }

            // Update physics
            physicsSystem.Update(gameObjects.Values, deltaTime);

            // Update game objects
            foreach (GameObject obj in gameObjects.Values) {
                obj.Update(deltaTime);
            }

            // If server or single player, create and send state updates
            if (!isMultiplayer || networkManager.IsServer) {
                currentState = CreateGameState();
                if (isMultiplayer) {
                    networkManager.SendUpdate(currentState);
                }
            }
        }

        private void SetupNetworking()
        {
            networkManager.ReceiveUpdate(state => {
                if (!networkManager.IsServer) {
                    ApplyGameState(state);
                }
            });
        }

        private GameState CreateGameState()
        {
            // Create a snapshot of the current game state
            var state = new GameState {
                TimeStamp = DateTime.UtcNow,
                ObjectStates = new Dictionary<Guid, GameObjectState>()
            };

            foreach (var obj in gameObjects.Values) {
                state.ObjectStates[obj.Id] = obj.GetState();
            }

            return state;
        }

        private void ApplyGameState(GameState state)
        {
            foreach (var objState in state.ObjectStates) {
                if (gameObjects.TryGetValue(objState.Key, out var gameObject)) {
                    gameObject.ApplyState(objState.Value);
                }
            }
        }

        private void ProcessInput(PlayerInput input)
        {
            // TODO: Process player input and update relevant game objects
        }

        // Additional methods for object management
        public void AddObject(GameObject obj) => gameObjects.Add(Guid.NewGuid(), obj);
        public void RemoveObject(Guid id) => gameObjects.Remove(id);
        public GameObject GetObject(Guid id) => gameObjects.GetValueOrDefault(id);
        public List<GameObject> GetObjects() => gameObjects.Values.ToList();

        public void Render(Game game, SKCanvas canvas) {
            if (this.gameObjects != null) {
                foreach (GameObject obj in gameObjects.Values) {
                    obj.Render(game, canvas);
                }
            }
        }

        public void Dispose()
        {
            // TODO: do some clean up here
        }
    }
}
