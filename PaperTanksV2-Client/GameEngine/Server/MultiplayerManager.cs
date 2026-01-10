using PaperTanksV2Client.GameEngine.data;
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
                if(TextData.DEBUG_MODE == true) Console.WriteLine("No Level File Found");
                if(TextData.DEBUG_MODE == true) Console.WriteLine(fileName);
                return null;
            }
            Level level = resource.Get(ResourceManagerFormat.MultiPlayerLevel, levelName + ".json") as Level;
            if (level == null) {
                if(TextData.DEBUG_MODE == true) Console.WriteLine("No Level File Found");
                if(TextData.DEBUG_MODE == true) Console.WriteLine(fileName);
                return null;
            }
            level.fileName = levelName;
            return level;
        }

        public static List<string> GetMultiPlayerList()
        {
            return (new ResourceManager()).GetMultiPlayerList()
            .Select(a => 
            {
                string str = a.ToString();
                int index = str.LastIndexOf(".json");
                return index >= 0 ? str.Substring(0, index) : str;
            })
            .ToList();
        }
    }
}