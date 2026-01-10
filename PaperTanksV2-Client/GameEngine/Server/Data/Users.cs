using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.GameEngine.Server.Data
{
    public struct Users
    {
        public List<UsersData> usersData;

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(usersData.Count));
            foreach (UsersData users in usersData) {
                bytes.AddRange(users.GetBytes());
            }
            return bytes.ToArray();
        }
    }
    
    public struct UsersData
    {
        public Guid guid;

        
        public byte[] GetBytes()
        {
            return this.guid.ToByteArray();
        }
    }
}