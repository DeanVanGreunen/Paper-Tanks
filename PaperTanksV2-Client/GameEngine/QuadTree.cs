using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PaperTanksV2Client.GameEngine
{
    public class QuadTree
    {
        private Rectangle bounds;
        private List<GameObject> objects;

        public QuadTree(Rectangle bounds)
        {
            this.bounds = bounds;
            this.objects = new List<GameObject>();
        }

        public void Insert(GameObject item) => objects.Add(item);
        public void Clear() => objects.Clear();
        public List<GameObject> Query(Rectangle area) => objects.Where(o => area.Intersects(o.Bounds.getRectangle())).ToList();
    }

    public struct Rectangle : System.IEquatable<Rectangle>
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public Rectangle(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rectangle) {
                Rectangle obj2 = (Rectangle) obj;
                return obj2.Position == this.Position && obj2.Size == this.Size;
            } else {
                return false;
            }
        }

        public bool Intersects(Rectangle other)
        {
            return this.Position.X < other.Position.X + other.Size.X &&
               this.Position.X + this.Size.X > other.Position.X &&
               this.Position.Y < other.Position.Y + other.Size.Y &&
               this.Position.Y + this.Size.Y > other.Position.Y;
        }

        public override int GetHashCode()
        {
            return this.Position.GetHashCode() + this.Size.GetHashCode();
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !( left == right );
        }

        public bool Equals(Rectangle other)
        {
            return other.Position == this.Position && other.Size == this.Size;
        }
    }
}
