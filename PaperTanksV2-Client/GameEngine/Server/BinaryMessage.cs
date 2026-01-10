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
            byte[] lengthBytes = BinaryHelper.GetBytesBigEndian(DataHeader.dataLength);
            if (DataHeader.dataLength >= 4096000) return null;
            Array.Copy(lengthBytes, 0, result, 1, 4);
            Array.Copy(DataHeader.buffer, 0, result, 5, DataHeader.buffer.Length);
        
            return result;
        }
    
        public static BinaryMessage FromBinaryArray(byte[] data)
        {
            try
            {
                if (data == null || data.Length < 5)
                {
                    Console.WriteLine($"[FromBinaryArray] Invalid data: length={data?.Length ?? 0}");
                    return null;
                }
            
                DataType dataType = (DataType)data[0];
                
                // FIX: Use Big Endian conversion to match ToBinaryArray()
                int dataLength = BinaryHelper.ToInt32BigEndian(data, 1);
                
                Console.WriteLine($"[FromBinaryArray] Type={dataType}, Length={dataLength}, DataSize={data.Length}");

                if (dataLength < 0 || dataLength >= 4096000)
                {
                    Console.WriteLine($"[FromBinaryArray] Invalid data length: {dataLength}");
                    return null;
                }
                
                if (dataLength > data.Length - 5)
                {
                    Console.WriteLine($"[FromBinaryArray] Data length {dataLength} exceeds available data {data.Length - 5}");
                    return null;
                }
                
                byte[] buffer = new byte[dataLength];
                Array.Copy(data, 5, buffer, 0, dataLength);
            
                DataHeader dataHeader = new DataHeader
                {
                    dataType = dataType,
                    dataLength = dataLength,
                    buffer = buffer
                };
            
                return new BinaryMessage(dataHeader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FromBinaryArray] Exception: {ex.Message}");
                return null;
            }
        }
    }
}