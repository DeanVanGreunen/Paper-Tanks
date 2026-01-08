using System;
using System.Collections.Generic;
using System.Linq;

namespace PaperTanksV2Client.GameEngine
{
    public static class CampaignManager
    {
        public static string GetNextLevel(Game game, string? levelName)
        {
            if (game == null) return null;
    
            List<string> levelNames = game.resources.GetList()
                .Select(s => s.Split("\\").Last().Replace(".json", ""))
                .OrderBy(s => Guid.Parse(s))
                .ToList();
    
            if (levelNames.Count == 0) return null;
            if(levelName == null){
                return levelNames[0];
            } else {
                int index = levelNames.IndexOf(levelName);
                // Return next level if current level exists and is not the last one
                if (index != -1 && index < levelNames.Count - 1)
                {
                    return levelNames[index + 1];
                }
           }
            // Level not found or is the last level
            return null;
        }

        public static Level LoadLevel(Game game, String levelName)
        {
            string fileName = game.resources.GetResourcePath(ResourceManagerFormat.Level, levelName + ".json");
            if (!game.resources.Load(ResourceManagerFormat.Level, levelName + ".json")) {
                Console.WriteLine("No Level File Found");
                Console.WriteLine(fileName);
                return null;
            }
            Level level = game.resources.Get(ResourceManagerFormat.Level, levelName + ".json") as Level;
            if (level == null) {
                Console.WriteLine("No Level File Found");
                Console.WriteLine(fileName);
                return null;
            }
            level.fileName = levelName;
            return level;
        }
    }
}