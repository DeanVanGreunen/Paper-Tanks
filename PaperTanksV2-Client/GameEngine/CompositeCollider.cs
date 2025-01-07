using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class CompositeCollider
    {
        private List<CollisionShape> shapes;
        private GameObject parent;
        private Rectangle cachedBounds;
        private Matrix3x2 worldTransform;

        public CompositeCollider(GameObject parent)
        {
            this.parent = parent;
            this.shapes = new List<CollisionShape>();
            this.worldTransform = Matrix3x2.Identity;
        }

        public Matrix3x2 WorldTransform => worldTransform;

        public void AddShape(CollisionShape shape)
        {
            shapes.Add(shape);
        }

        public void UpdateTransforms()
        {
            // Create world transform matrix from position and rotation
            worldTransform = Matrix3x2.CreateRotation(parent.Rotation) *
                           Matrix3x2.CreateTranslation(parent.Position);

            foreach (var shape in shapes) {
                shape.UpdateTransforms(parent.Position, parent.Rotation);
            }
            UpdateBoundingBox();
        }

        public Rectangle GetBoundingBox() => cachedBounds;

        private void UpdateBoundingBox()
        {
            if (shapes.Count == 0) {
                cachedBounds = new Rectangle(parent.Position, parent.Position);
                return;
            }
            var min = new Vector2(float.MaxValue);
            var max = new Vector2(float.MinValue);
            foreach (var shape in shapes) {
                var bounds = shape.GetBoundingBox();
                min = Vector2.Min(min, bounds.Min);
                max = Vector2.Max(max, bounds.Max);
            }
            cachedBounds = new Rectangle(min, max);
        }

        public bool TestCollision(CompositeCollider other)
        {
            // First test bounding boxes for early exit
            if (!cachedBounds.Intersects(other.GetBoundingBox()))
                return false;

            foreach (var thisShape in shapes) {
                foreach (var otherShape in other.shapes) {
                    if (thisShape.TestCollision(otherShape, WorldTransform, other.WorldTransform))
                        return true;
                }
            }
            return false;
        }
    }
}
