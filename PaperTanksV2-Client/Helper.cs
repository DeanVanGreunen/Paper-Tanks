using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PaperTanksV2Client
{
    static class Helper
    {
        static SKPaint whitePaint = new SKPaint
        {
            StrokeWidth = 1f,
            Color = SKColors.White,
            IsAntialias = false
        };
        static SKPaint blueLinePaint = new SKPaint
        {
            StrokeWidth = 1f,
            Color = SKColor.Parse("#58aff3"),
            IsAntialias = false
        };
        static SKPaint redLinePaint = new SKPaint
        {
            StrokeWidth = 1f,
            Color = SKColor.Parse("#ff0000"),
            IsAntialias = false
        };
        private static ushort[] ConvertToUShortArray(string text)
        {
            // Get UTF-16 encoded bytes
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(text);

            // Convert the byte array into ushort array
            ushort[] textBuffer = new ushort[utf16Bytes.Length / 2];
            for (int i = 0; i < textBuffer.Length; i++)
            {
                textBuffer[i] = BitConverter.ToUInt16(utf16Bytes, i * 2);
            }

            return textBuffer;
        }
        public static void DrawCenteredText(SKCanvas canvas, string text, SKRect rect, SKFont font, SKPaint paint)
        {
            ushort[] textBuffer = ConvertToUShortArray(text);
            int test = 1;
            if (test == 1)
            {
                // Measure the text's bounds
                SKRect textBounds = new SKRect();
                font.MeasureText(textBuffer, out textBounds, paint);
                // Calculate horizontal and vertical alignment
                float x = rect.Left + (rect.Width - textBounds.Width) / 2 - textBounds.Left;
                float y = rect.Top + (rect.Height - textBounds.Height) / 2 - textBounds.Top;
                // Draw the text
                canvas.DrawText(text, x, y, font, paint);
            } else if (test == 2)
            {
                // Dynamically adjust font size based on button height
                float desiredFontSize = rect.Height * 0.6f; // 60% of the button height
                font.Size = desiredFontSize;

                // Measure the text bounds
                SKRect textBounds = new SKRect();
                float textWidth = font.MeasureText(textBuffer, out textBounds, paint);

                // Calculate horizontal and vertical alignment
                float x = rect.Left + (rect.Width - textWidth) / 2;
                float y = rect.Top + (rect.Height + textBounds.Height) / 2 - textBounds.Bottom;

                // Draw the text
                canvas.DrawText(text, x, y, paint);
            }
        }
        public static SKImage DrawCoverPageAsImage(int pageWidth, int pageHeight)
        {
            SKImageInfo info = new SKImageInfo(pageWidth, pageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (SKSurface surface = SKSurface.Create(info))
            {
                SKCanvas canvas = surface.Canvas;
                //canvas.Clear(SKColors.Transparent);
                canvas.Clear(SKColors.Black);
                // Draw Cover Page Here
                canvas.Flush();
                return surface.Snapshot();
            }
        }
        public static SKImage DrawPageAsImage(bool isLeftPage, int pageWidth, int pageHeight, int totalLines = 90, int spacing = 28)
        {
            SKImageInfo info = new SKImageInfo(pageWidth, pageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (SKSurface surface = SKSurface.Create(info))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                DrawPage(canvas, isLeftPage, pageWidth, pageHeight, totalLines, spacing);
                canvas.Flush();
                return surface.Snapshot();
            }
        }

        public static void DrawPage(SKCanvas canvas, bool isLeftPage, int pageWidth, int pageHeight, int totalLines = 90, int spacing = 28) {
            // Draw Blank Page
            canvas.DrawRect(new SKRect(0, 0, pageWidth, pageHeight), whitePaint);

            // Calculate line spacing
            int lineSpacing = pageHeight / totalLines;

            // Draw Horizontal Blue Lines
            for (int i = 1; i < totalLines; i++) // Start from 1 to leave the top margin
            {
                int y = i * lineSpacing + spacing;
                canvas.DrawLine(0, y, pageWidth, y, blueLinePaint);
            }

            // Draw Vertical Red Line
            float redLineX = isLeftPage ? pageWidth - spacing : spacing;
            canvas.DrawLine(redLineX, 0, redLineX, pageHeight, redLinePaint);
        }
        public static void RenderPageFlipFromBitmaps(
            SKCanvas canvas,
            SKBitmap frontImage,
            SKBitmap backImage,
            float centerX,
            float centerY,
            float pageWidth,
            float pageHeight,
            float t,
            float outputOffsetX = 0,
            float outputOffsetY = 0)
        {
            // Ensure t is clamped between 0 and 1
            t = Math.Clamp(t, 0, 1);

            // Calculate the flipping angle (0 to PI radians)
            float angle = t * (float)Math.PI;

            // Cosine and sine of the angle for trapezoid deformation
            float cosAngle = (float)Math.Cos(angle);
            float sinAngle = (float)Math.Sin(angle);

            // Adjust position based on output offset
            float adjustedCenterX = centerX + outputOffsetX;
            float adjustedCenterY = centerY + outputOffsetY;

            // Calculate half-dimensions
            float halfWidth = pageWidth / 2;
            float halfHeight = pageHeight / 2;

            // Trapezoid vertices for the front page
            SKPoint[] frontVertices = new SKPoint[]
            {
        new SKPoint(adjustedCenterX - halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-left
        new SKPoint(adjustedCenterX + halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-right
        new SKPoint(adjustedCenterX + halfWidth, adjustedCenterY + halfHeight),           // Bottom-right
        new SKPoint(adjustedCenterX - halfWidth, adjustedCenterY + halfHeight)            // Bottom-left
            };
            // Trapezoid vertices for the back page
            SKPoint[] backVertices = new SKPoint[]
            {
        new SKPoint(adjustedCenterX + halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-left (mirrored)
        new SKPoint(adjustedCenterX - halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-right (mirrored)
        new SKPoint(adjustedCenterX - halfWidth, adjustedCenterY + halfHeight),           // Bottom-right (mirrored)
        new SKPoint(adjustedCenterX + halfWidth, adjustedCenterY + halfHeight)            // Bottom-left (mirrored)
            };
            // Apply sine effect for dynamic height scaling during the flip
            for (int i = 0; i < frontVertices.Length; i++)
            {
                frontVertices[i].Y += sinAngle * (i < 2 ? -halfHeight : halfHeight) * 0.3f;
                backVertices[i].Y += sinAngle * (i < 2 ? -halfHeight : halfHeight) * 0.3f;
            }

            // Determine which side to render based on t
            if (t <= 0.5)
            {
                // Render the front side of the page
                Helper.DrawImageOnTrapezoidFromBitmap(canvas, frontImage, frontVertices);
            }
            else
            {
                // Render the back side of the page
                Helper.DrawImageOnTrapezoidFromBitmap(canvas, backImage, backVertices);
            }
        }

        public static void RenderPageFlipFromImages(
    SKCanvas canvas,
    SKImage frontImage,
    SKImage backImage,
    float centerX,
    float centerY,
    float pageWidth,
    float pageHeight,
    float t,
    float outputOffsetX = 0,
    float outputOffsetY = 0)
        {
            // Ensure t is clamped between 0 and 1
            t = Math.Clamp(t, 0, 1);

            // Calculate the flipping angle (0 to PI radians)
            float angle = t * (float)Math.PI;

            // Cosine and sine of the angle for trapezoid deformation
            float cosAngle = (float)Math.Cos(angle);
            float sinAngle = (float)Math.Sin(angle);

            // Adjust position based on output offset
            float adjustedCenterX = centerX + outputOffsetX;
            float adjustedCenterY = centerY + outputOffsetY;

            // Calculate half-dimensions
            float halfWidth = pageWidth / 2;
            float halfHeight = pageHeight / 2;

            // Trapezoid vertices for the front page
            SKPoint[] frontVertices = new SKPoint[]
            {
        new SKPoint(adjustedCenterX - halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-left
        new SKPoint(adjustedCenterX + halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-right
        new SKPoint(adjustedCenterX + halfWidth, adjustedCenterY + halfHeight),           // Bottom-right
        new SKPoint(adjustedCenterX - halfWidth, adjustedCenterY + halfHeight)            // Bottom-left
            };

            // Trapezoid vertices for the back page
            SKPoint[] backVertices = new SKPoint[]
            {
        new SKPoint(adjustedCenterX + halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-left (mirrored)
        new SKPoint(adjustedCenterX - halfWidth * cosAngle, adjustedCenterY - halfHeight), // Top-right (mirrored)
        new SKPoint(adjustedCenterX - halfWidth, adjustedCenterY + halfHeight),           // Bottom-right (mirrored)
        new SKPoint(adjustedCenterX + halfWidth, adjustedCenterY + halfHeight)            // Bottom-left (mirrored)
            };

            // Apply sine effect for dynamic height scaling during the flip
            for (int i = 0; i < frontVertices.Length; i++)
            {
                frontVertices[i].Y += sinAngle * (i < 2 ? -halfHeight : halfHeight) * 0.3f;
                backVertices[i].Y += sinAngle * (i < 2 ? -halfHeight : halfHeight) * 0.3f;
            }

            // Determine which side to render based on t
            if (t <= 0.5)
            {
                // Render the front side of the page
                Helper.DrawImageOnTrapezoidFromImage(canvas, frontImage, frontVertices);
            }
            else
            {
                // Render the back side of the page
                Helper.DrawImageOnTrapezoidFromImage(canvas, backImage, backVertices);
            }
        }

        public static void DrawImageOnTrapezoidFromBitmap(SKCanvas canvas, SKBitmap image, SKPoint[] trapezoidVertices)
        {
            if (trapezoidVertices.Length != 4)
            {
                throw new ArgumentException("Trapezoid must have exactly 4 vertices.");
            }

            // Define the source rectangle (image bounds)
            float srcWidth = image.Width;
            float srcHeight = image.Height;
            var srcRect = new SKPoint[]
            {
        new SKPoint(0, 0),               // Top-left
        new SKPoint(srcWidth, 0),        // Top-right
        new SKPoint(srcWidth, srcHeight),// Bottom-right
        new SKPoint(0, srcHeight)        // Bottom-left
            };

            // Build the perspective transformation matrix
            var matrix = Helper.CreatePerspectiveMatrix(srcRect, trapezoidVertices);

            // Apply the perspective transformation and draw the image
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.FilterQuality = SKFilterQuality.High;

                // Set the transformation matrix on the canvas
                canvas.Save();
                canvas.SetMatrix(matrix);
                canvas.DrawBitmap(image, 0, 0, paint); // Draw the image with the applied matrix
                canvas.Restore();
            }
        }
        public static void DrawImageOnTrapezoidFromImage(SKCanvas canvas, SKImage image, SKPoint[] trapezoidVertices)
        {
            if (trapezoidVertices.Length != 4)
            {
                throw new ArgumentException("Trapezoid must have exactly 4 vertices.");
            }

            // Define the source rectangle (image bounds)
            float srcWidth = image.Width;
            float srcHeight = image.Height;
            var srcRect = new SKPoint[]
            {
        new SKPoint(0, 0),               // Top-left
        new SKPoint(srcWidth, 0),        // Top-right
        new SKPoint(srcWidth, srcHeight),// Bottom-right
        new SKPoint(0, srcHeight)        // Bottom-left
            };

            // Build the perspective transformation matrix
            var matrix = Helper.CreatePerspectiveMatrix(srcRect, trapezoidVertices);

            // Apply the perspective transformation and draw the image
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.FilterQuality = SKFilterQuality.High;

                // Set the transformation matrix on the canvas
                canvas.Save();
                canvas.SetMatrix(matrix);
                canvas.DrawImage(image, 0, 0, paint); // Draw the image with the applied matrix
                canvas.Restore();
            }
        }

        private static SKMatrix CreatePerspectiveMatrix(SKPoint[] src, SKPoint[] dst)
        {
            if (src.Length != 4 || dst.Length != 4)
            {
                throw new ArgumentException("Both source and destination points must have exactly 4 elements.");
            }
            float srcX0 = src[0].X, srcY0 = src[0].Y;
            float srcX1 = src[1].X, srcY1 = src[1].Y;
            float srcX2 = src[2].X, srcY2 = src[2].Y;
            float srcX3 = src[3].X, srcY3 = src[3].Y;
            float dstX0 = dst[0].X, dstY0 = dst[0].Y;
            float dstX1 = dst[1].X, dstY1 = dst[1].Y;
            float dstX2 = dst[2].X, dstY2 = dst[2].Y;
            float dstX3 = dst[3].X, dstY3 = dst[3].Y;
            SKMatrix matrix = new SKMatrix();
            float dx1 = dstX1 - dstX2, dx2 = dstX3 - dstX2, dx3 = dstX0 - dstX1 + dstX2 - dstX3;
            float dy1 = dstY1 - dstY2, dy2 = dstY3 - dstY2, dy3 = dstY0 - dstY1 + dstY2 - dstY3;
            float z = dx1 * dy2 - dy1 * dx2;
            float px = (dx3 * dy2 - dy3 * dx2) / z;
            float py = (dx1 * dy3 - dy1 * dx3) / z;
            matrix.ScaleX = dstX1 - dstX0 + px * dstX1;
            matrix.SkewX = dstX3 - dstX0 + py * dstX3;
            matrix.TransX = dstX0;
            matrix.SkewY = dstY1 - dstY0 + px * dstY1;
            matrix.ScaleY = dstY3 - dstY0 + py * dstY3;
            matrix.TransY = dstY0;
            matrix.Persp0 = px;
            matrix.Persp1 = py;
            matrix.Persp2 = 1;
            return matrix;
        }
    }
}
