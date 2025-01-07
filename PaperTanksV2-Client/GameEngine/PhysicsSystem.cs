using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class PhysicsSystem
    {
        private readonly QuadTree quadTree;
        private readonly List<(GameObject, GameObject)> potentialCollisions;
        public static readonly Vector2 MaxVector = new Vector2(float.MaxValue, float.MaxValue);

        public PhysicsSystem(Vector2 worldSize)
        {
            quadTree = new QuadTree(new Rectangle(Vector2.Zero, worldSize));
            potentialCollisions = new List<(GameObject, GameObject)>();
        }

        public void Update(IEnumerable<GameObject> objects, float deltaTime)
        {
            // Update all collider transforms
            foreach (var obj in objects) {
                obj.Collider.UpdateTransforms();
            }

            // Reset quad tree
            quadTree.Clear();
            foreach (var obj in objects) {
                quadTree.Insert(obj);
            }

            // Broad phase collision detection
            potentialCollisions.Clear();
            foreach (var obj in objects) {
                if (obj.IsStatic) continue;

                var nearby = quadTree.Query(obj.Collider.GetBoundingBox());
                foreach (var other in nearby) {
                    if (other != obj) {
                        potentialCollisions.Add((obj, other));
                    }
                }
            }

            // Narrow phase collision detection and resolution
            foreach (var (objA, objB) in potentialCollisions) {
                if (CheckDetailedCollision(objA, objB)) {
                    ResolveCollision(objA, objB);
                    objA.HandleCollision(objB);
                    objB.HandleCollision(objA);
                }
            }

            // Update positions
            foreach (var obj in objects) {
                if (obj.IsStatic) continue;
                obj.Position += obj.Velocity * deltaTime;
                obj.Rotation += obj.AngularVelocity * deltaTime;
            }
        }

        private bool CheckDetailedCollision(GameObject a, GameObject b)
        {
            return a.Collider.TestCollision(b.Collider);
        }

        private void ResolveCollision(GameObject a, GameObject b)
        {
            // Similar to your existing collision resolution, but consider rotation
            if (a.IsStatic && b.IsStatic) return;

            var normal = Vector2.Normalize(b.Position - a.Position);
            var relativeVelocity = b.Velocity - a.Velocity;
            var velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

            if (velocityAlongNormal > 0) return;

            var restitution = 0.5f;
            var j = -( 1 + restitution ) * velocityAlongNormal;

            if (!a.IsStatic && !b.IsStatic) {
                var massSum = 2.0f;
                j /= massSum;
                a.Velocity -= normal * j;
                b.Velocity += normal * j;

                // Add some angular velocity based on collision point
                float angularImpulse = 0.2f; // Adjust as needed
                a.AngularVelocity -= angularImpulse * j;
                b.AngularVelocity += angularImpulse * j;
            } else {
                var movingObj = a.IsStatic ? b : a;
                var sign = a.IsStatic ? 1.0f : -1.0f;
                movingObj.Velocity += sign * normal * j;
                movingObj.AngularVelocity += sign * 0.2f * j;
            }
        }
    }
}
