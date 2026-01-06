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
        private GameState currentState;
        private bool isMultiplayer;
        public Guid playerID;
        private Level level;
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
            this.isMultiplayer = isMultiplayer;
            this.playerID = Guid.NewGuid();
            this.MenuTypeface = MenuTypeface;
            this.MenuFont = MenuFont;
            this.SecondMenuTypeface = SecondMenuTypeface;
            this.SecondMenuFont = SecondMenuFont;
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
                        Guid guid = Guid.NewGuid();
                        if (obj is Tank) {
                            if (( obj as Tank ).Weapon0 == null) {
                                ( obj as Tank ).Weapon0 = new Weapon(10, 100);
                                if(( obj as Tank ).IsPlayer) {
                                    Weapon weapon0 = playerData.Weapon0 ?? new Weapon(10, 100);
                                    //weapon0.Bounds = new BoundsData(le, new Vector2Data(8, 8));
                                    this.playerID = guid;
                                }
                            }
                        }
                        this.gameObjects.Add(guid, obj);
                    }
                }
            }

        }

        public void Update(float deltaTime)
        {
            this.gameObjects = this.gameObjects
                .Where(o => o.Value.deleteMe != true)
                .ToDictionary(o => o.Key, o => o.Value);
            foreach(var obj in this.gameObjects)
            {
                obj.Value.Update(deltaTime);
            }
            foreach(KeyValuePair<Guid, GameObject> obj in this.gameObjects)
            {
                foreach(KeyValuePair<Guid, GameObject> obj1 in this.gameObjects) 
                {
                    if (obj.Key == obj1.Key) continue;
                    obj.Value.HandleCollision(obj1.Value);
                }
            }
        }

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
