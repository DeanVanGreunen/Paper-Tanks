using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client
{
    public class CampaignManager
    {
        public object tank_data = null; // TODO: UPDATE AND FIX THIS
        public float score = 0;
        public float deaths = 0;
        public float seconds_played = 0;
        public string level_id = "";
        public List<Achievement> achievements = new List<Achievement>();
        public void Load() {

        }
        public void Save() {

        }
    }
}
