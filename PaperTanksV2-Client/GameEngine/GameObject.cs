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
        public float AngularVelocity { get; set; }
        public Vector2 Scale { get; set; }
        public bool IsStatic { get; set; }
        public Rectangle Bounds { get; set; }
        public float Health { get; protected set; }
        public float Mass { get; protected set; }
        public CompositeCollider Collider { get; protected set; }
        protected Dictionary<string, object> CustomProperties;

        protected GameObject()
        {
            Id = Guid.NewGuid();
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Rotation = 0f;
            AngularVelocity = 0f;
            Scale = Vector2.One;
            Health = 100f;
            Mass = 1f; // Default mass
            CustomProperties = new Dictionary<string, object>();
            Collider = new CompositeCollider(this);
        }

        public virtual GameObjectState GetState()
        {
            return new GameObjectState {
                Position = this.Position,
                Velocity = this.Velocity,
                Rotation = this.Rotation,
                AngularVelocity = this.AngularVelocity,
                Scale = this.Scale,
                IsActive = true,
                Health = this.Health,
                Mass = this.Mass,
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
            AngularVelocity = state.AngularVelocity;
            Scale = state.Scale;
            Health = state.Health;
            Mass = state.Mass;
            CustomProperties = new Dictionary<string, object>(state.CustomProperties);
            // Update collider transforms after applying new state
            Collider.UpdateTransforms();
        }

        protected abstract ObjectType GetObjectType();

        public abstract void Update(float deltaTime);

        public abstract void HandleCollision(GameObject other);
    }
}
