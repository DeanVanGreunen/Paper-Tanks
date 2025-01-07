using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public abstract class CollisionShape
    {
        public Vector2 LocalPosition { get; set; } // Offset from parent object
        public float LocalRotation { get; set; }   // Local rotation in radians
        public Vector2 Scale { get; set; }

        // Transform matrix cache
        protected Matrix3x2 LocalTransform;
        protected Matrix3x2 WorldTransform;

        public abstract BoundingBox GetBoundingBox();
        public abstract bool TestCollision(CollisionShape other, Matrix3x2 thisTransform, Matrix3x2 otherTransform);

        // Update transform matrices
        public void UpdateTransforms(Vector2 parentPosition, float parentRotation)
        {
            // Create local transform
            LocalTransform = Matrix3x2.CreateScale(Scale) *
                            Matrix3x2.CreateRotation(LocalRotation) *
                            Matrix3x2.CreateTranslation(LocalPosition);

            // Create world transform
            WorldTransform = LocalTransform *
                            Matrix3x2.CreateRotation(parentRotation) *
                            Matrix3x2.CreateTranslation(parentPosition);
        }

        // Transform a point from local to world space
        protected Vector2 TransformPoint(Vector2 point) => Vector2.Transform(point, WorldTransform);
    }
}
