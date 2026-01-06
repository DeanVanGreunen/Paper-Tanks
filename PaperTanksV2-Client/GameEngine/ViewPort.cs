using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
        public class ViewPort
    {
        private BoundsData view;
        private Vector2 offset;

        public BoundsData View => view;
        public Vector2 Offset => offset;

        public ViewPort(Vector2Data viewSize)
        {
            this.view = new BoundsData(
                new Vector2Data(0, 0),
                viewSize
            );
            this.offset = new Vector2(0, 0);
        }

        public void MoveBy(float X, float Y)
        {
            offset.X += X;
            offset.Y += Y;
        }

        /// <summary>
        /// Centers the viewport around the given player GameObject
        /// </summary>
        public void CenterAround(GameObject player)
        {
            if (player == null) return;

            // Calculate the center position
            float centerX = player.Position.X - (view.Size.X / 2);
            float centerY = player.Position.Y - (view.Size.Y / 2);

            view.Position = new Vector2Data(centerX, centerY);
            
            // Store offset for rendering transformations
            offset = new Vector2(-centerX, -centerY);
        }

        /// <summary>
        /// Renders all visible GameObjects using SkiaSharp canvas
        /// </summary>
        public void Render(Game game, SKCanvas canvas, List<GameObject> visibleObjects)
        {
            if (canvas == null) return;
            if (visibleObjects == null) return;
            // Apply viewport transformation
            // canvas.Translate(offset.X, offset.Y);
            
            // Render each visible object
            foreach (GameObject obj in visibleObjects)
            {
                if (obj == null || obj.deleteMe)
                {
                    Console.WriteLine("Object Invalid or Deleted");
                    continue;
                };
                RenderGameObject(game, canvas, obj);
            }
        }

        /// <summary>
        /// Renders an individual GameObject (override or extend for custom rendering)
        /// </summary>
        protected virtual void RenderGameObject(Game game, SKCanvas canvas, GameObject obj)
        {
            // Draw bounds for debugging
            if (obj.Bounds != null) {
                obj.InternalRender(game, canvas);
            }
            // Actual object rendering would go here
        }

        /// <summary>
        /// Converts world coordinates to screen coordinates
        /// </summary>
        public Vector2 WorldToScreen(Vector2 worldPos)
        {
            return new Vector2(
                worldPos.X - view.Position.X,
                worldPos.Y - view.Position.Y
            );
        }

        /// <summary>
        /// Converts screen coordinates to world coordinates
        /// </summary>
        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            return new Vector2(
                screenPos.X + view.Position.X,
                screenPos.Y + view.Position.Y
            );
        }

        /// <summary>
        /// Checks if a GameObject is within the viewport
        /// </summary>
        public bool IsVisible(GameObject obj)
        {
            if (obj?.Bounds == null) return false;
            return view.getRectangle().Intersects(obj.Bounds.getRectangle());
        }

        /// <summary>
        /// Updates viewport size (useful for window resizing)
        /// </summary>
        public void SetViewSize(Vector2Data newSize)
        {
            view.Size = newSize;
        }
    }
}
