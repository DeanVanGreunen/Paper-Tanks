using PaperTanksV2Client.GameEngine.AI;
using PaperTanksV2Client.PageStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace PaperTanksV2Client.GameEngine
{
    public sealed class GameEngineInstance : IDisposable
    {
        private Dictionary<Guid, GameObject> gameObjects;
        private Queue<GameObject> objectsToAdd;  // Add this field
        private GameState currentState;
        private bool isMultiplayer;
        public Guid playerID;
        public Level level { get; protected set; }
        private SKTypeface MenuTypeface = null;
        private SKFont MenuFont = null;
        private SKTypeface SecondMenuTypeface = null;
        private SKFont SecondMenuFont = null;
        private PlayerData playerData;
        public GameEngineInstance(bool isMultiplayer = false, SKTypeface MenuTypeface = null,
        SKFont MenuFont = null,
        SKTypeface SecondMenuTypeface = null,
        SKFont SecondMenuFont = null)
        {
            this.gameObjects = new Dictionary<Guid, GameObject>();
            this.objectsToAdd = new Queue<GameObject>();
            this.isMultiplayer = isMultiplayer;
            this.playerID = Guid.NewGuid();
            this.MenuTypeface = MenuTypeface;
            this.MenuFont = MenuFont;
            this.SecondMenuTypeface = SecondMenuTypeface;
            this.SecondMenuFont = SecondMenuFont;
        }

        public int GetObjectsCount => this.gameObjects.Count();
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
            this.gameObjects.Clear();
            this.objectsToAdd.Clear();
            this.level = level;
            this.playerData = playerData;
            if (this.level != null) {
                if (this.level.gameObjects != null) {
                    foreach (var obj in this.level.gameObjects) {
                        Guid guid = Guid.NewGuid();
                        if (obj is Tank) {
                            if (( obj as Tank ).Weapon0 == null) {
                                ( obj as Tank ).Weapon0 = new Weapon(10, 100);
                                if(( obj as Tank ).IsPlayer) {
                                    Weapon weapon0 = playerData.Weapon0 ?? new Weapon(10, 100);
                                    this.playerID = guid;
                                    ( obj as Tank ).SetPlayerDiedCallback((Game game) => {
                                        this.ReloadLevel(game);
                                    });
                                } else {
                                    ( obj as Tank ).AiAgent = new ChaseAndDodgeAI();
                                }
                            }
                        }
                        if (obj is AmmoPickup) {
                            ( obj as AmmoPickup ).AmmoCount = 20;
                            obj.IsStatic = true;
                        }
                        if (obj is HealthPickup) {
                            ( obj as HealthPickup ).Health = 50;
                            obj.IsStatic = true;
                        }
                        if (obj is Wall) {
                            obj.IsStatic = true;
                        }
                        this.gameObjects.Add(guid, obj);
                    }
                }
            }

        }

        public void ReloadLevel(Game game)
        {
            var campaign = new GamePlayMode();
            campaign.init(game);
            campaign.LoadLevelByName(game, this.level.fileName.Split("\\").Last().Replace(".json", ""), (Game game1) => {
                MainMenuPage mainMenu = new MainMenuPage();
                mainMenu.init(game1);
                mainMenu.SetForceOpen();
                mainMenu.currentMenu = MainMenuEnum.CREDITS;
                game1.states.Clear();
                game1.states.Add(mainMenu);
            });
            game.states.Clear();
            game.states.Add(campaign);
        }

        public void Update(Game game, float deltaTime)
        {
            // Update all objects
            if (this.gameObjects.Values.Count >= 1) {
                var objectsList1 = this.gameObjects.ToList();
                foreach(var obj in objectsList1) // Use ToList() to avoid modification issues
                {
                    obj.Value.Update(this, deltaTime);
                    if (obj.Value.IsOutOfBounds(game.bitmap.Width, game.bitmap.Height)) {
                        obj.Value.deleteMe = true;
                    }
                }
            }

            // Add queued objects
            while (objectsToAdd.Count > 0)
            {
                GameObject obj = objectsToAdd.Dequeue();
                Guid guid = Guid.NewGuid();
                gameObjects.Add(guid, obj);
                obj.Id = guid;
            }
    
            // Remove deleted objects
            this.gameObjects = this.gameObjects
                .Where(o => o.Value.deleteMe != true)
                .ToDictionary(o => o.Key, o => o.Value);
    
            // Handle collisions - create a snapshot to avoid modification during enumeration
            var objectsList = this.gameObjects.ToList();
            foreach(var obj in objectsList)
            {
                foreach(var obj1 in objectsList) 
                {
                    if (obj.Key == obj1.Key) continue;
                    obj.Value.HandleCollision(game, obj1.Value);
                }
            }
        }

        public void AddObject(GameObject obj)
        {
            Guid guid = Guid.NewGuid();
            gameObjects.Add(guid, obj);
            obj.Id = guid;
        }

        public void RemoveObject(Guid id) => gameObjects.Remove(id);
        public GameObject GetObject(Guid id) => gameObjects.GetValueOrDefault(id);
        public Dictionary<Guid, GameObject> GetObjects() => gameObjects;

        public void Render(Game game, SKCanvas canvas) {
            if (this.gameObjects != null) {
                foreach (GameObject obj in gameObjects.Values) {
                    obj.Render(game, canvas);
                }
            }
        }

        public void Dispose()
        {
        }

        public void QueueAddObject(GameObject obj)
        {
            objectsToAdd.Enqueue(obj);
        }

        public List<T> GetObjectByType<T>() where T : GameObject
        {
            return this.gameObjects.Values.OfType<T>().ToList();
        }

        public void DeleteObject(Guid cId)
        {
            this.gameObjects.Remove(cId);
        }
    }
}
