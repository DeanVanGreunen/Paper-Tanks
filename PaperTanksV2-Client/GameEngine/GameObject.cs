using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public abstract class GameObject
    {
        public Guid Id { get; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public bool IsStatic { get; set; }
        public Rectangle Bounds { get; set; }
        public float Health { get; protected set; }
        protected Dictionary<string, object> CustomProperties;

        protected GameObject()
        {
            Id = Guid.NewGuid();
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Rotation = 0f;
            Scale = Vector2.One;
            Health = 100f;
            CustomProperties = new Dictionary<string, object>();
        }

        public virtual GameObjectState GetState()
        {
            return new GameObjectState {
                Position = this.Position,
                Velocity = this.Velocity,
                Rotation = this.Rotation,
                Scale = this.Scale,
                IsActive = true,
                Health = this.Health,
                Type = GetObjectType(),
                CustomProperties = new Dictionary<string, object>(CustomProperties),
                TimeStamp = DateTime.UtcNow
            };
        }

        public virtual void ApplyState(GameObjectState state)
        {
            Position = state.Position;
            Velocity = state.Velocity;
            Rotation = state.Rotation;
            Scale = state.Scale;
            Health = state.Health;
            CustomProperties = new Dictionary<string, object>(state.CustomProperties);
        }

        protected abstract ObjectType GetObjectType();
        public abstract void Update(float deltaTime);
        public abstract void HandleCollision(GameObject other);
    }
}
