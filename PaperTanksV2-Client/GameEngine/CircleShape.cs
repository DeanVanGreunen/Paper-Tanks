using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class CircleShape : CollisionShape
    {
        public float Radius { get; set; }

        public CircleShape(float radius)
        {
            Radius = radius;
            Scale = Vector2.One;
        }

        public override BoundingBox GetBoundingBox()
        {
            var scaledRadius = Radius * Math.Max(Scale.X, Scale.Y);
            var center = Vector2.Transform(LocalPosition, WorldTransform);
            return new BoundingBox(
                center - new Vector2(scaledRadius),
                center + new Vector2(scaledRadius)
            );
        }

        public override bool TestCollision(CollisionShape other, Matrix3x2 thisTransform, Matrix3x2 otherTransform)
        {
            if (other is CircleShape circle)
                return TestCircleCircle(circle, thisTransform, otherTransform);
            if (other is RectangleShape rectangle)
                return TestCircleRectangle(rectangle, thisTransform, otherTransform);
            return false;
        }

        private bool TestCircleCircle(CircleShape other, Matrix3x2 thisTransform, Matrix3x2 otherTransform)
        {
            var thisCenter = Vector2.Transform(Vector2.Zero, thisTransform);
            var otherCenter = Vector2.Transform(Vector2.Zero, otherTransform);
            var distance = Vector2.Distance(thisCenter, otherCenter);
            return distance < ( this.Radius + other.Radius );
        }

        private bool TestCircleRectangle(RectangleShape rectangle, Matrix3x2 thisTransform, Matrix3x2 otherTransform)
        {
            // Transform circle center to world space
            var circleCenter = Vector2.Transform(Vector2.Zero, thisTransform);

            // Get inverse of rectangle transform
            Matrix3x2 inverseRect;
            if (!Matrix3x2.Invert(otherTransform, out inverseRect)) {
                // Handle case where matrix is not invertible
                return false;
            }

            // Transform circle center to rectangle's local space
            var localCircleCenter = Vector2.Transform(circleCenter, inverseRect);

            // Find closest point on rectangle to circle center
            var closestX = Math.Max(rectangle.Min.X, Math.Min(localCircleCenter.X, rectangle.Max.X));
            var closestY = Math.Max(rectangle.Min.Y, Math.Min(localCircleCenter.Y, rectangle.Max.Y));
            var closest = new Vector2(closestX, closestY);

            // Transform closest point back to world space
            var worldClosest = Vector2.Transform(closest, otherTransform);

            // Check if distance is less than or equal to circle radius
            return Vector2.Distance(circleCenter, worldClosest) <= this.Radius;
        }
    }
}
