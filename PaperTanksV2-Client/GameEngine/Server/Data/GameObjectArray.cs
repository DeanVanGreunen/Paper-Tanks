using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.GameEngine.Server.Data
{
    public struct GameObjectArray
    {
        public List<GameObject> gameObjectsData;

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinaryHelper.GetBytesBigEndian(gameObjectsData.Count));
            foreach (GameObject obj in gameObjectsData) {
                bytes.AddRange(obj.GetBytes());
            }
            return bytes.ToArray();
        }
    }
}