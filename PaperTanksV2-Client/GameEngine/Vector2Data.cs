using Newtonsoft.Json;

namespace PaperTanksV2Client.GameEngine
{
    public class Vector2Data
    {
        [JsonProperty("X")]
        public float X { get; set; }

        [JsonProperty("Y")]
        public float Y { get; set; }

        public Vector2Data(float X, float Y) {
            this.X = X;
            this.Y = Y;
        }

        public static Vector2Data Zero => new Vector2Data(0f, 0f);
    }
}