using Newtonsoft.Json;

namespace PaperTanksV2Client.GameEngine
{
    public class Shape
    {
        [JsonProperty("Radius")]
        public float Radius { get; set; }
    }
}