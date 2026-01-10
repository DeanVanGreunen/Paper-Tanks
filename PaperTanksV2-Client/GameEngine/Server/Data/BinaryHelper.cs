using System;
using System.Collections.Generic;
using System.Linq;

namespace PaperTanksV2Client.GameEngine.Server.Data
{
    public static class BinaryHelper
    {
        /// <summary>
        /// Converts an BoundsData to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(BoundsData value)
        {
            if (value == null) return Array.Empty<byte>();
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value.Position.X));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value.Position.Y));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value.Size.X));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value.Size.Y));
            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a big-endian byte array to an array of ClientConnection objects
        /// Note: This creates ClientConnections without Sockets, as sockets cannot be serialized
        /// </summary>
        public static ClientConnection[] ToClientConnectionArrayBigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
                throw new ArgumentException("Invalid byte array");
            int offset = startIndex;
            int count = ToInt32BigEndian(bytes, offset);
            offset += 4;
            if (count == 0)
                return Array.Empty<ClientConnection>();

            ClientConnection[] connections = new ClientConnection[count];
            for (int i = 0; i < count; i++) {
                if (bytes.Length < offset + 17)
                    throw new ArgumentException(
                        $"Invalid byte array: not enough data for ClientConnection at index {i}");

                connections[i] = ToClientConnectionBigEndian(bytes, offset);
                offset += 17;
            }

            return connections;
        }

        /// <summary>
        /// Converts an array of ClientConnection to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(ClientConnection[] values)
        {
            if (values == null) return GetBytesBigEndian(0);
            List<byte> bytes = new List<byte>();
            bytes.AddRange(GetBytesBigEndian(values.Length));
            foreach (var value in values) {
                if (value != null) {
                    bytes.AddRange(GetBytesBigEndian(value));
                } else {
                    bytes.AddRange(new byte[17]);
                }
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Converts an Movement to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(Movement value)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinaryHelper.GetBytesBigEndian((int) value.input));
            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a big-endian byte array to a Movement object
        /// </summary>
        public static Movement ToMovementBigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
                throw new ArgumentException("Invalid byte array");
            PlayerInput input = BinaryHelper.ToPlayerInputBigEndian(bytes, startIndex);
            return new Movement { input = input };
        }

        /// <summary>
        /// Converts an BoundsData to a big-endian byte array
        /// </summary>
        public static BoundsData ToBoundsBigEndian(byte[] bytes, int startIndex = 0)
        {
            int offset = startIndex;
            float X1 = ToSingleBigEndian(bytes, offset);
            offset += 4;
            float Y1 = ToSingleBigEndian(bytes, offset);
            offset += 4;
            float X2 = ToSingleBigEndian(bytes, offset);
            offset += 4;
            float Y2 = ToSingleBigEndian(bytes, offset);
            return new BoundsData(new Vector2Data(X1, Y1), new Vector2Data(X2, Y2));
        }

        /// <summary>
        /// Converts a Dictionary<string, object> to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(Dictionary<string, object> dict)
        {
            List<byte> bytes = new List<byte>();

            if (dict == null || dict.Count == 0) {
                // Write count of 0 for null/empty dictionary
                bytes.AddRange(BinaryHelper.GetBytesBigEndian(0).Reverse().ToArray());
                return bytes.ToArray();
            }

            // Write count
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(dict.Count).ToArray().Reverse());

            foreach (var kvp in dict) {
                if (kvp.Key == null) {
                    Console.WriteLine("WARNING: Skipping null key in dictionary");
                    continue;
                }

                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(kvp.Key);
                bytes.AddRange(BinaryHelper.GetBytesBigEndian(keyBytes.Length));
                bytes.AddRange(keyBytes);
                byte[] valueBytes = SerializeObject(kvp.Value);
                bytes.AddRange(valueBytes);
            }

            return bytes.ToArray();
        }

        private static byte[] SerializeObject(object value)
        {
            List<byte> bytes = new List<byte>();

            if (value == null) {
                bytes.Add(0); // Type: null
            } else if (value is int intValue) {
                bytes.Add(1); // Type: int
                bytes.AddRange(GetBytesBigEndian(intValue));
            } else if (value is float floatValue) {
                bytes.Add(2); // Type: float
                bytes.AddRange(GetBytesBigEndian(floatValue));
            } else if (value is double doubleValue) {
                bytes.Add(3); // Type: double
                bytes.AddRange(GetBytesBigEndian(doubleValue));
            } else if (value is long longValue) {
                bytes.Add(4); // Type: long
                bytes.AddRange(GetBytesBigEndian(longValue));
            } else if (value is short shortValue) {
                bytes.Add(5); // Type: short
                bytes.AddRange(GetBytesBigEndian(shortValue));
            } else if (value is string stringValue) {
                bytes.Add(6); // Type: string
                byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(stringValue);
                bytes.AddRange(GetBytesBigEndian(stringBytes.Length));
                bytes.AddRange(stringBytes);
            } else if (value is bool boolValue) {
                bytes.Add(7); // Type: bool
                bytes.Add((byte) ( boolValue ? 1 : 0 ));
            } else if (value is byte byteValue) {
                bytes.Add(8); // Type: byte
                bytes.Add(byteValue);
            } else {
                // Unsupported type - write as null
                bytes.Add(0);
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a big-endian byte array to Dictionary<string, object>
        /// </summary>
        public static Dictionary<string, object> ToDictionaryBigEndian(byte[] bytes, int startIndex = 0)
        {
            try {
                int offset = startIndex;

                // Safety check for buffer size
                if (bytes == null || bytes.Length < offset + 4) {
                    Console.WriteLine($"[ToDictionary] Buffer too small at offset {startIndex}");
                    return new Dictionary<string, object>();
                }

                // Read dictionary count
                int count = ToInt32BigEndian(bytes, offset);
                Console.WriteLine($"[ToDictionary] Reading dictionary at offset {offset}, count={count}");
                offset += 4;

                // If count is 0, return empty dictionary
                if (count == 0) {
                    return new Dictionary<string, object>();
                }

                // Safety check for count
                if (count < 0 || count > 10000) {
                    Console.WriteLine($"[ToDictionary] ERROR: Invalid count {count}");
                    return new Dictionary<string, object>();
                }

                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                for (int i = 0; i < count; i++) {
                    Console.WriteLine($"[ToDictionary] Reading item {i + 1}/{count} at offset {offset}");

                    // Safety check before reading key length
                    if (bytes.Length < offset + 4) {
                        Console.WriteLine($"[ToDictionary] ERROR: Not enough bytes for key length");
                        return dictionary;
                    }

                    // Read key length
                    int keyLength = ToInt32BigEndian(bytes, offset);
                    Console.WriteLine($"[ToDictionary] Key length: {keyLength}");
                    offset += 4;

                    // THIS IS THE CRITICAL CHECK THAT PREVENTS YOUR ERROR
                    if (keyLength < 0) {
                        Console.WriteLine($"[ToDictionary] ERROR: Negative key length {keyLength}");
                        return dictionary;
                    }

                    if (keyLength > bytes.Length - offset) {
                        Console.WriteLine(
                            $"[ToDictionary] ERROR: Key length {keyLength} exceeds remaining buffer {bytes.Length - offset}");
                        return dictionary;
                    }

                    if (keyLength > 1000) {
                        Console.WriteLine($"[ToDictionary] ERROR: Key length {keyLength} too large");
                        return dictionary;
                    }

                    string key = System.Text.Encoding.UTF8.GetString(bytes, offset, keyLength);
                    Console.WriteLine($"[ToDictionary] Key: {key}");
                    offset += keyLength;
                    object value = DeserializeObject(bytes, ref offset);
                    Console.WriteLine($"[ToDictionary] Value type: {value?.GetType().Name ?? "null"}");
                    dictionary[key] = value;
                }

                Console.WriteLine($"[ToDictionary] Successfully read dictionary with {dictionary.Count} entries");
                return dictionary;
            } catch (Exception ex) {
                Console.WriteLine($"[ToDictionary] EXCEPTION at offset {startIndex}: {ex.Message}");
                Console.WriteLine($"[ToDictionary] Stack: {ex.StackTrace}");
                return new Dictionary<string, object>();
            }
        }

        private static object DeserializeObject(byte[] bytes, ref int offset)
        {
            try {
                // Safety check
                if (bytes == null || offset >= bytes.Length) {
                    Console.WriteLine(
                        $"[DeserializeObject] ERROR: Invalid offset {offset}, buffer length {bytes?.Length ?? 0}");
                    return null;
                }

                byte typeId = bytes[offset++];
                Console.WriteLine($"[DeserializeObject] TypeId: {typeId} at offset {offset - 1}");

                switch (typeId) {
                    case 0: // null
                        return null;

                    case 1: // int
                        if (offset + 4 > bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for int");
                            return null;
                        }

                        int intValue = ToInt32BigEndian(bytes, offset);
                        offset += 4;
                        Console.WriteLine($"[DeserializeObject] int value: {intValue}");
                        return intValue;

                    case 2: // float
                        if (offset + 4 > bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for float");
                            return null;
                        }

                        float floatValue = ToSingleBigEndian(bytes, offset);
                        offset += 4;
                        Console.WriteLine($"[DeserializeObject] float value: {floatValue}");
                        return floatValue;

                    case 3: // double
                        if (offset + 8 > bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for double");
                            return null;
                        }

                        double doubleValue = ToDoubleBigEndian(bytes, offset);
                        offset += 8;
                        Console.WriteLine($"[DeserializeObject] double value: {doubleValue}");
                        return doubleValue;

                    case 4: // long
                        if (offset + 8 > bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for long");
                            return null;
                        }

                        long longValue = ToInt64BigEndian(bytes, offset);
                        offset += 8;
                        Console.WriteLine($"[DeserializeObject] long value: {longValue}");
                        return longValue;

                    case 5: // short
                        if (offset + 2 > bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for short");
                            return null;
                        }

                        short shortValue = ToInt16BigEndian(bytes, offset);
                        offset += 2;
                        Console.WriteLine($"[DeserializeObject] short value: {shortValue}");
                        return shortValue;

                    case 6: // string
                        if (offset + 4 > bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for string length");
                            return null;
                        }

                        int stringLength = ToInt32BigEndian(bytes, offset);
                        Console.WriteLine($"[DeserializeObject] string length: {stringLength}");
                        offset += 4;

                        // CRITICAL VALIDATION
                        if (stringLength < 0) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Negative string length {stringLength}");
                            return null;
                        }

                        if (stringLength > bytes.Length - offset) {
                            Console.WriteLine(
                                $"[DeserializeObject] ERROR: String length {stringLength} exceeds buffer (remaining: {bytes.Length - offset})");
                            return null;
                        }

                        if (stringLength > 100000) // Reasonable max string size
                        {
                            Console.WriteLine($"[DeserializeObject] ERROR: String length {stringLength} too large");
                            return null;
                        }

                        string stringValue = System.Text.Encoding.UTF8.GetString(bytes, offset, stringLength);
                        offset += stringLength;
                        Console.WriteLine(
                            $"[DeserializeObject] string value: {stringValue.Substring(0, Math.Min(50, stringValue.Length))}...");
                        return stringValue;

                    case 7: // bool
                        if (offset >= bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for bool");
                            return null;
                        }

                        bool boolValue = bytes[offset++] == 1;
                        Console.WriteLine($"[DeserializeObject] bool value: {boolValue}");
                        return boolValue;

                    case 8: // byte
                        if (offset >= bytes.Length) {
                            Console.WriteLine($"[DeserializeObject] ERROR: Not enough bytes for byte");
                            return null;
                        }

                        byte byteValue = bytes[offset++];
                        Console.WriteLine($"[DeserializeObject] byte value: {byteValue}");
                        return byteValue;

                    default:
                        Console.WriteLine($"[DeserializeObject] ERROR: Unknown typeId {typeId}");
                        return null;
                }
            } catch (Exception ex) {
                Console.WriteLine($"[DeserializeObject] EXCEPTION at offset {offset}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Converts an Vector2Data to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(Vector2Data value)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value?.X ?? 0));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value?.Y ?? 0));
            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a big-endian byte array to Vector2Data
        /// </summary>
        public static Vector2Data ToVector2DataBigEndian(byte[] bytes, int startIndex = 0)
        {
            Vector2Data data = new Vector2Data(0, 0);
            data.X = ToSingleBigEndian(bytes, startIndex + 0);
            data.Y = ToSingleBigEndian(bytes, startIndex + 4);
            return data;
        }

        /// <summary>
        /// Converts a big-endian byte array to bool
        /// </summary>
        public static bool ToBoolBigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes.Length == 0 || bytes.Length > 1) return false;
            return bytes[0] != 0;
        }

        /// <summary>
        /// Converts an Int32 to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Converts an Bool to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Converts an Int16 to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Converts an Int64 to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Converts a float to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Converts a double to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Converts a big-endian byte array to Int32
        /// </summary>
        public static int ToInt32BigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
                throw new ArgumentException("Invalid byte array");

            byte[] workingBytes = new byte[4];
            Array.Copy(bytes, startIndex, workingBytes, 0, 4);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(workingBytes);
            }

            return BitConverter.ToInt32(workingBytes, 0);
        }

        /// <summary>
        /// Converts a big-endian byte array to Int16
        /// </summary>
        public static short ToInt16BigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 2)
                throw new ArgumentException("Invalid byte array");

            byte[] workingBytes = new byte[2];
            Array.Copy(bytes, startIndex, workingBytes, 0, 2);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(workingBytes);
            }

            return BitConverter.ToInt16(workingBytes, 0);
        }

        /// <summary>
        /// Converts a big-endian byte array to Int64
        /// </summary>
        public static long ToInt64BigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 8)
                throw new ArgumentException("Invalid byte array");

            byte[] workingBytes = new byte[8];
            Array.Copy(bytes, startIndex, workingBytes, 0, 8);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(workingBytes);
            }

            return BitConverter.ToInt64(workingBytes, 0);
        }

        /// <summary>
        /// Converts a big-endian byte array to float
        /// </summary>
        public static float ToSingleBigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
                throw new ArgumentException("Invalid byte array");

            byte[] workingBytes = new byte[4];
            Array.Copy(bytes, startIndex, workingBytes, 0, 4);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(workingBytes);
            }

            return BitConverter.ToSingle(workingBytes, 0);
        }

        /// <summary>
        /// Converts a big-endian byte array to double
        /// </summary>
        public static double ToDoubleBigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 8)
                throw new ArgumentException("Invalid byte array");

            byte[] workingBytes = new byte[8];
            Array.Copy(bytes, startIndex, workingBytes, 0, 8);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(workingBytes);
            }

            return BitConverter.ToDouble(workingBytes, 0);
        }

        public static PlayerInput ToPlayerInputBigEndian(byte[] bytes, Int32 startIndex)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
                throw new ArgumentException("Invalid byte array");

            byte[] workingBytes = new byte[4];
            Array.Copy(bytes, startIndex, workingBytes, 0, 4);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(workingBytes);
            }

            int value = BitConverter.ToInt32(workingBytes, 0);

            if (!Enum.IsDefined(typeof(PlayerInput), value))
                return PlayerInput.DO_NOTHING;

            return (PlayerInput) value;
        }

        /// <summary>
        /// Converts a ClientConnection to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(ClientConnection value)
        {
            if (value == null) return Array.Empty<byte>();
            List<byte> bytes = new List<byte>();
            bytes.AddRange(value.Id.ToByteArray());
            bytes.Add((byte) ( value.isReady ? 1 : 0 ));
            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a big-endian byte array to a ClientConnection object
        /// Note: This creates a ClientConnection without a Socket, as sockets cannot be serialized
        /// </summary>
        public static ClientConnection ToClientConnectionBigEndian(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 17)
                throw new ArgumentException("Invalid byte array");
            int offset = startIndex;
            byte[] guidBytes = new byte[16];
            Array.Copy(bytes, offset, guidBytes, 0, 16);
            Guid id = new Guid(guidBytes);
            offset += 16;
            bool isReady = bytes[offset] == 1;
            ClientConnection connection = new ClientConnection(null) {
                isReady = isReady,
                Id = id,
            };
            return connection;
        }

        public static GameObjectArray ToGameObjectArray(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
                throw new ArgumentException("Invalid byte array");
            int offset = startIndex;
            int count = ToInt32BigEndian(bytes, offset);
            offset += 4;
            if (count == 0)
                return new GameObjectArray(new List<GameObject>());
            List<GameObject> gameObjects = new List<GameObject>();
            for (int i = 0; i < count; i++) {
                GameObject obj = ToGameObjectBigEndian(bytes, ref offset);
                gameObjects.Add(obj);
            }

            return new GameObjectArray(gameObjects);
        }

        /// <summary>
        /// Converts a big-endian byte array to a GameObject object
        /// </summary>
        public static GameObject ToGameObjectBigEndian(byte[] bytes, ref int offset)
        {
            try {
                Console.WriteLine($"[ToGameObject] Starting at offset {offset}");

                if (bytes == null || bytes.Length < offset + 16)
                    throw new ArgumentException("Invalid byte array");

                // Read Id (Guid - 16 bytes)
                byte[] guidBytes = new byte[16];
                Array.Copy(bytes, offset, guidBytes, 0, 16);
                Guid id = new Guid(guidBytes);
                offset += 16;
                Console.WriteLine($"[ToGameObject] ID: {id}, offset now {offset}");

                // Read Health (FLOAT - 4 bytes) ← FIXED
                float health = ToSingleBigEndian(bytes, offset);
                offset += 4;
                Console.WriteLine($"[ToGameObject] Health: {health}, offset now {offset}");

                // Read Bounds (BoundsData - 16 bytes: 4 floats)
                BoundsData bounds = ToBoundsBigEndian(bytes, offset);
                offset += 16;
                Console.WriteLine($"[ToGameObject] Bounds read, offset now {offset}");

                // Read Velocity (Vector2Data - 8 bytes: 2 floats)
                Vector2Data velocity = ToVector2DataBigEndian(bytes, offset);
                offset += 8;
                Console.WriteLine($"[ToGameObject] Velocity read, offset now {offset}");

                // Read Rotation (float - 4 bytes)
                float rotation = ToSingleBigEndian(bytes, offset);
                offset += 4;
                Console.WriteLine($"[ToGameObject] Rotation: {rotation}, offset now {offset}");

                // Read Scale (Vector2Data - 8 bytes: 2 floats)
                Vector2Data scale = ToVector2DataBigEndian(bytes, offset);
                offset += 8;
                Console.WriteLine($"[ToGameObject] Scale read, offset now {offset}");

                // Read IsStatic (bool - 1 BYTE) ← FIXED
                bool isStatic = bytes[offset++] == 1;
                Console.WriteLine($"[ToGameObject] IsStatic: {isStatic}, offset now {offset}");

                // Read Mass (float - 4 bytes)
                float mass = ToSingleBigEndian(bytes, offset);
                offset += 4;
                Console.WriteLine($"[ToGameObject] Mass: {mass}, offset now {offset}");

                // Read CustomProperties (Dictionary<string, object> - variable length)
                Console.WriteLine($"[ToGameObject] About to read dictionary at offset {offset}");
                Dictionary<string, object> customProperties = ToDictionaryBigEndian(bytes, offset);
                Console.WriteLine($"[ToGameObject] Dictionary has {customProperties?.Count ?? 0} entries");

                // Calculate dictionary size to update offset properly
                int dictStartOffset = offset;
                int dictCount = ToInt32BigEndian(bytes, offset);
                offset += 4;

                if (dictCount > 0 && dictCount < 10000) {
                    for (int i = 0; i < dictCount; i++) {
                        // Read key length and key
                        int keyLength = ToInt32BigEndian(bytes, offset);
                        offset += 4 + keyLength;

                        // Read value type and advance offset accordingly
                        byte typeId = bytes[offset++];
                        switch (typeId) {
                            case 0: break; // null
                            case 1: offset += 4; break; // int
                            case 2: offset += 4; break; // float
                            case 3: offset += 8; break; // double
                            case 4: offset += 8; break; // long
                            case 5: offset += 2; break; // short
                            case 6: // string
                                int stringLength = ToInt32BigEndian(bytes, offset);
                                offset += 4 + stringLength;
                                break;
                            case 7: offset += 1; break; // bool
                            case 8: offset += 1; break; // byte
                        }
                    }
                }

                Console.WriteLine($"[ToGameObject] Final offset: {offset}");

                GameObject gameObject = new GameObject() {
                    Id = id,
                    Health = health, // Now correctly using float
                    Bounds = bounds,
                    Velocity = velocity,
                    Rotation = rotation,
                    Scale = scale,
                    IsStatic = isStatic,
                    Mass = mass,
                    CustomProperties = customProperties ?? new Dictionary<string, object>()
                };

                return gameObject;
            } catch (Exception ex) {
                Console.WriteLine($"[ToGameObject] EXCEPTION at offset {offset}: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }
    }
}