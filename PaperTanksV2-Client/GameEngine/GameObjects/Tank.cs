using Cairo;
using Newtonsoft.Json;
using PaperTanksV2Client.GameEngine.AI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public class Tank : GameObject
    {
        [JsonProperty("isPlayer")]
        public bool IsPlayer { get; set; } = false;

        [JsonProperty("weapon0")]
        public Weapon Weapon0 { get; set; } = null;

        [JsonProperty("weapon1")]
        public Weapon Weapon1 { get; set; } = null;

        [JsonProperty("weapon2")]
        public Weapon Weapon2 { get; set; } = null;
        
        [JsonProperty("aiAgent")]
        public AIAgent AiAgent { get; set; } = null;

        private SKTypeface MenuTypeface = null;
        private SKFont MenuFont = null;
        private SKTypeface SecondMenuTypeface = null;
        private SKFont SecondMenuFont = null;
        private Action<Game> playerDiedCallback = null;
        
        public Tank(bool isPlayer, Weapon w0, Weapon w1, Weapon w2,
            SKTypeface MenuTypeface,
                SKFont MenuFont,
                SKTypeface SecondMenuTypeface,
                SKFont SecondMenuFont,
            Action<Game> playerDiedCallback) : base(){
            this.IsPlayer = isPlayer;
            this.Health = 100;
            this.Weapon0 = w0;
            this.Weapon1 = w1;
            this.Weapon2 = w2;
            this.MenuTypeface =  MenuTypeface; 
            this.MenuFont =  MenuFont; 
            this.SecondMenuTypeface =  SecondMenuTypeface; 
            this.SecondMenuFont =  SecondMenuFont;
            this.Bounds = new BoundsData(new Vector2Data(0, 0), new Vector2Data(50, 50));
            this.playerDiedCallback = playerDiedCallback;
        }

        public override void HandleCollision(Game game, GameObject other)
        {
            if (other == null){
                return;
            }
            bool intersects = this.Bounds.Intersects(other.Bounds);
            if (other is Projectile && intersects) {
                this.Health -= (other as Projectile).Damage;
                if (this.Health <= 0) this.Health = 0;
                if (!this.IsPlayer && this.Health <= 0) {
                    this.deleteSelf();
                } else if(this.Health <= 0){
                    this.playerDiedCallback?.Invoke(game);
                }
            }
            if (other is Wall) {
                bool intersectsWall = this.Bounds.IntersectsWhenRotated(other.Bounds, other.Rotation);
                if (intersectsWall) {
                    this.Bounds = this.Bounds.GetNonIntersectingPosition(other.Bounds);
                }
            }
            if (other is Tank && intersects && this.IsPlayer) {
                this.Bounds = this.Bounds.GetNonIntersectingPosition(other.Bounds);
            }
        }

        public override void Update(GameEngineInstance engine, Single deltaTime)
        {
            if (!this.IsPlayer) {
                if (AiAgent != null) {
                    this.AiAgent.Update(this, engine, deltaTime);
                }
            }
        }

        public Projectile Fire(GameEngineInstance engine)
        {
            Projectile projectile = new Projectile(SKColors.Red);
            Vector2Data size = new Vector2Data(8, 8);
            float movementSpeed = 100;
            if (this.Rotation == 0) {
                projectile.Bounds =
                    new BoundsData(
                        new Vector2Data(this.Position.X + 100,
                            this.Position.Y + ( this.Size.Y / 2 ) - ( size.Y / 2 )), size);
                projectile.Velocity = new Vector2Data(movementSpeed, 0);
            } else if (this.Rotation == -180) {
                projectile.Bounds =
                    new BoundsData(
                        new Vector2Data(this.Position.X - 58,
                            this.Position.Y + ( this.Size.Y / 2 ) - ( size.Y / 2 )), size);
                projectile.Velocity = new Vector2Data(-movementSpeed, 0);
            } else if (this.Rotation == -90) {
                projectile.Bounds =
                    new BoundsData(
                        new Vector2Data(this.Position.X + ( this.Size.X / 2 ) - ( size.X / 2 ),
                            this.Position.Y - 58), size);
                projectile.Velocity = new Vector2Data(0, -movementSpeed);
            } else if (this.Rotation == 90) {
                projectile.Bounds =
                    new BoundsData(
                        new Vector2Data(this.Position.X + ( this.Size.X / 2 ) - ( size.X / 2 ),
                            this.Position.Y + 100), size);
                projectile.Velocity = new Vector2Data(0, movementSpeed);
            }
            return projectile;
        }

        public override void Render(Game game, SKCanvas canvas, float? centerX = null, float? centerY = null)
        {
            float innerSpacing = 8f;
            float gunXOffset = 18;
            float gunYOffset = 5;
            float gunXSize = 50;
            float gunYSize = 10;
            canvas.DrawRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Size.X, this.Bounds.Size.Y, new SKPaint() {
                Color = this.IsPlayer ? SKColors.Green : SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            });
            canvas.DrawCircle(this.Bounds.Position.X + (this.Bounds.Size.X / 2), this.Bounds.Position.Y + (this.Bounds.Size.Y / 2), (this.Bounds.Size.X / 2) - innerSpacing, new SKPaint() {
                Color = this.IsPlayer ? SKColors.Green : SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            });
            canvas.DrawRect(this.Bounds.Position.X + (this.Bounds.Size.X / 2) + gunXOffset, this.Bounds.Position.Y + (this.Bounds.Size.Y / 2) - gunYOffset, gunXSize, gunYSize, new SKPaint() {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 2f,
            });
            canvas.DrawRect(this.Bounds.Position.X + (this.Bounds.Size.X / 2) + gunXOffset, this.Bounds.Position.Y + (this.Bounds.Size.Y / 2) - gunYOffset, gunXSize, gunYSize, new SKPaint() {
                Color = this.IsPlayer ? SKColors.Green : SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
            });
            if (centerX != null && centerY != null) {
                canvas.RotateDegrees(this.Rotation * -1, (float)centerX, (float)centerY);
                int ammoCount = this.Weapon0.AmmoCount;
                canvas.DrawText($"{this.Health.ToString()}", this.Bounds.Position.X + ( this.Bounds.Size.X / 2 ),
                    this.Bounds.Position.Y + ( this.Bounds.Size.Y / 2 ) - 2, new SKPaint() {
                        Color = SKColors.Green,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2f,
                        TextAlign = SKTextAlign.Center,
                        TextSize = 14
                    });
                canvas.DrawText($"{ammoCount}", this.Bounds.Position.X + ( this.Bounds.Size.X / 2 ),
                    this.Bounds.Position.Y + ( this.Bounds.Size.Y / 2 ) + 10, new SKPaint() {
                        Color = SKColors.Orange,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2f,
                        TextAlign = SKTextAlign.Center,
                        TextSize = 14
                    });
                canvas.RotateDegrees(this.Rotation, (float)centerX, (float)centerY);
            }
        }

        protected override ObjectType GetObjectType()
        {
            return this.IsPlayer ? ObjectType.Player : ObjectType.Enemy;
        }
    }
}
