using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class PlayerInput
    {
        // Core networking properties
        public uint Sequence { get; set; }
        public Guid PlayerId { get; set; }
        public DateTime Timestamp { get; set; }

        // Input state properties
        public Vector2 Movement { get; set; }
        public Vector2 AimPosition { get; set; }
        public HashSet<InputAction> Actions { get; set; }
        public Dictionary<InputAction, float> AnalogValues { get; set; }

        // For prediction and reconciliation
        public bool IsPredicted { get; set; }
        public uint LastProcessedSequence { get; set; }

        public PlayerInput()
        {
            PlayerId = Guid.Empty;
            Timestamp = DateTime.UtcNow;
            Movement = Vector2.Zero;
            AimPosition = Vector2.Zero;
            Actions = new HashSet<InputAction>();
            AnalogValues = new Dictionary<InputAction, float>();
        }

        // Deep copy constructor for prediction system
        public PlayerInput(PlayerInput other)
        {
            Sequence = other.Sequence;
            PlayerId = other.PlayerId;
            Timestamp = other.Timestamp;
            Movement = other.Movement;
            AimPosition = other.AimPosition;
            Actions = new HashSet<InputAction>(other.Actions);
            AnalogValues = new Dictionary<InputAction, float>(other.AnalogValues);
            IsPredicted = other.IsPredicted;
            LastProcessedSequence = other.LastProcessedSequence;
        }

        // Creates a predicted input based on the last known input
        public static PlayerInput CreatePredicted(PlayerInput lastInput)
        {
            var predicted = new PlayerInput(lastInput) {
                Sequence = lastInput.Sequence + 1,
                Timestamp = DateTime.UtcNow,
                IsPredicted = true
            };
            return predicted;
        }

        // Interpolation between two inputs for smoothing
        public static PlayerInput Lerp(PlayerInput a, PlayerInput b, float t)
        {
            var interpolated = new PlayerInput {
                Sequence = b.Sequence,
                PlayerId = b.PlayerId,
                Timestamp = DateTime.UtcNow,
                Movement = Vector2.Lerp(a.Movement, b.Movement, t),
                AimPosition = Vector2.Lerp(a.AimPosition, b.AimPosition, t),
                LastProcessedSequence = b.LastProcessedSequence
            };

            // For discrete actions, use the most recent state
            interpolated.Actions = new HashSet<InputAction>(b.Actions);

            // Interpolate analog values
            interpolated.AnalogValues = new Dictionary<InputAction, float>();
            foreach (var kvp in b.AnalogValues) {
                float startValue = a.AnalogValues.TryGetValue(kvp.Key, out float aValue) ? aValue : 0f;
                interpolated.AnalogValues[kvp.Key] = MathHelper.Lerp(startValue, kvp.Value, t);
            }

            return interpolated;
        }

        // Serialization for network transmission
        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms)) {
                // Write core properties
                writer.Write(Sequence);
                writer.Write(PlayerId.ToByteArray());
                writer.Write(Timestamp.ToBinary());

                // Write movement data
                writer.Write(Movement.X);
                writer.Write(Movement.Y);
                writer.Write(AimPosition.X);
                writer.Write(AimPosition.Y);

                // Write discrete actions
                writer.Write(Actions.Count);
                foreach (var action in Actions) {
                    writer.Write((byte) action);
                }

                // Write analog values
                writer.Write(AnalogValues.Count);
                foreach (var kvp in AnalogValues) {
                    writer.Write((byte) kvp.Key);
                    writer.Write(kvp.Value);
                }

                // Write prediction data
                writer.Write(LastProcessedSequence);
                writer.Write(IsPredicted);

                return ms.ToArray();
            }
        }

        // Deserialization from network data
        public static PlayerInput Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms)) {
                var input = new PlayerInput {
                    Sequence = reader.ReadUInt32(),
                    PlayerId = new Guid(reader.ReadBytes(16)),
                    Timestamp = DateTime.FromBinary(reader.ReadInt64()),
                    Movement = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    AimPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle())
                };

                // Read discrete actions
                int actionCount = reader.ReadInt32();
                for (int i = 0; i < actionCount; i++) {
                    input.Actions.Add((InputAction) reader.ReadByte());
                }

                // Read analog values
                int analogCount = reader.ReadInt32();
                for (int i = 0; i < analogCount; i++) {
                    var action = (InputAction) reader.ReadByte();
                    var value = reader.ReadSingle();
                    input.AnalogValues[action] = value;
                }

                // Read prediction data
                input.LastProcessedSequence = reader.ReadUInt32();
                input.IsPredicted = reader.ReadBoolean();

                return input;
            }
        }

        // Delta compression - only sends changes from the last input
        public byte[] SerializeDelta(PlayerInput lastInput)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms)) {
                // Always write core networking data
                writer.Write(Sequence);
                writer.Write(PlayerId.ToByteArray());
                writer.Write(Timestamp.ToBinary());

                // Write movement only if changed
                bool movementChanged = Movement != lastInput.Movement;
                writer.Write(movementChanged);
                if (movementChanged) {
                    writer.Write(Movement.X);
                    writer.Write(Movement.Y);
                }

                bool aimChanged = AimPosition != lastInput.AimPosition;
                writer.Write(aimChanged);
                if (aimChanged) {
                    writer.Write(AimPosition.X);
                    writer.Write(AimPosition.Y);
                }

                // Write only changed actions
                var addedActions = Actions.Except(lastInput.Actions).ToList();
                var removedActions = lastInput.Actions.Except(Actions).ToList();
                writer.Write(addedActions.Count);
                writer.Write(removedActions.Count);
                foreach (var action in addedActions) writer.Write((byte) action);
                foreach (var action in removedActions) writer.Write((byte) action);

                // Write only changed analog values
                var changedAnalog = AnalogValues
                    .Where(kvp => !lastInput.AnalogValues.TryGetValue(kvp.Key, out float lastValue)
                                 || !MathHelper.NearlyEquals(lastValue, kvp.Value))
                    .ToList();

                writer.Write(changedAnalog.Count());
                foreach (var kvp in changedAnalog) {
                    writer.Write((byte) kvp.Key);
                    writer.Write(kvp.Value);
                }

                return ms.ToArray();
            }
        }

        // Apply delta-compressed input
        public static PlayerInput DeserializeDelta(byte[] data, PlayerInput baseInput)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms)) {
                var input = new PlayerInput(baseInput);

                input.Sequence = reader.ReadUInt32();
                input.PlayerId = new Guid(reader.ReadBytes(16));
                input.Timestamp = DateTime.FromBinary(reader.ReadInt64());

                if (reader.ReadBoolean()) {
                    input.Movement = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                }

                if (reader.ReadBoolean()) {
                    input.AimPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                }

                int addedCount = reader.ReadInt32();
                int removedCount = reader.ReadInt32();

                for (int i = 0; i < addedCount; i++)
                    input.Actions.Add((InputAction) reader.ReadByte());

                for (int i = 0; i < removedCount; i++)
                    input.Actions.Remove((InputAction) reader.ReadByte());

                int analogCount = reader.ReadInt32();
                for (int i = 0; i < analogCount; i++) {
                    var action = (InputAction) reader.ReadByte();
                    var value = reader.ReadSingle();
                    input.AnalogValues[action] = value;
                }

                return input;
            }
        }
    }

    public enum InputAction : byte
    {
        None = 0,
        Action0 = 1,
        Action1 = 2,
        Action2 = 3,
        Action3 = 4,
        Action4 = 5,
        Action5 = 6,
        Action6 = 7,
        Action7 = 8,
        Action8 = 9,
        Action9 = 10
    }
}
