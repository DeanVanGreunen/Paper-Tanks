using System;

namespace PaperTanksV2Client.GameEngine.Server.Data
{
    public static class BinaryHelper
    {
        /// <summary>
        /// Converts an Int32 to a big-endian byte array
        /// </summary>
        public static byte[] GetBytesBigEndian(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            
            if (BitConverter.IsLittleEndian)
            {
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
            
            if (BitConverter.IsLittleEndian)
            {
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
            
            if (BitConverter.IsLittleEndian)
            {
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
            
            if (BitConverter.IsLittleEndian)
            {
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
            
            if (BitConverter.IsLittleEndian)
            {
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
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(workingBytes);
            }
            
            return BitConverter.ToInt64(workingBytes, 0);
        }
    }
}