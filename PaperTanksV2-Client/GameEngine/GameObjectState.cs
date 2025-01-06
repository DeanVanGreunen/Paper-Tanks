using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace PaperTanksV2Client.GameEngine
{
    public class GameObjectState
    {
        // Basic physics properties
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }

        // Object properties
        public bool IsActive { get; set; }
        public float Health { get; set; }
        public ObjectType Type { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; }

        // Animation state
        public string CurrentAnimation { get; set; }
        public float AnimationTime { get; set; }

        // Network specific
        public uint LastProcessedInputSequence { get; set; }
        public DateTime TimeStamp { get; set; }

        public GameObjectState()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Rotation = 0f;
            Scale = Vector2.One;
            IsActive = true;
            Health = 100f;
            CustomProperties = new Dictionary<string, object>();
            TimeStamp = DateTime.UtcNow;
        }

        // Deep copy constructor
        public GameObjectState(GameObjectState other)
        {
            Position = other.Position;
            Velocity = other.Velocity;
            Rotation = other.Rotation;
            Scale = other.Scale;
            IsActive = other.IsActive;
            Health = other.Health;
            Type = other.Type;
            CurrentAnimation = other.CurrentAnimation;
            AnimationTime = other.AnimationTime;
            LastProcessedInputSequence = other.LastProcessedInputSequence;
            TimeStamp = other.TimeStamp;

            // Deep copy custom properties
            CustomProperties = new Dictionary<string, object>(other.CustomProperties);
        }

        // Interpolation between states
        public static GameObjectState Lerp(GameObjectState a, GameObjectState b, float t)
        {
            return new GameObjectState {
                Position = Vector2.Lerp(a.Position, b.Position, t),
                Velocity = Vector2.Lerp(a.Velocity, b.Velocity, t),
                Rotation = MathHelper.LerpAngle(a.Rotation, b.Rotation, t),
                Scale = Vector2.Lerp(a.Scale, b.Scale, t),
                IsActive = b.IsActive, // Use latest active state
                Health = MathHelper.Lerp(a.Health, b.Health, t),
                Type = b.Type, // Enum doesn't interpolate
                CurrentAnimation = b.CurrentAnimation, // Use latest animation
                AnimationTime = MathHelper.Lerp(a.AnimationTime, b.AnimationTime, t),
                LastProcessedInputSequence = b.LastProcessedInputSequence, // Use latest sequence
                TimeStamp = b.TimeStamp, // Use latest timestamp
                CustomProperties = b.CustomProperties // Use latest properties
            };
        }

        // Serialize the state for network transmission
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms)) {
                // Write basic properties
                writer.Write(Position.X);
                writer.Write(Position.Y);
                writer.Write(Velocity.X);
                writer.Write(Velocity.Y);
                writer.Write(Rotation);
                writer.Write(Scale.X);
                writer.Write(Scale.Y);
                writer.Write(IsActive);
                writer.Write(Health);
                writer.Write((int) Type);
                writer.Write(CurrentAnimation ?? string.Empty);
                writer.Write(AnimationTime);
                writer.Write(LastProcessedInputSequence);
                writer.Write(TimeStamp.ToBinary());

                // Write custom properties
                writer.Write(CustomProperties.Count);
                foreach (var kvp in CustomProperties) {
                    writer.Write(kvp.Key);
                    WriteValue(writer, kvp.Value);
                }

                return ms.ToArray();
            }
        }

        // Deserialize the state from network data
        public static GameObjectState Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms)) {
                GameObjectState state = new GameObjectState {
                    Position = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    Velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    Rotation = reader.ReadSingle(),
                    Scale = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    IsActive = reader.ReadBoolean(),
                    Health = reader.ReadSingle(),
                    Type = (ObjectType) reader.ReadInt32(),
                    CurrentAnimation = reader.ReadString(),
                    AnimationTime = reader.ReadSingle(),
                    LastProcessedInputSequence = reader.ReadUInt32(),
                    TimeStamp = DateTime.FromBinary(reader.ReadInt64())
                };

                // Read custom properties
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string key = reader.ReadString();
                    object value = ReadValue(reader);
                    state.CustomProperties[key] = value;
                }

                return state;
            }
        }

        private static void WriteValue(BinaryWriter writer, object value)
        {
            // Write type identifier
            if (value is int intValue) {
                writer.Write((byte) 1);
                writer.Write(intValue);
            } else if (value is float floatValue) {
                writer.Write((byte) 2);
                writer.Write(floatValue);
            } else if (value is string stringValue) {
                writer.Write((byte) 3);
                writer.Write(stringValue);
            } else if (value is bool boolValue) {
                writer.Write((byte) 4);
                writer.Write(boolValue);
            }
            // Add more types as needed
        }

        private static object ReadValue(BinaryReader reader)
        {
            byte type = reader.ReadByte();
            switch (type) {
                case 1: return reader.ReadInt32();
                case 2: return reader.ReadSingle();
                case 3: return reader.ReadString();
                case 4: return reader.ReadBoolean();
                default: throw new Exception($"Unknown type identifier: {type}");
            }
        }
    }

    public enum ObjectType
    {
        Player,
        Enemy,
        Projectile,
        Obstacle,
        Pickup,
        Trigger
        // Add more types as needed
    }

    public static class MathHelper
    {
        public static float LerpAngle(float a, float b, float t)
        {
            float delta = ( ( b - a + 180 ) % 360 ) - 180;
            return a + delta * Math.Clamp(t, 0, 1);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + ( b - a ) * Math.Clamp(t, 0, 1);
        }
    }
}
