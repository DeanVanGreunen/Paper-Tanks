using System;
using System.Collections.Generic;
using System.Linq;

namespace PaperTanksV2Client.GameEngine
{
    public static class CampaignManager
    {
        public static string GetNextLevel(Game game, string levelName)
        {
            if (game == null) return null;
            List<string> levelNames = game.resources.GetList().OrderBy(s => Guid.Parse(s.Split("\\").Last().Replace(".json", ""))).ToList();
            if (levelNames.Count >= 1) {
                int index = levelNames.IndexOf(levelName);
                if (index != -1 && index <= levelNames.Count - 1) {
                    try {
                        return levelNames[index + 1];
                    } catch (Exception e) {
                        return null;
                    }
                }
            }
            return null;
        }
    }
}