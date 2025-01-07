using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class RectangleShape : CollisionShape
    {
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }

        public RectangleShape(Vector2 size)
        {
            Min = -size / 2;
            Max = size / 2;
            Scale = Vector2.One;
        }

        public override BoundingBox GetBoundingBox()
        {
            // Transform all corners and find min/max
            var corners = new[]
            {
            TransformPoint(new Vector2(Min.X, Min.Y)),
            TransformPoint(new Vector2(Max.X, Min.Y)),
            TransformPoint(new Vector2(Min.X, Max.Y)),
            TransformPoint(new Vector2(Max.X, Max.Y))
        };

            var boundMin = new Vector2(float.MaxValue);
            var boundMax = new Vector2(float.MinValue);

            foreach (var corner in corners) {
                boundMin = Vector2.Min(boundMin, corner);
                boundMax = Vector2.Max(boundMax, corner);
            }

            return new BoundingBox(boundMin, boundMax);
        }

        public override bool TestCollision(CollisionShape other, Matrix3x2 thisTransform, Matrix3x2 otherTransform)
        {
            if (other is RectangleShape rectangle)
                return TestRectangleRectangle(rectangle, thisTransform, otherTransform);
            if (other is CircleShape circle)
                return circle.TestCollision(this, otherTransform, thisTransform);
            return false;
        }

        private bool TestRectangleRectangle(RectangleShape other, Matrix3x2 thisTransform, Matrix3x2 otherTransform)
        {
            // Use Separating Axis Theorem (SAT)
            var thisCorners = GetTransformedCorners(thisTransform);
            var otherCorners = other.GetTransformedCorners(otherTransform);

            // Get axes to test
            var axes = new List<Vector2>();
            AddRectangleAxes(thisCorners, axes);
            AddRectangleAxes(otherCorners, axes);

            // Test projection onto each axis
            foreach (var axis in axes) {
                if (!OverlapOnAxis(thisCorners, otherCorners, axis))
                    return false;
            }

            return true;
        }

        private Vector2[] GetTransformedCorners(Matrix3x2 transform)
        {
            return new[]
            {
            Vector2.Transform(new Vector2(Min.X, Min.Y), transform),
            Vector2.Transform(new Vector2(Max.X, Min.Y), transform),
            Vector2.Transform(new Vector2(Max.X, Max.Y), transform),
            Vector2.Transform(new Vector2(Min.X, Max.Y), transform)
        };
        }

        private void AddRectangleAxes(Vector2[] corners, List<Vector2> axes)
        {
            for (int i = 0; i < corners.Length; i++) {
                var edge = corners[( i + 1 ) % corners.Length] - corners[i];
                var normal = Vector2.Normalize(new Vector2(-edge.Y, edge.X));
                axes.Add(normal);
            }
        }

        private bool OverlapOnAxis(Vector2[] corners1, Vector2[] corners2, Vector2 axis)
        {
            var (min1, max1) = ProjectOntoAxis(corners1, axis);
            var (min2, max2) = ProjectOntoAxis(corners2, axis);
            return max1 >= min2 && max2 >= min1;
        }

        private (float min, float max) ProjectOntoAxis(Vector2[] corners, Vector2 axis)
        {
            var min = float.MaxValue;
            var max = float.MinValue;

            foreach (var corner in corners) {
                var projection = Vector2.Dot(corner, axis);
                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            return (min, max);
        }
    }
}
