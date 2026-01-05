using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public abstract class GameObject
    {
        [JsonIgnore]
        public bool deleteMe = false;
        public Guid Id { get; }
        [JsonIgnore]
        public Vector2Data Position { get { return this.Bounds.Position;  } }
        [JsonIgnore]
        public Vector2Data Size { get { return this.Bounds.Size;  } }
        [JsonProperty("Velocity")]
        public Vector2Data Velocity { get; set; }
        [JsonProperty("Rotation")]
        public float Rotation { get; set; }
        [JsonProperty("AngularVelocity")]
        public float AngularVelocity { get; set; }
        [JsonProperty("Scale")]
        public Vector2Data Scale { get; set; }
        [JsonProperty("IsStatic")]
        public bool IsStatic { get; set; }
        [JsonProperty("Bounds")]
        public BoundsData Bounds { get; set; }
        [JsonProperty("Health")]
        public float Health { get; protected set; }
        [JsonProperty("Mass")]
        public float Mass { get; protected set; }
        [JsonIgnore]
        public CompositeCollider Collider { get; protected set; }
        [JsonProperty("CustomProperties")]
        public Dictionary<string, object> CustomProperties { get; set; }
        readonly String[] ALLOWED_GAMEOBJECTS = new String[] {
            "RECT",
            "CIRCLE",
            "TRIANGLE",
            "IMAGE"
        };
        private SKImage imageData;

        public void deleteSelf() {
            this.deleteMe = true;
        }

        protected GameObject()
        {
            Id = Guid.NewGuid();
            this.Bounds = new BoundsData(new Vector2Data(0, 0), new Vector2Data(0, 0));
            Velocity = Vector2Data.Zero;
            Rotation = 0f;
            AngularVelocity = 0f;
            Scale = Vector2Data.One;
            Health = 100f;
            Mass = 1f; // Default mass
            CustomProperties = new Dictionary<string, object>();
            Collider = new CompositeCollider(this);
            this.CustomProperties["RENDER_TYPE"] = "NOT_SET";
        }

        public void MoveBy(float X, float Y)
        {
            this.Bounds.Position.X += X;
            this.Bounds.Position.Y += Y;
        }

        public virtual GameObjectState GetState()
        {
            return new GameObjectState {
                Position = this.Position,
                Velocity = this.Velocity,
                Rotation = this.Rotation,
                AngularVelocity = this.AngularVelocity,
                Scale = this.Scale,
                IsActive = true,
                Health = this.Health,
                Mass = this.Mass,
                Type = GetObjectType(),
                CustomProperties = new Dictionary<string, object>(CustomProperties),
                TimeStamp = DateTime.UtcNow
            };
        }

        public virtual void LoadImageData(Game game) {
            if (game == null) {
                return; // Game not valid? big bug...
            }
            if (!this.CustomProperties.ContainsKey("IMAGE_RESOURCE_NAME")) return;
            this.imageData = game.resources.Get(ResourceManagerFormat.Image, this.CustomProperties["IMAGE_RESOURCE_NAME"].ToString()) as SKImage;
        }

        public void LoadAsRect(string data) {
            this.LoadRect(this.LoadVertexData(data));
        }

        public virtual float[] LoadVertexData(string VECTORLIST) {
            return VECTORLIST.Split(',')
           .Select(s => float.Parse(s.Trim()))
           .ToArray();
        }

        public virtual void LoadRect(float[] values)
        {
            if (values == null) return;
            if (values.Count() != 4) return;
            this.Bounds = new BoundsData(new Vector2Data(values[0], values[1]), new Vector2Data(values[2], values[3]));
        }

        public virtual void ApplyState(GameObjectState state)
        {
            if (state == null) return;
            this.Bounds.Position = new Vector2Data(state.Position.X, state.Position.Y);
            Velocity = state.Velocity;
            Rotation = state.Rotation;
            AngularVelocity = state.AngularVelocity;
            Scale = state.Scale;
            Health = state.Health;
            Mass = state.Mass;
            CustomProperties = new Dictionary<string, object>(state.CustomProperties);
            Collider.UpdateTransforms();
        }

        protected virtual ObjectType GetObjectType() { return ObjectType.None; }

        public virtual void Update(float deltaTime) { }

        public virtual void HandleCollision(GameObject other) { }

        public void SetCustomProperty(string key, string value) {
            this.CustomProperties[key] = value;
        }

        public void InternalRender(Game game, SKCanvas canvas) {
            if (!this.CustomProperties.ContainsKey("RENDER_TYPE")) {
                // TODO: DRAW ERROR OPVERLAY
                return;
            }
            if (!this.ALLOWED_GAMEOBJECTS.ToList().Contains(this.CustomProperties["RENDER_TYPE"])) {
                // TODO: DRAW CONSOLE ERROR, AND DRAW ERROR OVERLAY
                return;
            }
            // DRAW SPECIFIC TYPE OF GameObject
            // = TRIANGLE (Store Each Vertext, Color, Border Size, Border Color)
            if (!this.CustomProperties.ContainsKey("RENDER_COLOR")) {
                // TODO: DRAW ERROR OPVERLAY
                return;
            }
            if (!this.CustomProperties.ContainsKey("RENDER_BORDER_SIZE")) {
                // TODO: DRAW ERROR OPVERLAY
                return;
            }
            if (!this.CustomProperties.ContainsKey("RENDER_BORDER_COLOR")) {
                // TODO: DRAW ERROR OPVERLAY
                return;
            }
            string Color = this.CustomProperties["RENDER_COLOR"].ToString();
            float bSize = Single.Parse(this.CustomProperties["RENDER_BORDER_SIZE"].ToString());
            string bColor = this.CustomProperties["RENDER_BORDER_COLOR"].ToString();
            SKPaint pFill = new SKPaint {
                Color = SKColor.Parse(Color),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            SKPaint pStroke = new SKPaint {
                Color = SKColor.Parse(bColor),
                StrokeWidth = bSize,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Square,
                StrokeJoin = SKStrokeJoin.Miter
            };
            SKRect imageDest = new SKRect(this.Bounds.Position.X, this.Bounds.Position.Y, this.Bounds.Position.X + this.Bounds.Size.X, this.Bounds.Position.Y + this.Bounds.Size.Y);
            switch (this.CustomProperties["RENDER_TYPE"]) {
                case "RECT":
                    canvas.DrawRect(imageDest, pFill);
                    canvas.DrawRect(imageDest, pStroke);
                    break;
                case "CIRCLE":
                    if (!this.CustomProperties.ContainsKey("RENDER_COLOR")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (!this.CustomProperties.ContainsKey("RENDER_BORDER_SIZE")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (!this.CustomProperties.ContainsKey("RENDER_BORDER_COLOR")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    canvas.DrawRect(imageDest, pFill);
                    canvas.DrawRect(imageDest, pStroke);
                    break;
                case "TRIANGLE":
                    return; // NOT USING THIS YET
                    if (!this.CustomProperties.ContainsKey("RENDER_COLOR")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (!this.CustomProperties.ContainsKey("RENDER_BORDER_SIZE")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (!this.CustomProperties.ContainsKey("RENDER_BORDER_COLOR")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    // TODOs: Finish at some point
                    break;
                case "IMAGE":
                    if (!this.CustomProperties.ContainsKey("IMAGE_RESOURCE_NAME")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (!this.CustomProperties.ContainsKey("RENDER_COLOR")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (!this.CustomProperties.ContainsKey("RENDER_BORDER_SIZE")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (!this.CustomProperties.ContainsKey("RENDER_BORDER_COLOR")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    canvas.DrawRect(imageDest, pFill);
                    canvas.DrawRect(imageDest, pStroke);
                    canvas.DrawImage(imageData, imageDest);
                    break;
                default:
                    // TODO: DRAW CONSOLE ERROR, AND DRAW ERROR OVERLAY
                    break;
            }

            this.Render(game, canvas);
        }
        public abstract void Render(Game game, SKCanvas canvas);
    }
}
