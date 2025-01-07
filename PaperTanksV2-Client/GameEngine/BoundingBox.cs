using System.Numerics;

namespace PaperTanksV2Client.GameEngine
{
    public class BoundingBox
    {
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }

        public BoundingBox(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public bool Intersects(BoundingBox other)
        {
            return Max.X >= other.Min.X && Min.X <= other.Max.X &&
                   Max.Y >= other.Min.Y && Min.Y <= other.Max.Y;
        }
    }
}
