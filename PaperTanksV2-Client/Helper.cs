using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PaperTanksV2Client
{
    static class Helper
    {

        private static readonly SKPoint[] frontVertices = new SKPoint[4];
        private static readonly SKPoint[] backVertices = new SKPoint[4];
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
            canvas.DrawLine(redLineX, spacing, redLineX, pageHeight, redLinePaint);
        }
        public static SKMatrix CreateYAxisRotationMatrix(float angleInDegrees)
        {
            // Convert angle to radians
            float angleInRadians = angleInDegrees * (float)Math.PI / 180f;

            // Create matrix for Y-axis rotation
            SKMatrix matrix = SKMatrix.MakeIdentity();

            // For Y-axis rotation, we only want to scale the width based on the angle
            // cos(angle) gives us the proper foreshortening effect while maintaining rectangle shape
            float scaleX = (float)Math.Cos(angleInRadians);

            // Only scale in X direction to maintain rectangular shape
            matrix = matrix.PostConcat(SKMatrix.MakeScale(scaleX, 1.0f));

            return matrix;
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
            // Adjust the progress (t) to go from 0 to 1 (start to end of flip)
            float flipAmount = Math.Clamp(t, 0, 1);
            float angleInDegrees = flipAmount * 180.0f; // Flip angle (0 to 180 degrees)
            SKMatrix frontMatrix = CreateYAxisRotationMatrix(angleInDegrees);

            // Apply back page rotation (rotate clockwise)
            //SKMatrix backMatrix = SKMatrix.MakeIdentity();
            //backMatrix = SKMatrix.Concat(
            //    SKMatrix.MakeTranslation(-centerX, -centerY), // Translate to origin
            //    SKMatrix.Concat(
            //        SKMatrix.MakeRotationDegrees(angle), // Rotate
            //        SKMatrix.MakeTranslation(centerX, centerY) // Translate back
            //    )
            //);

            // Apply front page rotation and draw
            canvas.Save();
            //canvas.Translate(outputOffsetX, outputOffsetY);
            canvas.Translate(centerX - pageWidth / 2, 0);
            canvas.SetMatrix(frontMatrix);
            canvas.DrawBitmap(frontImage, new SKRect(centerX - pageWidth / 2, centerY - pageHeight / 2, centerX + pageWidth / 2, centerY + pageHeight / 2));
            canvas.Restore();

            // Apply back page rotation and draw
            //canvas.Save();
            //canvas.Translate(outputOffsetX, outputOffsetY);
            //canvas.SetMatrix(backMatrix);
            //canvas.DrawBitmap(backImage, new SKRect(centerX - pageWidth / 2, centerY - pageHeight / 2, centerX + pageWidth / 2, centerY + pageHeight / 2));
            //canvas.Restore();
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

            // Calculate the determinant
            float dx1 = dstX1 - dstX2, dx2 = dstX3 - dstX2, dx3 = dstX0 - dstX1 + dstX2 - dstX3;
            float dy1 = dstY1 - dstY2, dy2 = dstY3 - dstY2, dy3 = dstY0 - dstY1 + dstY2 - dstY3;
            float z = dx1 * dy2 - dy1 * dx2;

            // Calculate perspective coefficients
            float px = (dx3 * dy2 - dy3 * dx2) / z;
            float py = (dx1 * dy3 - dy1 * dx3) / z;

            // SkiaSharp uses the following matrix format for perspective transformations:
            var matrix = new SKMatrix
            {
                ScaleX = dstX1 - dstX0 + px * dstX1,
                SkewX = dstX3 - dstX0 + py * dstX3,
                TransX = dstX0,
                SkewY = dstY1 - dstY0 + px * dstY1,
                ScaleY = dstY3 - dstY0 + py * dstY3,
                TransY = dstY0,
                Persp0 = px,
                Persp1 = py,
                Persp2 = 1
            };

            return matrix;
        }
    }
}
