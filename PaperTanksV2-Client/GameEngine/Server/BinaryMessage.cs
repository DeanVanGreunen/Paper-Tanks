using PaperTanksV2Client.GameEngine.Server.Data;
using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.GameEngine.Server
{
    public class BinaryMessage
    {
        public DataHeader DataHeader { get; }
        public static BinaryMessage HeartBeatMessage = new BinaryMessage(new DataHeader(DataType.HeartBeat, 1, new byte[]{0}));
        public BinaryMessage(byte[] data, DataType dataType)
        {
            DataHeader = new DataHeader
            {
                buffer = data,
                dataLength = data.Length,
                dataType = dataType
            };
        }
        
        public BinaryMessage(DataHeader dataHeader)
        {
            DataHeader = dataHeader;
        }
        
        public byte[] ToBinaryArray()
        {
            // Format: [DataType(1 byte)][DataLength(4 bytes)][Buffer(N bytes)]
            byte[] result = new byte[1 + 4 + DataHeader.buffer.Length];
            
            result[0] = (byte)DataHeader.dataType;
            byte[] lengthBytes = BitConverter.GetBytes(DataHeader.dataLength);
            Array.Copy(lengthBytes, 0, result, 1, 4);
            Array.Copy(DataHeader.buffer, 0, result, 5, DataHeader.buffer.Length);
            
            return result;
        }
        
        public static DataHeader FromBinaryArray(byte[] data)
        {
            if (data == null || data.Length < 5)
                throw new ArgumentException("Invalid data format");
            
            DataType dataType = (DataType)data[0];
            int dataLength = BitConverter.ToInt32(data, 1);
            
            byte[] buffer = new byte[dataLength];
            Array.Copy(data, 5, buffer, 0, Math.Min(dataLength, data.Length - 5));
            
            return new DataHeader
            {
                dataType = dataType,
                dataLength = dataLength,
                buffer = buffer
            };
        }
    }
}