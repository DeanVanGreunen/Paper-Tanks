using System;
using System.Collections.Generic;

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
        public static byte[] GetBytesBigEndian(Dictionary<string, object> value)
        {
            List<byte> bytes = new List<byte>();

            if (value == null) {
                // Write 0 for null dictionary
                bytes.AddRange(GetBytesBigEndian(0));
                return bytes.ToArray();
            }

            // Write the number of dictionary entries (4 bytes)
            bytes.AddRange(GetBytesBigEndian(value.Count));

            foreach (var kvp in value) {
                // Write key length (4 bytes) and key string
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(kvp.Key);
                bytes.AddRange(GetBytesBigEndian(keyBytes.Length));
                bytes.AddRange(keyBytes);

                // Write value type and value data
                bytes.AddRange(SerializeObject(kvp.Value));
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
            int offset = startIndex;

            // Read dictionary count
            int count = ToInt32BigEndian(bytes, offset);
            offset += 4;

            if (count == 0) {
                return null;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            for (int i = 0; i < count; i++) {
                // Read key
                int keyLength = ToInt32BigEndian(bytes, offset);
                offset += 4;
                string key = System.Text.Encoding.UTF8.GetString(bytes, offset, keyLength);
                offset += keyLength;

                // Read value
                object value = DeserializeObject(bytes, ref offset);

                dictionary[key] = value;
            }

            return dictionary;
        }

        private static object DeserializeObject(byte[] bytes, ref int offset)
        {
            byte typeId = bytes[offset++];

            switch (typeId) {
                case 0: // null
                    return null;
                case 1: // int
                    int intValue = ToInt32BigEndian(bytes, offset);
                    offset += 4;
                    return intValue;
                case 2: // float
                    float floatValue = ToSingleBigEndian(bytes, offset);
                    offset += 4;
                    return floatValue;
                case 3: // double
                    double doubleValue = ToDoubleBigEndian(bytes, offset);
                    offset += 8;
                    return doubleValue;
                case 4: // long
                    long longValue = ToInt64BigEndian(bytes, offset);
                    offset += 8;
                    return longValue;
                case 5: // short
                    short shortValue = ToInt16BigEndian(bytes, offset);
                    offset += 2;
                    return shortValue;
                case 6: // string
                    int stringLength = ToInt32BigEndian(bytes, offset);
                    offset += 4;
                    string stringValue = System.Text.Encoding.UTF8.GetString(bytes, offset, stringLength);
                    offset += stringLength;
                    return stringValue;
                case 7: // bool
                    bool boolValue = bytes[offset++] == 1;
                    return boolValue;
                case 8: // byte
                    return bytes[offset++];
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts an Vector2Data to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(Vector2Data value)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value.X));
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(value.Y));
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

    }
}