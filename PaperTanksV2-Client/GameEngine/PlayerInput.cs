using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class PlayerInput
    {
        // Sequence number for network synchronization
        public uint Sequence { get; set; }

        // Timestamp when input was created
        public DateTime Timestamp { get; set; }

        // Movement vector (normalized)
        public Vector2 Movement { get; set; }

        // Mouse/Aim position in world coordinates
        public Vector2 AimPosition { get; set; }

        // Button states
        public HashSet<GameAction> Actions { get; set; }

        // Input state for continuous actions (0 to 1)
        public Dictionary<GameAction, float> AnalogActions { get; set; }

        public PlayerInput()
        {
            Timestamp = DateTime.UtcNow;
            Movement = Vector2.Zero;
            AimPosition = Vector2.Zero;
            Actions = new HashSet<GameAction>();
            AnalogActions = new Dictionary<GameAction, float>();
        }

        // Deep copy constructor
        public PlayerInput(PlayerInput other)
        {
            Sequence = other.Sequence;
            Timestamp = other.Timestamp;
            Movement = other.Movement;
            AimPosition = other.AimPosition;
            Actions = new HashSet<GameAction>(other.Actions);
            AnalogActions = new Dictionary<GameAction, float>(other.AnalogActions);
        }

        // Serialization for network transmission
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms)) {
                writer.Write(Sequence);
                writer.Write(Timestamp.ToBinary());
                writer.Write(Movement.X);
                writer.Write(Movement.Y);
                writer.Write(AimPosition.X);
                writer.Write(AimPosition.Y);

                // Write discrete actions
                writer.Write(Actions.Count);
                foreach (var action in Actions) {
                    writer.Write((int) action);
                }

                // Write analog actions
                writer.Write(AnalogActions.Count);
                foreach (var kvp in AnalogActions) {
                    writer.Write((int) kvp.Key);
                    writer.Write(kvp.Value);
                }

                return ms.ToArray();
            }
        }

        // Deserialization from network data
        public static PlayerInput Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms)) {
                var input = new PlayerInput {
                    Sequence = reader.ReadUInt32(),
                    Timestamp = DateTime.FromBinary(reader.ReadInt64()),
                    Movement = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    AimPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle())
                };

                // Read discrete actions
                int actionCount = reader.ReadInt32();
                for (int i = 0; i < actionCount; i++) {
                    input.Actions.Add((GameAction) reader.ReadInt32());
                }

                // Read analog actions
                int analogCount = reader.ReadInt32();
                for (int i = 0; i < analogCount; i++) {
                    var action = (GameAction) reader.ReadInt32();
                    var value = reader.ReadSingle();
                    input.AnalogActions[action] = value;
                }

                return input;
            }
        }
    }
}
