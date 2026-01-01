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
        public Guid Id { get; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public Vector2 Scale { get; set; }
        public bool IsStatic { get; set; }
        public Rectangle Bounds { get; set; }
        public float Health { get; protected set; }
        public float Mass { get; protected set; }
        public CompositeCollider Collider { get; protected set; }
        protected Dictionary<string, object> CustomProperties;
        readonly String[] ALLOWED_GAMEOBJECTS = new String[] {
            "RECT",
            "CIRCLE",
            "TRIANGLE",
            "IMAGE"
        };

        protected GameObject()
        {
            Id = Guid.NewGuid();
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Rotation = 0f;
            AngularVelocity = 0f;
            Scale = Vector2.One;
            Health = 100f;
            Mass = 1f; // Default mass
            CustomProperties = new Dictionary<string, object>();
            Collider = new CompositeCollider(this);
            this.CustomProperties["render_type"] = "NOT_SET";
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

        public virtual void ApplyState(GameObjectState state)
        {
            Position = state.Position;
            Velocity = state.Velocity;
            Rotation = state.Rotation;
            AngularVelocity = state.AngularVelocity;
            Scale = state.Scale;
            Health = state.Health;
            Mass = state.Mass;
            CustomProperties = new Dictionary<string, object>(state.CustomProperties);
            // Update collider transforms after applying new state
            Collider.UpdateTransforms();
        }

        protected abstract ObjectType GetObjectType();

        public abstract void Update(float deltaTime);

        public abstract void HandleCollision(GameObject other);

        public void SetCustomProperty(string key, string value) {
            this.CustomProperties[key] = value;
        }
        public void Render(SKCanvas canvas) {
            if (!this.CustomProperties.ContainsKey("RENDER_TYPE")) {
                // TODO: DRAW ERROR OPVERLAY
                return;
            }
            if (!this.ALLOWED_GAMEOBJECTS.ToList().Contains(this.CustomProperties["RENDER_TYPE"])) {
                // TODO: DRAW CONSOLE ERROR, AND DRAW ERROR OVERLAY
                return;
            }
            // DRAW SPECIFIC TYPE OF GameObject
            // = RECT (Store Each Vertex, Color, Border Size, Border Color)
            // = CIRCLE (Center and Radius, Color, Border Size, Border Color)
            // = TRIANGLE (Store Each Vertext, Color, Border Size, Border Color)
            // = IMAGE (Load Image Data From GameResources, Border Size, Border Color)
            if (!this.CustomProperties.ContainsKey("VECTOR_LIST")) {
                // TODO: DRAW ERROR OPVERLAY
                return;
            }
            float[] vectorList = this.CustomProperties["VECTOR_LIST"]
           .ToString()
           .Split(',')
           .Select(s => float.Parse(s.Trim()))
           .ToArray();
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
            switch (this.CustomProperties["RENDER_TYPE"]) {
                case "RECT":
                    if (vectorList.Count() != 8) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }                    
                    // TODO: DRAW GAME OBJECT
                    // = this.CustomProperties["VECTOR_LIST"] = [
                    //      [0.00, 0.00],
                    //      [0.00, 0.00],
                    //      [0.00, 0.00],
                    //      [0.00, 0.00],
                    // ];
                    // = this.CustomProperties["RENDER_COLOR"] = "hex_color";
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
                    // TODO: DRAW GAME OBJECT
                    // TODO: DRAW GAME OBJECT
                    // = this.CustomProperties["VECTOR_LIST"] = [
                    //      [0.00, 0.00],
                    // ];
                    // = this.CustomProperties["RENDER_COLOR"] = "hex_color"
                    // = this.CustomProperties["RENDER_BORDER_SIZE"] = "1";
                    // = this.CustomProperties["RENDER_BORDER_COLOR"] = "hex_color";
                    break;
                case "TRIANGLE":
                    if (!this.CustomProperties.ContainsKey("VECTOR_LIST")) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
                    if (vectorList.Count() != 6) {
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
                    // TODO: DRAW GAME OBJECT
                    // = this.CustomProperties["VECTOR_LIST"] = [
                    //      [0.00, 0.00],
                    //      [0.00, 0.00],
                    //      [0.00, 0.00],
                    // ];
                    // = this.CustomProperties["RENDER_BORDER_SIZE"] = "1";
                    // = this.CustomProperties["RENDER_BORDER_COLOR"] = "hex_color";
                    break;
                case "IMAGE":
                    if (vectorList.Count() != 8) {
                        // TODO: DRAW ERROR OPVERLAY
                        return;
                    }
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
                    // TODO: DRAW GAME OBJECT
                    // = this.CustomProperties["IMAGE_RESOURCE_NAME"] = "named image resource";
                    // = this.CustomProperties["RENDER_BORDER_SIZE"] = "1";
                    // = this.CustomProperties["RENDER_BORDER_COLOR"] = "hex_color";
                    break;
                default:
                    // TODO: DRAW CONSOLE ERROR, AND DRAW ERROR OVERLAY
                    break;
            }
        }
    }
}
