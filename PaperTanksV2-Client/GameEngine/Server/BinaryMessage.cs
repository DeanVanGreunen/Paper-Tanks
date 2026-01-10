namespace PaperTanksV2Client.GameEngine.Server
{
    public class BinaryMessage
    {
        public byte[] Data { get; }
        
        public BinaryMessage(byte[] data)
        {
            Data = data;
        }
    }
}