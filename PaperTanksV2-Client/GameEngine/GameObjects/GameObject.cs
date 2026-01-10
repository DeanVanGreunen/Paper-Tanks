using Newtonsoft.Json;
using PaperTanksV2Client.GameEngine.Server.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class GameObject
    {
        [JsonIgnore]
        public bool deleteMe = false;
        public Guid Id { get; set; }
        [JsonIgnore]
        public Vector2Data Position { get { return this.Bounds.Position;  } }
        [JsonIgnore]
        public Vector2Data Size { get { return this.Bounds.Size;  } }
        [JsonProperty("Velocity")]
        public Vector2Data Velocity { get; set; }
        [JsonProperty("Rotation")]
        public float Rotation { get; set; }
        [JsonProperty("AngularVelocity")]
        public float AngularVelocity { get; set; }
        [JsonProperty("Scale")]
        public Vector2Data Scale { get; set; }
        [JsonProperty("IsStatic")]
        public bool IsStatic { get; set; }
        [JsonProperty("Bounds")]
        public BoundsData Bounds { get; set; }
        [JsonProperty("Health")]
        public float Health { get; set; }
        [JsonProperty("Mass")]
        public float Mass { get; set; }
        [JsonProperty("CustomProperties")]
        public Dictionary<string, object> CustomProperties { get; set; }
        private SKImage imageData;

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<Byte>();
            bytes.AddRange(this.Id.ToByteArray());
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.Health));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.Bounds));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.Velocity));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.Rotation));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.Scale));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.IsStatic));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.Mass));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(this.CustomProperties));
            return bytes.ToArray();
        }
        
        public GameObject FromBytes(byte[] bytes)
        {
            GameObject gameObject = new GameObject();
            int offset = 0;
            byte[] guidBytes = new byte[16];
            Array.Copy(bytes, offset, guidBytes, 0, 16);
            gameObject.Id = new Guid(guidBytes);
            offset += 16;
            gameObject.Health = BinaryHelper.ToSingleBigEndian(bytes, offset);
            offset += 4;
            gameObject.Bounds = BinaryHelper.ToBoundsBigEndian(bytes, offset);
            offset += 4 * 4;
            gameObject.Velocity = BinaryHelper.ToVector2DataBigEndian(bytes, offset);
            offset += 4 * 2;
            gameObject.Rotation = BinaryHelper.ToSingleBigEndian(bytes, offset);
            offset += 4 * 2;
            gameObject.Scale = BinaryHelper.ToVector2DataBigEndian(bytes, offset);
            gameObject.IsStatic = bytes[offset++] == 1;
            gameObject.Mass = BinaryHelper.ToSingleBigEndian(bytes, offset);
            offset += 4;
            gameObject.CustomProperties = BinaryHelper.ToDictionaryBigEndian(bytes, offset);
            return gameObject;
        }

        public void deleteSelf() {
            this.deleteMe = true;
        }

        public bool IsOutOfBounds(float boundsWidth, float boundsHeight)
        {
            float x = this.Bounds.Position.X;
            float y = this.Bounds.Position.Y;
            float w = this.Bounds.Size.X;
            float h = this.Bounds.Size.Y;
    
            // Check if object is completely outside the bounds
            if (x + w < 0 ||           // Completely to the left
                x > boundsWidth ||      // Completely to the right
                y + h < 0 ||           // Completely above
                y > boundsHeight)      // Completely below
            {
                return true;
            }
    
            return false;
        }

        public GameObject()
        {
            Id = Guid.NewGuid();
            this.Bounds = new BoundsData(new Vector2Data(0, 0), new Vector2Data(0, 0));
            Velocity = Vector2Data.Zero;
            Rotation = 0f;
            AngularVelocity = 0f;
            Scale = Vector2Data.One;
            Health = 100f;
            Mass = 1f; // Default mass
            CustomProperties = new Dictionary<string, object>();
        }

        public void MoveBy(float X, float Y)
        {
            this.Bounds.Position.X += X;
            this.Bounds.Position.Y += Y;
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
            if (state == null) return;
            this.Bounds.Position = new Vector2Data(state.Position.X, state.Position.Y);
            Velocity = state.Velocity;
            Rotation = state.Rotation;
            AngularVelocity = state.AngularVelocity;
            Scale = state.Scale;
            Health = state.Health;
            Mass = state.Mass;
            CustomProperties = new Dictionary<string, object>(state.CustomProperties);
        }

        protected virtual ObjectType GetObjectType() { return ObjectType.None; }

        public virtual void Update(GameEngineInstance engine, float deltaTime) { }

        public virtual void HandleCollision(Game game, GameObject other) { }

        public void SetCustomProperty(string key, string value) {
            this.CustomProperties[key] = value;
        }

        public void InternalRender(Game game, SKCanvas canvas) {
            var rect = new SKRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Position.X + this.Bounds.Size.X, this.Bounds.Position.Y + this.Bounds.Size.Y);
            canvas.Save();
            float centerX = rect.MidX;
            float centerY = rect.MidY;
            canvas.RotateDegrees(this.Rotation, centerX, centerY);
            this.Render(game, canvas, centerX, centerY);
            canvas.Restore();
        }

        public virtual void Render(Game game, SKCanvas canvas, float? centerX = null, float? centerY = null) { }
    }
}
