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
            // Reset quad tree
            quadTree.Clear();
            foreach (var obj in objects) {
                quadTree.Insert(obj);
            }

            // Broad phase collision detection
            potentialCollisions.Clear();
            foreach (var obj in objects) {
                if (obj.IsStatic) continue;

                var nearby = quadTree.Query(obj.Bounds);
                foreach (var other in nearby) {
                    if (other != obj) {
                        potentialCollisions.Add(obj, other);
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

                // Update position
                obj.Position += obj.Velocity * deltaTime;
            }
        }

        private bool CheckDetailedCollision(GameObject a, GameObject b)
        {
            // Implement detailed collision detection based on object bounds
            return a.Bounds.Intersects(b.Bounds);
        }

        private void ResolveCollision(GameObject a, GameObject b)
        {
            if (a.IsStatic && b.IsStatic) return;

            // Calculate collision response
            var normal = Vector2.Normalize(b.Position - a.Position);
            var relativeVelocity = b.Velocity - a.Velocity;
            var velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

            // Only resolve if objects are moving toward each other
            if (velocityAlongNormal > 0) return;

            var restitution = 0.5f; // Bounce factor
            var j = -( 1 + restitution ) * velocityAlongNormal;

            if (!a.IsStatic && !b.IsStatic) {
                var massSum = 2.0f; // Assuming equal masses for simplicity
                j /= massSum;

                a.Velocity -= normal * j;
                b.Velocity += normal * j;
            } else {
                var movingObj = a.IsStatic ? b : a;
                var sign = a.IsStatic ? 1.0f : -1.0f;
                movingObj.Velocity += sign * normal * j;
            }
        }
    }
}
