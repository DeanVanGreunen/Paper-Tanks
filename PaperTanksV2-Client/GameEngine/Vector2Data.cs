using Newtonsoft.Json;
using System.Numerics;

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
        public static Vector2Data One => new Vector2Data(1f, 1f);
        public static implicit operator Vector2(Vector2Data data)
        {
            if (data == null) return new Vector2(0f, 0f);
            return new Vector2(data.X, data.Y);
        }
        public static implicit operator Vector2Data(Vector2 vec) {
            return new Vector2Data(vec.X, vec.Y);
        }
    }
}