using Newtonsoft.Json;
using System;

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
        public bool Intersects(BoundsData other)
        {
            // Calculate the edges of both bounds
            float thisLeft = this.Position.X;
            float thisRight = this.Position.X + this.Size.X;
            float thisTop = this.Position.Y;
            float thisBottom = this.Position.Y + this.Size.Y;

            float otherLeft = other.Position.X;
            float otherRight = other.Position.X + other.Size.X;
            float otherTop = other.Position.Y;
            float otherBottom = other.Position.Y + other.Size.Y;

            // Check if they DON'T intersect, then negate
            return !(thisRight < otherLeft || 
                     thisLeft > otherRight || 
                     thisBottom < otherTop || 
                     thisTop > otherBottom);
        }
        
        public BoundsData GetNonIntersectingPosition(BoundsData other)
        {
            if (!this.Intersects(other))
            {
                // Already not intersecting, return a copy of current bounds
                return new BoundsData(
                    new Vector2Data(this.Position.X, this.Position.Y), 
                    new Vector2Data(this.Size.X, this.Size.Y)
                );
            }

            // Calculate overlap on each axis
            float thisLeft = this.Position.X;
            float thisRight = this.Position.X + this.Size.X;
            float thisTop = this.Position.Y;
            float thisBottom = this.Position.Y + this.Size.Y;

            float otherLeft = other.Position.X;
            float otherRight = other.Position.X + other.Size.X;
            float otherTop = other.Position.Y;
            float otherBottom = other.Position.Y + other.Size.Y;

            // Calculate push distances for each direction
            float pushLeft = otherLeft - thisRight;
            float pushRight = otherRight - thisLeft;
            float pushUp = otherTop - thisBottom;
            float pushDown = otherBottom - thisTop;

            // Find the smallest push distance
            float minPush = float.MaxValue;
            Vector2Data newPosition = new Vector2Data(this.Position.X, this.Position.Y);

            if (Math.Abs(pushLeft) < Math.Abs(minPush))
            {
                minPush = pushLeft;
                newPosition = new Vector2Data(this.Position.X + pushLeft, this.Position.Y);
            }
            if (Math.Abs(pushRight) < Math.Abs(minPush))
            {
                minPush = pushRight;
                newPosition = new Vector2Data(this.Position.X + pushRight, this.Position.Y);
            }
            if (Math.Abs(pushUp) < Math.Abs(minPush))
            {
                minPush = pushUp;
                newPosition = new Vector2Data(this.Position.X, this.Position.Y + pushUp);
            }
            if (Math.Abs(pushDown) < Math.Abs(minPush))
            {
                minPush = pushDown;
                newPosition = new Vector2Data(this.Position.X, this.Position.Y + pushDown);
            }

            return new BoundsData(newPosition, new Vector2Data(this.Size.X, this.Size.Y));
        }

        public BoundsData GetRotatedBounds(float angle)
        {
            // Calculate the center of the bounds
            float centerX = this.Position.X + this.Size.X / 2;
            float centerY = this.Position.Y + this.Size.Y / 2;

            // Convert angle to radians
            float radians = angle * (float)Math.PI / 180f;

            // Calculate rotated position (rotate the top-left corner around center)
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            // Translate position to origin (relative to center)
            float relativeX = this.Position.X - centerX;
            float relativeY = this.Position.Y - centerY;

            // Apply rotation
            float rotatedX = relativeX * cos - relativeY * sin;
            float rotatedY = relativeX * sin + relativeY * cos;

            // Translate back
            float newX = rotatedX + centerX;
            float newY = rotatedY + centerY;

            return new BoundsData(
                new Vector2Data(newX, newY),
                new Vector2Data(this.Size.X, this.Size.Y)
            );
        }
    }
}