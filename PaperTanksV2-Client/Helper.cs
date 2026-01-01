using SkiaSharp;
using System;
using System.IO;
using System.Text;

namespace PaperTanksV2Client
{
    static class Helper
    {

        private static readonly SKPoint[] frontVertices = new SKPoint[4];
        private static readonly SKPoint[] backVertices = new SKPoint[4];
        public static readonly SKPaint whitePaint = new SKPaint {
            StrokeWidth = 1f,
            Color = SKColors.White,
            IsAntialias = false
        };
        public static readonly SKPaint blueLinePaint = new SKPaint {
            StrokeWidth = 1f,
            Color = SKColor.Parse("#58aff3"),
            IsAntialias = false
        };
        public static readonly SKPaint redLinePaint = new SKPaint {
            StrokeWidth = 1f,
            Color = SKColor.Parse("#ff0000"),
            IsAntialias = false
        };
        public static readonly SKPaint greyLinePaint = new SKPaint {
            StrokeWidth = 1f,
            Color = SKColor.Parse("#474747"),
            IsAntialias = false
        };

        public static void EnsureDirectoryExists(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
        }
        private static ushort[] ConvertToUShortArray(string text)
        {
            // Get UTF-16 encoded bytes
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(text);

            // Convert the byte array into ushort array
            ushort[] textBuffer = new ushort[utf16Bytes.Length / 2];
            for (int i = 0; i < textBuffer.Length; i++) {
                textBuffer[i] = BitConverter.ToUInt16(utf16Bytes, i * 2);
            }

            return textBuffer;
        }
        public static void DrawTextCenetered(SKCanvas canvas, string text, SKRect rect, SKTypeface face, SKFont font, SKPaint paint)
        {
            // Create an SKPaint object for text styling
            using (SKPaint paint2 = new SKPaint {
                Color = paint.Color,      // Text color
                IsAntialias = true,          // Enable anti-aliasing
                TextAlign = SKTextAlign.Center, // Center text horizontally
                TextSize = 50f,              // Start with a default font size
                Typeface = face, // Choose a font family
            }) {

                // Measure the text size
                SKRect textBounds = new SKRect();
                paint2.MeasureText(text, ref textBounds);

                // Dynamically adjust the font size to fit within the rectangle
                float textWidth = textBounds.Width;
                float textHeight = textBounds.Height;

                // Ensure the text fits within the rectangle width
                float scaleX = rect.Width / textWidth;
                float scaleY = rect.Height / textHeight;

                // Use the smallest scale factor to ensure the text fits
                float scaleFactor = Math.Min(scaleX, scaleY);

                // Update the text size based on the scale factor
                paint2.TextSize = paint2.TextSize * scaleFactor;

                // Recalculate the text bounds with the new text size
                paint2.MeasureText(text, ref textBounds);

                // Calculate the position to center the text within the rectangle
                float x = rect.Left + ( rect.Width - textBounds.Width ) / 2;
                float y = rect.Top + ( rect.Height + textBounds.Height ) / 2;

                // Draw the text on the canvas
                canvas.DrawText(text, x, y, paint2);
            }
        }
        public static float GetSingleLineHeight(SKPaint paint)
        {
            var metrics = paint.FontMetrics;
            // This gives precise height for single line
            return -metrics.Ascent + metrics.Descent;
        }
        public static SKImage DrawPageAsImage(bool isLeftPage, int pageWidth, int pageHeight, int totalLines = 90, int spacing = 28)
        {
            SKImageInfo info = new SKImageInfo(pageWidth, pageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (SKSurface surface = SKSurface.Create(info)) {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                DrawPage(canvas, isLeftPage, pageWidth, pageHeight, totalLines, spacing);
                canvas.Flush();
                return surface.Snapshot();
            }
        }

        public static void DrawPage(SKCanvas canvas, bool isLeftPage, int pageWidth, int pageHeight, int totalLines = 90, int spacing = 28)
        {
            canvas.DrawRect(new SKRect(0, 0, pageWidth, pageHeight), whitePaint);
            int lineSpacing = pageHeight / totalLines;
            for (int i = 1; i < totalLines; i++) {
                int y = ( i * lineSpacing ) + spacing / 8;
                canvas.DrawLine(0, y, pageWidth, y, blueLinePaint);
            }
            float redLineX = isLeftPage ? pageWidth - spacing : spacing;
            canvas.DrawLine(redLineX, 0, redLineX, pageHeight, redLinePaint);
        }
        public static SKMatrix CreateYAxisRotationMatrix(float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * (float) Math.PI / 180f;
            SKMatrix matrix = SKMatrix.CreateIdentity();
            float scaleX = (float) Math.Cos(angleInRadians);
            matrix = matrix.PostConcat(SKMatrix.CreateScale(scaleX, 1.0f));
            return matrix;
        }
        public static SKMatrix CreateSineWaveMatrix(float progress)
        {
            progress = Math.Max(0, Math.Min(1, progress));
            float sineValue = (float) Math.Sin(progress * Math.PI);
            float scaleFactor = 1.0f - ( sineValue * 0.75f );
            SKMatrix matrix = SKMatrix.CreateIdentity();
            matrix = matrix.PostConcat(SKMatrix.CreateScale(scaleFactor, 1.0f));
            return matrix;
        }

        public static void RenderPageFlipFromBitmapsAndCallbackToRenderRightSide(
            SKCanvas canvas,
            SKBitmap frontImage,
            SKBitmap backImage,
            SKBitmap secondImage,
            float t,
            Game game,
            Action<Game, SKCanvas> callback
        )
        {
            float flipAmount = Math.Clamp(t, 0, 1);
            float angleInDegrees = flipAmount * 180.0f;
            SKMatrix translationOnlyMatrix = SKMatrix.CreateIdentity().PostConcat(SKMatrix.CreateTranslation(secondImage.Width, 0));
            SKMatrix rotationMatrix = CreateYAxisRotationMatrix(angleInDegrees);
            SKMatrix frontMatrix = rotationMatrix.PostConcat(SKMatrix.CreateTranslation(frontImage.Width, 0));
            SKMatrix backMatrix = rotationMatrix.PostConcat(SKMatrix.CreateTranslation(backImage.Width, 0));
            canvas.Save();
            canvas.SetMatrix(translationOnlyMatrix);
            canvas.DrawBitmap(secondImage, new SKRect(0, 0, secondImage.Width, secondImage.Height));
            canvas.Restore();
            callback?.Invoke(game, canvas);
            canvas.Save();
            canvas.SetMatrix(frontMatrix);
            canvas.DrawBitmap(frontImage, new SKRect(0, 0, frontImage.Width, frontImage.Height));
            canvas.Restore();
            if (flipAmount > 0.5f) {
                canvas.Save();
                canvas.SetMatrix(backMatrix);
                canvas.DrawBitmap(backImage, new SKRect(0, 0, backImage.Width, backImage.Height));
                canvas.Restore();
                canvas.Save();
                canvas.DrawLine(frontImage.Width, 0, frontImage.Width, frontImage.Height, greyLinePaint);
                canvas.Restore();
            }
        }

        public static void RenderPageFlipFromBitmaps(
            SKCanvas canvas,
            SKBitmap frontImage,
            SKBitmap backImage,
            SKBitmap secondImage,
            float t
        )
        {
            float flipAmount = Math.Clamp(t, 0, 1);
            float angleInDegrees = flipAmount * 180.0f;
            SKMatrix translationOnlyMatrix = SKMatrix.CreateIdentity().PostConcat(SKMatrix.CreateTranslation(secondImage.Width, 0));
            SKMatrix rotationMatrix = CreateYAxisRotationMatrix(angleInDegrees);
            SKMatrix frontMatrix = rotationMatrix.PostConcat(SKMatrix.CreateTranslation(frontImage.Width, 0));
            SKMatrix backMatrix = rotationMatrix.PostConcat(SKMatrix.CreateTranslation(backImage.Width, 0));
            canvas.Save();
            canvas.Translate(0, 0);
            canvas.SetMatrix(translationOnlyMatrix);
            canvas.DrawBitmap(secondImage, new SKRect(0, 0, secondImage.Width, secondImage.Height));
            canvas.Restore();

            canvas.Save();
            canvas.Translate(0, 0);
            canvas.SetMatrix(frontMatrix);
            canvas.DrawBitmap(frontImage, new SKRect(0, 0, frontImage.Width, frontImage.Height));
            canvas.Restore();
            if (flipAmount > 0.5f) {
                canvas.Save();
                canvas.Translate(0, 0);
                canvas.SetMatrix(backMatrix);
                canvas.DrawBitmap(backImage, new SKRect(0, 0, backImage.Width, backImage.Height));
                canvas.Restore();
            }
        }

        private static SKMatrix CreatePerspectiveMatrix(SKPoint[] src, SKPoint[] dst)
        {
            if (src.Length != 4 || dst.Length != 4) {
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
            float px = ( dx3 * dy2 - dy3 * dx2 ) / z;
            float py = ( dx1 * dy3 - dy1 * dx3 ) / z;

            // SkiaSharp uses the following matrix format for perspective transformations:
            var matrix = new SKMatrix {
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
