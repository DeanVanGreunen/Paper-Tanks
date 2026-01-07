using System;

namespace PaperTanksV2Client.GameEngine.AI
{
    public class ChaseAI : AIAgent
    {
        private float fireCooldown = 0f;
        private float nextFireDelay = 0f;
        private Random random = new Random();
        public override void Update(Tank self, GameEngineInstance engine, Single deltaTime)
        {// Update fire cooldown
            if (fireCooldown > 0) {
                fireCooldown -= deltaTime;
            }
                    // AI behavior
        Tank player = engine.GetObject(engine.playerID) as Tank;
        if (player != null) {
            // Calculate direction vector to player
            float dx = player.Bounds.Position.X - self.Bounds.Position.X;
            float dy = player.Bounds.Position.Y - self.Bounds.Position.Y;

            // Calculate distance
            float distance = (float) Math.Sqrt(dx * dx + dy * dy);

            // Only move if not already at player's position
            if (distance > 0) {
                // Calculate angle in degrees
                float angleRadians = (float) Math.Atan2(dy, dx);
                float angleDegrees = angleRadians * ( 180f / (float) Math.PI );

                // Snap to nearest allowed angle (0, 90, -90, 180, -180)
                float snappedAngle;
                if (angleDegrees > -45 && angleDegrees <= 45) {
                    snappedAngle = 0; // Right
                } else if (angleDegrees > 45 && angleDegrees <= 135) {
                    snappedAngle = 90; // Down
                } else if (angleDegrees > -135 && angleDegrees <= -45) {
                    snappedAngle = -90; // Up
                } else {
                    snappedAngle = ( angleDegrees > 0 ) ? 180 : -180; // Left
                }

                self.Rotation = snappedAngle;

                // Check if player is in line of sight (aligned on one axis)
                bool inLineOfSight = false;
                float alignmentThreshold = 10f; // Adjust based on tank size

                if (snappedAngle == 0 || snappedAngle == 180 || snappedAngle == -180) {
                    // Horizontal alignment - check if Y positions are similar
                    inLineOfSight = Math.Abs(dy) < alignmentThreshold;
                } else {
                    // Vertical alignment - check if X positions are similar
                    inLineOfSight = Math.Abs(dx) < alignmentThreshold;
                }

                // Fire if in line of sight and cooldown has expired
                if (inLineOfSight && fireCooldown <= 0) {
                    Projectile projectile = self.Fire(engine);
                    engine.QueueAddObject(projectile);
                    
                    // Set random cooldown between 1 and 3 seconds
                    fireCooldown = 1f + (float) random.NextDouble() * 2f;
                }

                // Move only in cardinal directions (no diagonals)
                float speed = 25f;
                
                // Determine primary movement direction based on snapped angle
                if (snappedAngle == 0) {
                    // Move right only
                    self.Bounds.Position.X += speed * deltaTime;
                } else if (snappedAngle == 180 || snappedAngle == -180) {
                    // Move left only
                    self.Bounds.Position.X -= speed * deltaTime;
                } else if (snappedAngle == -90) {
                    // Move up only
                    self.Bounds.Position.Y -= speed * deltaTime;
                } else if (snappedAngle == 90) {
                    // Move down only
                    self.Bounds.Position.Y += speed * deltaTime;
                }
            }
        }
        }
    }
}