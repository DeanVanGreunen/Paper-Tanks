using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.GameEngine.Server.Data
{
    public class GameObjectArray
    {
        public List<GameObject> gameObjectsData = new List<GameObject>();

        public GameObjectArray(List<GameObject> list)
        {
            this.gameObjectsData = list;
        }
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