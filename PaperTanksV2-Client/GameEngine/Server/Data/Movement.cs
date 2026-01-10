using System;

namespace PaperTanksV2Client.GameEngine.Server.Data
{
    public struct Movement
    {
        public PlayerInput input;
        public Movement(PlayerInput input)
        {
            this.input = input; 
        }
        public static Movement FromBytes(byte[] data)
        {
            return new Movement(BinaryHelper.ToPlayerInputBigEndian(data, 0));
        }
        public byte[] ToBytes()
        {
            return BinaryHelper.GetBytesBigEndian(this);
        }
    }
}