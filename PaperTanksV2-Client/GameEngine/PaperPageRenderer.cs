using SkiaSharp;
using System;
using System.Numerics;

namespace PaperTanksV2Client.GameEngine
{
        public class PaperPageRenderer
    {
        private int pageWidth;
        private int pageHeight;
        private int spacing;
        private int totalLines;

        public PaperPageRenderer(int pageWidth, int pageHeight, int spacing = 20, int totalLines = 60)
        {
            this.pageWidth = pageWidth;
            this.pageHeight = pageHeight;
            this.spacing = spacing;
            this.totalLines = totalLines;
        }

        /// <summary>
        /// Renders a single right-hand page that fills the viewport and moves with the world
        /// </summary>
        public void Render(SKCanvas canvas, ViewPort viewPort)
        {
            canvas.Save();

            // Apply viewport offset to align with world coordinates
            canvas.Translate(viewPort.Offset.X, viewPort.Offset.Y);

            // Calculate the page position in world space to fill the viewport
            float worldX = viewPort.View.Position.X;
            float worldY = viewPort.View.Position.Y;

            // White paper background
            using (var whitePaint = new SKPaint())
            {
                whitePaint.Color = SKColors.White;
                whitePaint.Style = SKPaintStyle.Fill;
                canvas.DrawRect(new SKRect(worldX, worldY, worldX + pageWidth, worldY + pageHeight), whitePaint);
            }

            // Red margin line (left side for right-hand page)
            using (var redLinePaint = new SKPaint())
            {
                redLinePaint.Color = new SKColor(255, 182, 193); // Light red/pink
                redLinePaint.StrokeWidth = 2;
                redLinePaint.IsAntialias = true;

                float redLineX = worldX + spacing; // ✅ Just use world coordinates
                canvas.DrawLine(redLineX, worldY, redLineX, worldY + pageHeight, redLinePaint);
            }

            // Blue horizontal lines
            using (var blueLinePaint = new SKPaint())
            {
                blueLinePaint.Color = new SKColor(173, 216, 230); // Light blue
                blueLinePaint.StrokeWidth = 1;
                blueLinePaint.IsAntialias = true;

                int lineSpacing = pageHeight / totalLines;
                for (int i = 1; i < totalLines; i++)
                {
                    float y = worldY + (i * lineSpacing) + spacing / 8;
                    canvas.DrawLine(worldX, y, worldX + pageWidth, y, blueLinePaint);
                }
            }

            canvas.Restore();
        }

        /// <summary>
        /// Renders the page fixed to screen coordinates (doesn't move with viewport)
        /// </summary>
        public void RenderFixed(SKCanvas canvas)
        {
            // White paper background
            using (var whitePaint = new SKPaint())
            {
                whitePaint.Color = SKColors.White;
                whitePaint.Style = SKPaintStyle.Fill;
                canvas.DrawRect(new SKRect(0, 0, pageWidth, pageHeight), whitePaint);
            }

            // Red margin line (left side for right-hand page)
            using (var redLinePaint = new SKPaint())
            {
                redLinePaint.Color = new SKColor(255, 182, 193); // Light red/pink
                redLinePaint.StrokeWidth = 2;
                redLinePaint.IsAntialias = true;

                float redLineX = spacing;
                canvas.DrawLine(redLineX, 0, redLineX, pageHeight, redLinePaint);
            }

            // Blue horizontal lines
            using (var blueLinePaint = new SKPaint())
            {
                blueLinePaint.Color = new SKColor(173, 216, 230); // Light blue
                blueLinePaint.StrokeWidth = 1;
                blueLinePaint.IsAntialias = true;

                int lineSpacing = pageHeight / totalLines;
                for (int i = 1; i < totalLines; i++)
                {
                    float y = (i * lineSpacing) + spacing / 8;
                    canvas.DrawLine(0, y, pageWidth, y, blueLinePaint);
                }
            }
        }

        // Property setters for dynamic updates
        public void SetPageDimensions(int width, int height)
        {
            pageWidth = width;
            pageHeight = height;
        }

        public void SetLineCount(int lines)
        {
            totalLines = lines;
        }
    }
}