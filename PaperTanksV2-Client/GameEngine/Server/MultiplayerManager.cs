using System;
using System.Collections.Generic;
using System.Linq;

namespace PaperTanksV2Client.GameEngine
{
    public static class MultiplayerManager
    {

        public static Level LoadLevel(String levelName)
        {
            ResourceManager resource = new ResourceManager();
            string fileName = resource.GetResourcePath(ResourceManagerFormat.MultiPlayerLevel, levelName + ".json");
            if (!resource.Load(ResourceManagerFormat.Level, levelName + ".json")) {
                Console.WriteLine("No Level File Found");
                Console.WriteLine(fileName);
                return null;
            }
            Level level = resources.Get(ResourceManagerFormat.MultiPlayerLevel, levelName + ".json") as Level;
            if (level == null) {
                Console.WriteLine("No Level File Found");
                Console.WriteLine(fileName);
                return null;
            }
            level.fileName = levelName;
            return level;
        }

        public static List<string> GetMultiPlayerList()
        {
            ResourceManager resource = new ResourceManager();
            return resource.GetMultiPlayerList();
        }
    }
}