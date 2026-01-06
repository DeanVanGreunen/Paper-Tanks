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
        
        public bool IntersectsWhenRotated(BoundsData other, float angle)
{
    // Convert angle to radians
    float radians = angle * (float)Math.PI / 180f;
    float cos = (float)Math.Cos(radians);
    float sin = (float)Math.Sin(radians);

    // Calculate centers
    float thisCenterX = this.Position.X + this.Size.X / 2;
    float thisCenterY = this.Position.Y + this.Size.Y / 2;
    float otherCenterX = other.Position.X + other.Size.X / 2;
    float otherCenterY = other.Position.Y + other.Size.Y / 2;

    // Get the 4 corners of this rotated AABB
    Vector2Data[] thisCorners = new Vector2Data[4];
    float halfWidth = this.Size.X / 2;
    float halfHeight = this.Size.Y / 2;

    // Top-left, top-right, bottom-right, bottom-left (relative to center)
    float[] relativeX = { -halfWidth, halfWidth, halfWidth, -halfWidth };
    float[] relativeY = { -halfHeight, -halfHeight, halfHeight, halfHeight };

    for (int i = 0; i < 4; i++)
    {
        float rotatedX = relativeX[i] * cos - relativeY[i] * sin;
        float rotatedY = relativeX[i] * sin + relativeY[i] * cos;
        thisCorners[i] = new Vector2Data(
            thisCenterX + rotatedX,
            thisCenterY + rotatedY
        );
    }

    // Get the 4 corners of the other AABB (not rotated)
    Vector2Data[] otherCorners = new Vector2Data[4];
    otherCorners[0] = new Vector2Data(other.Position.X, other.Position.Y); // Top-left
    otherCorners[1] = new Vector2Data(other.Position.X + other.Size.X, other.Position.Y); // Top-right
    otherCorners[2] = new Vector2Data(other.Position.X + other.Size.X, other.Position.Y + other.Size.Y); // Bottom-right
    otherCorners[3] = new Vector2Data(other.Position.X, other.Position.Y + other.Size.Y); // Bottom-left

    // Use Separating Axis Theorem (SAT)
    // Test axes from the rotated rectangle
    Vector2Data[] axes = new Vector2Data[4];
    axes[0] = new Vector2Data(cos, sin); // Rotated X axis
    axes[1] = new Vector2Data(-sin, cos); // Rotated Y axis
    axes[2] = new Vector2Data(1, 0); // Other's X axis
    axes[3] = new Vector2Data(0, 1); // Other's Y axis

    foreach (var axis in axes)
    {
        // Project both shapes onto this axis
        float thisMin = float.MaxValue, thisMax = float.MinValue;
        float otherMin = float.MaxValue, otherMax = float.MinValue;

        foreach (var corner in thisCorners)
        {
            float projection = corner.X * axis.X + corner.Y * axis.Y;
            thisMin = Math.Min(thisMin, projection);
            thisMax = Math.Max(thisMax, projection);
        }

        foreach (var corner in otherCorners)
        {
            float projection = corner.X * axis.X + corner.Y * axis.Y;
            otherMin = Math.Min(otherMin, projection);
            otherMax = Math.Max(otherMax, projection);
        }

        // Check if projections overlap
        if (thisMax < otherMin || otherMax < thisMin)
        {
            return false; // Found a separating axis, no intersection
        }
    }

    return true; // No separating axis found, they intersect
}
    }
}