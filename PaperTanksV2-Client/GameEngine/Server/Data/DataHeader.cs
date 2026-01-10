using System;

namespace PaperTanksV2Client.GameEngine.Server.Data
{
    public struct DataHeader
    {
        public DataType dataType;
        public int dataLength;
        public byte[] buffer;

        public DataHeader(DataType dataType,
        int dataLength,
        byte[] buffer)
        {
            this.dataType = dataType;
            this.dataLength = dataLength;
            this.buffer = buffer;
        }

        public DataHeader GetHeartBeatMessage()
        {
            byte[] HeartBeatData = new byte[]{0};
            return new DataHeader(DataType.HeartBeat, HeartBeatData.Length, HeartBeatData);
        }
    }
}