using System;
using System.Collections.Generic;

namespace PaperTanksV2Client.GameEngine.AI
{
    public class ChaseAndDodgeAI : AIAgent
    {
        private float fireCooldown = 0f;
        private float nextFireDelay = 0f;
        private Random random = new Random();

        public override void Update(Tank self, GameEngineInstance engine, Single deltaTime)
        {
            // Update fire cooldown
            if (fireCooldown > 0) {
                fireCooldown -= deltaTime;
            }
            
            List<Projectile> projectiles = engine.GetObjectByType<Projectile>();
            Tank player = engine.GetObject(engine.playerID) as Tank;
            
            // Check for incoming projectiles first (priority behavior)
            bool isDodging = false;
            Projectile closestThreat = null;
            float closestDistance = float.MaxValue;
            
            foreach (Projectile proj in projectiles) {
                // Skip if projectile belongs to this AI tank
                if (proj.ownerId == self.Id) {
                    continue;
                }

                // Calculate distance to projectile
                float projDx = proj.Bounds.Position.X - self.Bounds.Position.X;
                float projDy = proj.Bounds.Position.Y - self.Bounds.Position.Y;
                float projDistance = (float) Math.Sqrt(projDx * projDx + projDy * projDy);

                // Check if projectile is heading towards the tank
                bool isHeadingTowards = false;
                float dotProduct = 0f;
                
                if (proj.Velocity.X != 0 || proj.Velocity.Y != 0) {
                    // Normalize direction to tank
                    float dirX = projDx / projDistance;
                    float dirY = projDy / projDistance;
                    
                    // Normalize projectile velocity
                    float velMag = (float) Math.Sqrt(proj.Velocity.X * proj.Velocity.X + proj.Velocity.Y * proj.Velocity.Y);
                    float velDirX = proj.Velocity.X / velMag;
                    float velDirY = proj.Velocity.Y / velMag;
                    
                    // Dot product to check if heading towards tank
                    dotProduct = velDirX * dirX + velDirY * dirY;
                    isHeadingTowards = dotProduct > 0.7f; // Roughly within 45 degrees
                }

                // If projectile is within avoidance radius and heading towards tank
                if (projDistance < 100f && isHeadingTowards && projDistance < closestDistance) {
                    closestDistance = projDistance;
                    closestThreat = proj;
                }
            }
            
            // Dodge the closest threat
            if (closestThreat != null) {
                isDodging = true;
                float speed = 60f; // Faster dodge speed
                
                float projDx = closestThreat.Bounds.Position.X - self.Bounds.Position.X;
                float projDy = closestThreat.Bounds.Position.Y - self.Bounds.Position.Y;
                
                // Determine if projectile is moving horizontally or vertically
                // and dodge perpendicular to its path
                if (Math.Abs(closestThreat.Velocity.X) > Math.Abs(closestThreat.Velocity.Y)) {
                    // Projectile moving horizontally, dodge vertically
                    if (projDy > 0) {
                        // Projectile is below, dodge up
                        self.Rotation = -90;
                        self.Bounds.Position.Y -= speed * deltaTime;
                    } else {
                        // Projectile is above, dodge down
                        self.Rotation = 90;
                        self.Bounds.Position.Y += speed * deltaTime;
                    }
                } else {
                    // Projectile moving vertically, dodge horizontally
                    if (projDx > 0) {
                        // Projectile is to the right, dodge left
                        self.Rotation = 180;
                        self.Bounds.Position.X -= speed * deltaTime;
                    } else {
                        // Projectile is to the left, dodge right
                        self.Rotation = 0;
                        self.Bounds.Position.X += speed * deltaTime;
                    }
                }
            }
            
            // Only chase player if not dodging
            if (!isDodging && player != null) {
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
                    float alignmentThreshold = 10f;

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
                        fireCooldown = 1f + (float) random.NextDouble() * 2f;
                    }

                    // Move only in cardinal directions (no diagonals)
                    float speed = 25f;

                    if (snappedAngle == 0) {
                        self.Bounds.Position.X += speed * deltaTime;
                    } else if (snappedAngle == 180 || snappedAngle == -180) {
                        self.Bounds.Position.X -= speed * deltaTime;
                    } else if (snappedAngle == -90) {
                        self.Bounds.Position.Y -= speed * deltaTime;
                    } else if (snappedAngle == 90) {
                        self.Bounds.Position.Y += speed * deltaTime;
                    }
                }
            }
        }
    }
}