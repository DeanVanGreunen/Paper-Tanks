using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public static class StateCompressor
    {
        private const byte CurrentVersion = 1;
        private const float PositionPrecision = 0.01f; // 1cm precision
        private const float VelocityPrecision = 0.01f; // 1cm/s precision
        private const float RotationPrecision = 0.1f;  // 0.1 degree precision
        private const float ScalePrecision = 0.01f;    // 1% precision

        public static byte[] Compress(GameState state)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms)) {
                // Write header
                writer.Write(CurrentVersion);
                writer.Write(state.TimeStamp.ToBinary());
                writer.Write(state.SequenceNumber);

                // Write object states
                writer.Write(state.ObjectStates.Count);
                foreach (var kvp in state.ObjectStates) {
                    CompressObjectState(writer, kvp.Key, kvp.Value);
                }

                // Write world state
                CompressWorldState(writer, state.WorldState);

                return ms.ToArray();
            }
        }

        public static GameState Decompress(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms)) {
                // Read and verify version
                byte version = reader.ReadByte();
                if (version != CurrentVersion) {
                    throw new InvalidOperationException($"Unsupported state version: {version}");
                }

                var state = new GameState {
                    TimeStamp = DateTime.FromBinary(reader.ReadInt64()),
                    SequenceNumber = reader.ReadUInt32(),
                    ObjectStates = new Dictionary<Guid, GameObjectState>(),
                    WorldState = new Dictionary<string, object>()
                };

                // Read object states
                int objectCount = reader.ReadInt32();
                for (int i = 0; i < objectCount; i++) {
                    var (guid, objectState) = DecompressObjectState(reader);
                    state.ObjectStates[guid] = objectState;
                }

                // Read world state
                state.WorldState = DecompressWorldState(reader);

                return state;
            }
        }

        private static void CompressObjectState(BinaryWriter writer, Guid guid, GameObjectState state)
        {
            // Write identifier
            writer.Write(guid.ToByteArray());

            // Compress basic physics properties using fixed-point compression
            CompressVector2(writer, state.Position, PositionPrecision);
            CompressVector2(writer, state.Velocity, VelocityPrecision);
            writer.Write(CompressFloat(state.Rotation, RotationPrecision));
            writer.Write(CompressFloat(state.AngularVelocity, RotationPrecision));
            writer.Write(state.Mass);
            CompressVector2(writer, state.Scale, ScalePrecision);

            // Write object properties
            writer.Write(state.IsActive);
            writer.Write(CompressFloat(state.Health, 0.1f));
            writer.Write((byte) state.Type);

            // Write animation state
            writer.Write(state.CurrentAnimation ?? string.Empty);
            writer.Write(state.AnimationTime);
            writer.Write(state.LastProcessedInputSequence);

            // Write custom properties
            CompressCustomProperties(writer, state.CustomProperties);
        }

        private static (Guid, GameObjectState) DecompressObjectState(BinaryReader reader)
        {
            var guid = new Guid(reader.ReadBytes(16));
            var state = new GameObjectState {
                Position = DecompressVector2(reader, PositionPrecision),
                Velocity = DecompressVector2(reader, VelocityPrecision),
                Rotation = DecompressFloat(reader.ReadInt32(), RotationPrecision),
                AngularVelocity = DecompressFloat(reader.ReadInt32(), RotationPrecision),
                Mass = reader.ReadSingle(),
                Scale = DecompressVector2(reader, ScalePrecision),
                IsActive = reader.ReadBoolean(),
                Health = DecompressFloat(reader.ReadInt32(), 0.1f),
                Type = (ObjectType) reader.ReadByte(),
                CurrentAnimation = reader.ReadString(),
                AnimationTime = reader.ReadSingle(),
                LastProcessedInputSequence = reader.ReadUInt32(),
                CustomProperties = DecompressCustomProperties(reader)
            };

            return (guid, state);
        }

        private static void CompressVector2(BinaryWriter writer, Vector2 vector, float precision)
        {
            writer.Write(CompressFloat(vector.X, precision));
            writer.Write(CompressFloat(vector.Y, precision));
        }

        private static Vector2 DecompressVector2(BinaryReader reader, float precision)
        {
            return new Vector2(
                DecompressFloat(reader.ReadInt32(), precision),
                DecompressFloat(reader.ReadInt32(), precision)
            );
        }

        private static int CompressFloat(float value, float precision)
        {
            return (int) ( value / precision );
        }

        private static float DecompressFloat(int value, float precision)
        {
            return value * precision;
        }

        private static void CompressCustomProperties(BinaryWriter writer, Dictionary<string, object> properties)
        {
            writer.Write(properties.Count);
            foreach (var kvp in properties) {
                writer.Write(kvp.Key);
                CompressValue(writer, kvp.Value);
            }
        }

        private static Dictionary<string, object> DecompressCustomProperties(BinaryReader reader)
        {
            var properties = new Dictionary<string, object>();
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++) {
                string key = reader.ReadString();
                object value = DecompressValue(reader);
                properties[key] = value;
            }

            return properties;
        }

        private static void CompressWorldState(BinaryWriter writer, Dictionary<string, object> worldState)
        {
            writer.Write(worldState.Count);
            foreach (var kvp in worldState) {
                writer.Write(kvp.Key);
                CompressValue(writer, kvp.Value);
            }
        }

        private static Dictionary<string, object> DecompressWorldState(BinaryReader reader)
        {
            var worldState = new Dictionary<string, object>();
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++) {
                string key = reader.ReadString();
                object value = DecompressValue(reader);
                worldState[key] = value;
            }

            return worldState;
        }

        private static void CompressValue(BinaryWriter writer, object value)
        {
            switch (value) {
                case null:
                    writer.Write((byte) ValueType.Null);
                    break;
                case bool boolValue:
                    writer.Write((byte) ValueType.Boolean);
                    writer.Write(boolValue);
                    break;
                case int intValue:
                    writer.Write((byte) ValueType.Integer);
                    writer.Write(intValue);
                    break;
                case float floatValue:
                    writer.Write((byte) ValueType.Float);
                    writer.Write(floatValue);
                    break;
                case string stringValue:
                    writer.Write((byte) ValueType.String);
                    writer.Write(stringValue);
                    break;
                case Vector2 vector2Value:
                    writer.Write((byte) ValueType.Vector2);
                    CompressVector2(writer, vector2Value, PositionPrecision);
                    break;
                default:
                    throw new ArgumentException($"Unsupported value type: {value.GetType()}");
            }
        }

        private static object DecompressValue(BinaryReader reader)
        {
            var type = (ValueType) reader.ReadByte();
            return type switch
            {
                ValueType.Null => null,
                ValueType.Boolean => reader.ReadBoolean(),
                ValueType.Integer => reader.ReadInt32(),
                ValueType.Float => reader.ReadSingle(),
                ValueType.String => reader.ReadString(),
                ValueType.Vector2 => DecompressVector2(reader, PositionPrecision),
                _ => throw new ArgumentException($"Unsupported value type: {type}")
            };
        }

        private enum ValueType : byte
        {
            Null = 0,
            Boolean = 1,
            Integer = 2,
            Float = 3,
            String = 4,
            Vector2 = 5
        }
    }
}
