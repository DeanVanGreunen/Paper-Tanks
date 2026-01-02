using Newtonsoft.Json;

namespace PaperTanksV2Client.GameEngine
{
    public class BoundsData
    {
        [JsonProperty("Position")]
        public Vector2Data Position { get; set; }

        [JsonProperty("Size")]
        public Vector2Data Size { get; set; }

        public BoundsData(Vector2Data Position, Vector2Data Size) {
            this.Position = Position;
            this.Size = Size;
        }

        public Rectangle getRectangle() {
            return new Rectangle(new System.Numerics.Vector2(this.Position.X, this.Position.Y), new System.Numerics.Vector2(this.Size.X, this.Size.Y));
        }
    }
}