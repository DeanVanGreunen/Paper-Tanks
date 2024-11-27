using System;
using System.Collections.Generic;
using System.Text;
using PaperTanksV2Client;
using SkiaSharp;

namespace PaperTanksV2_Client.FontManager
{
    // character block size 103 x 169
    // (Pencil) row 0: A-Z0123456789.,()
    // (Pencil) row 1: a-z
    // (Quick Pencil) row 2: A-Z0123456789.,()
    // (Quick Pencil) row 3: a-z
    // (QuickSand) row 4: A-Z0123456789.,()
    // (QuickSand) row 5: a-z
    class FontManager
    {
        public string[] SUPPORTED_FEATURES = new string[] {
            "a-z CHARACTER RENDERING SUPPORT",
            "A-Z CHARACTER RENDERING SUPPORT",
            "0-9 CHARACTER RENDERING SUPPORT",
            ". , ( and ) CHARACTER RENDERING SUPPORT",
            "UNSUPPORTED CHARACTERS WILL RENDER AS A BLANK SPACE",
            "SINGLE CHARACTER RENDERING WITH FONT STYLE AND COLOR",
            "MULTIPLE CHARACTER RENDERING WITH FONT STYLE AND COLOR",
        };
        public const Int32 CHARACTER_BLOCK_SIZE_WIDTH = 103;
        public const Int32 CHARACTER_BLOCK_SIZE_HEIGHT = 169;
        public struct Size
        {
            public Int32 Width;
            public Int32 Height;
            public Size(Int32 Width, Int32 Height)
            {
                this.Width = Width;
                this.Height = Height;
            }
            public static readonly Size Empty = new Size(0, 0);
            public bool IsEmpty => this.Width == 0 && this.Height == 0;
            public override string ToString()
            {
                return $"Width: {Width}, Height: {Height}";
            }
        }
        SkiaSharp.SKImage characters_image = null;
        readonly HashSet<char> NUMBER_CHARACTERS = new HashSet<char> {
            '0','1','2','3','4','5','6','7','8','9',
        };
        readonly HashSet<char> SPECIAL_CHARACTERS = new HashSet<char> {
            '.',',','(',')'
        };
        readonly char[] SPECIAL_CHARACTERS_FOR_INDEX = new char[]{
            '.',',','(',')'
        };
        private Size scaleByPercentage(Size original, Int32 percentage)
        {
            double scaleFactor = percentage / 100;
            Size scaled = original;
            scaled.Width = (Int32)Math.Round(original.Width * scaleFactor);
            scaled.Height = (Int32)Math.Round(original.Height * scaleFactor);
            return scaled;
        }
        private Size scaleByWidth(Size original, Int32 desiredWidth)
        {
            double spectRatio = original.Height / original.Width;
            Size scaled = original;
            scaled.Width = (Int32)desiredWidth;
            scaled.Height = (Int32)Math.Round(desiredWidth * spectRatio);
            return scaled;
        }
        public enum DrawMethod
        {
            CANVAS = 0x00,
            IMAGE = 0x01,
        }
        public enum ScaleStyle
        {
            WIDTH = 0x00,
            PERCENTAGE = 0x01,
        }
        public enum CharacterStyle
        {
            PENCIL = 0x00,
            QUICK_PENCIL = 0x01,
            QUICK_SAND = 0x002
        }
        /// <summary>
        /// Allows this to be independant from actually reloading the resources by asking for the files needed, allows for data to be managed elsewhere
        /// </summary>
        /// <param name="manager">The resource manager, which has been loaded and where we can get the resources from</param>
        /// <returns>Whether we were able to get all the resources we needed, true mean successful, false means failure</returns>
        public bool init(ResourceManager manager)
        {
            bool character_sheet_loaded = manager.Load(ResourceManagerFormat.Image, "characters.png");
            if (character_sheet_loaded) {
                this.characters_image = (SkiaSharp.SKImage)manager.Get(ResourceManagerFormat.Image, "characters.png");
            }
            return character_sheet_loaded;
        }

        /// <summary>
        /// Get's the source SKRect from the character and style values
        /// </summary>
        /// <param name="character">Which Character to find</param>
        /// <param name="style">Which font style to pick from</param>
        /// <returns></returns>
        public SKRect getCharacterSourceRect(char character, CharacterStyle style)
        {
            Int32 row_to_add = 0;
            Int32 col_to_add = 0;
            switch (style)
            {
                case CharacterStyle.PENCIL:
                    row_to_add = 0;
                    break;
                case CharacterStyle.QUICK_PENCIL:
                    row_to_add += CHARACTER_BLOCK_SIZE_HEIGHT * 2;
                    break;
                case CharacterStyle.QUICK_SAND:
                    row_to_add += CHARACTER_BLOCK_SIZE_HEIGHT * 4;
                    break;
            }
            bool isUpper = char.IsUpper(character);
            bool isSpecial = SPECIAL_CHARACTERS.Contains(character);
            bool isNumber = NUMBER_CHARACTERS.Contains(character);
            if (isUpper || isSpecial || isNumber)
            {
                row_to_add += 0;
                if (isUpper)
                {
                    col_to_add += (character - 'A') * CHARACTER_BLOCK_SIZE_WIDTH;
                }
                else if (isNumber)
                {
                    col_to_add += ((character - '0') + 26) * CHARACTER_BLOCK_SIZE_WIDTH;
                }
                else if (isSpecial)
                {
                    Int32 characterIndex = Array.IndexOf(SPECIAL_CHARACTERS_FOR_INDEX, character);
                    col_to_add += ((character - '0') + 26 + characterIndex) * CHARACTER_BLOCK_SIZE_WIDTH;
                }
            } else
            {
                row_to_add += CHARACTER_BLOCK_SIZE_HEIGHT;
            }
            return new SKRect(col_to_add, row_to_add, col_to_add + CHARACTER_BLOCK_SIZE_WIDTH, row_to_add + CHARACTER_BLOCK_SIZE_HEIGHT);
        }

        /// <summary>
        /// Draws a character at x, y points with the selected font style, color and also scale (width or percentage) options
        /// </summary>
        /// <param name="method">Provides away to determin how to draw this character either to an image or to a canvas</param>
        /// <param name="canvas">The Canvas to draw to (optional)</param>
        /// <param name="image">The Image to draw to (optional)</param>
        /// <param name="x">Where to draw to X Destination</param>
        /// <param name="y">Where to draw to Y Destination</param>
        /// <param name="character">What Character to draw from our supported list</param>
        /// <param name="style">Which Font Style to use</param>
        /// <param name="color">What color to make the text for rending</param>
        /// <param name="scale">render at set width or at a set percentage</param>
        /// <param name="width_or_percentage"></param>
        public void drawCharacterAt(DrawMethod method, SkiaSharp.SKCanvas? canvas, SkiaSharp.SKImage? image, Int32 x, Int32 y, char character, CharacterStyle style, SKColor color, ScaleStyle scale, Int32 width_or_percentage) // width in pixels, percentage as 0 to 100
        {
            if (this.characters_image == null) return;
            if (canvas == null && image == null) return;
            SkiaSharp.SKBitmap? bitmap = null;
            SkiaSharp.SKCanvas? canvas_from_image = null;
            if (method == DrawMethod.IMAGE && image != null) {
                bitmap = SkiaSharp.SKBitmap.FromImage(image);
#pragma warning disable CA2000 // Dispose objects before losing scope
                canvas_from_image = new SKCanvas(bitmap);
#pragma warning restore CA2000 // Dispose objects before losing scope
            } else if(method == DrawMethod.CANVAS && canvas != null)
            {
                canvas_from_image = canvas;
            } else
            {
                // TODO: ADD LOGGING
                return;
            }
            
            // Check if character is supported, if not then simply return
            bool isUpper = char.IsUpper(character);
            bool isSpecial = SPECIAL_CHARACTERS.Contains(character);
            bool isNumber = NUMBER_CHARACTERS.Contains(character);
            if (!isUpper && !isSpecial && !isNumber) return;

            SKRect srcRect = getCharacterSourceRect(character, style);
            Size orginalSize = new Size(CHARACTER_BLOCK_SIZE_WIDTH, CHARACTER_BLOCK_SIZE_HEIGHT);
            Size scaledSize;
            SKRect destRect = SKRect.Empty;
            switch (scale)
            {
                case ScaleStyle.WIDTH:
                    scaledSize = scaleByWidth(orginalSize, width_or_percentage);
                    destRect = new SKRect(x, y, x + scaledSize.Width, y + scaledSize.Height);
                    break;
                case ScaleStyle.PERCENTAGE:
                    scaledSize = scaleByPercentage(orginalSize, width_or_percentage);
                    destRect = new SKRect(x, y, x + scaledSize.Width, y + scaledSize.Height);
                    break;
            }
            using (SKPaint paint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Transparent,
                BlendMode = SKBlendMode.SrcOver,
                IsDither = true,
                ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.Modulate)
            })
            {
                if (destRect != SKRect.Empty)
                {
                    canvas.DrawImage(this.characters_image, srcRect, destRect, paint);
                }
            }
            if (method == DrawMethod.IMAGE && image != null && bitmap != null)
            {
                canvas_from_image.Dispose();
                bitmap.Dispose();
            }
            return;
        }

        /// <summary>
        /// Draws a string at x, y points with the selected font style, color and also scale (width or percentage) options
        /// </summary>
        /// <param name="method">Provides away to determin how to draw this character either to an image or to a canvas</param>
        /// <param name="canvas">The Canvas to draw to (optional)</param>
        /// <param name="image">The Image to draw to (optional)</param>
        /// <param name="x">Where to draw to X Destination</param>
        /// <param name="y">Where to draw to Y Destination</param>
        /// <param name="text">What Character to draw from our supported list</param>
        /// <param name="style">Which Font Style to use</param>
        /// <param name="color">What color to make the text for rending</param>
        /// <param name="scale">render at set width or at a set percentage</param>
        public void drawCharactersAt(DrawMethod method, SkiaSharp.SKCanvas? canvas, SkiaSharp.SKImage? image, Int32 x, Int32 y, string text, CharacterStyle style, SKColor color, ScaleStyle scale, Int32 width_or_percentage)
        {
            if (this.characters_image == null) return;
            if (canvas == null && image == null) return;
            SkiaSharp.SKBitmap? bitmap = null;
            SkiaSharp.SKCanvas? canvas_from_image = null;
            if (method == DrawMethod.IMAGE && image != null)
            {
                bitmap = SkiaSharp.SKBitmap.FromImage(image);
#pragma warning disable CA2000 // Dispose objects before losing scope
                canvas_from_image = new SKCanvas(bitmap);
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            else if (method == DrawMethod.CANVAS && canvas != null)
            {
                canvas_from_image = canvas;
            }
            else
            {
                // TODO: ADD LOGGING
                return;
            }
            Size orginalSize = new Size(CHARACTER_BLOCK_SIZE_WIDTH, CHARACTER_BLOCK_SIZE_HEIGHT);
            Size scaledSize;
            SKRect destRect = SKRect.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                // Check if character is supported, if not then simply skip to the next character (space is already calculated per index)
                bool isUpper = char.IsUpper(character);
                bool isSpecial = SPECIAL_CHARACTERS.Contains(character);
                bool isNumber = NUMBER_CHARACTERS.Contains(character);
                if (!isUpper && !isSpecial && !isNumber) continue;
                SKRect srcRect = getCharacterSourceRect(character, style);
                int currentX = 0;
                int currentY = y;
                switch (scale)
                {
                    case ScaleStyle.WIDTH:
                        scaledSize = scaleByWidth(orginalSize, width_or_percentage);
                        currentX = x + i * (scaledSize.Width);
                        destRect = new SKRect(currentX, currentY, currentX + scaledSize.Width, currentY + scaledSize.Height);
                        break;

                    case ScaleStyle.PERCENTAGE:
                        scaledSize = scaleByPercentage(orginalSize, width_or_percentage);
                        currentX = x + i * (scaledSize.Width);
                        destRect = new SKRect(currentX, currentY, currentX + scaledSize.Width, currentY + scaledSize.Height);
                        break;
                }
                using (SKPaint paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = SKColors.Transparent,
                    BlendMode = SKBlendMode.SrcOver,
                    IsDither = true,
                    ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.Modulate)
                })
                {
                    if (destRect != SKRect.Empty)
                    {
                        canvas.DrawImage(this.characters_image, srcRect, destRect, paint);
                    }
                }
            }
            if (method == DrawMethod.IMAGE && image != null && bitmap != null)
            {
                canvas_from_image.Dispose();
                bitmap.Dispose();
            }
        }

        /// <summary>
        /// Draws a string which will fit perfectly between x,y and x+width, y+height, note that this does not yet support word wrapping or rending of new-line or tab characters
        /// </summary>
        /// <param name="method">Provides away to determin how to draw this character either to an image or to a canvas</param>
        /// <param name="canvas">The Canvas to draw to (optional)</param>
        /// <param name="image">The Image to draw to (optional)</param>
        /// <param name="x">Where to draw to X Destination</param>
        /// <param name="y">Where to draw to Y Destination</param>
        /// <param name="text">What Character to draw from our supported list</param>
        /// <param name="style">Which Font Style to use</param>
        /// <param name="color">What color to make the text for rending</param>
        /// <param name="scale">render at set width or at a set percentage</param>
        public void drawCharactersAtWithinBounds(DrawMethod method, SkiaSharp.SKCanvas? canvas, SkiaSharp.SKImage? image, Int32 x, Int32 y, string text, CharacterStyle style, SKColor color, ScaleStyle scale, Int32 width, Int32 height)
        {

            if (this.characters_image == null) return;
            if (canvas == null && image == null) return;
            SkiaSharp.SKBitmap? bitmap = null;
            SkiaSharp.SKCanvas? canvas_from_image = null;
            if (method == DrawMethod.IMAGE && image != null)
            {
                bitmap = SkiaSharp.SKBitmap.FromImage(image);
#pragma warning disable CA2000 // Dispose objects before losing scope
                canvas_from_image = new SKCanvas(bitmap);
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            else if (method == DrawMethod.CANVAS && canvas != null)
            {
                canvas_from_image = canvas;
            }
            else
            {
                // TODO: ADD LOGGING
                return;
            }
            Size orginalSize = new Size(CHARACTER_BLOCK_SIZE_WIDTH, CHARACTER_BLOCK_SIZE_HEIGHT);
            Size scaledSize;
            SKRect destRect = SKRect.Empty;
            int totalStringWidth = (Int32)(CHARACTER_BLOCK_SIZE_WIDTH * text.Length);
            float scaleFactor = Math.Min((float)width / totalStringWidth, (float)height / orginalSize.Height);
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                // Check if character is supported, if not then simply skip to the next character (space is already calculated per index)
                bool isUpper = char.IsUpper(character);
                bool isSpecial = SPECIAL_CHARACTERS.Contains(character);
                bool isNumber = NUMBER_CHARACTERS.Contains(character);
                if (!isUpper && !isSpecial && !isNumber) continue;
                SKRect srcRect = getCharacterSourceRect(character, style);
                scaledSize = new Size((int)(orginalSize.Width * scaleFactor), (int)(orginalSize.Height * scaleFactor));
                int currentX = x + i * scaledSize.Width;
                int currentY = y;
                destRect = new SKRect(currentX, currentY, currentX + scaledSize.Width, currentY + scaledSize.Height);
                using (SKPaint paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = SKColors.Transparent,
                    BlendMode = SKBlendMode.SrcOver,
                    IsDither = true,
                    ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.Modulate)
                })
                {
                    if (destRect != SKRect.Empty)
                    {
                        canvas.DrawImage(this.characters_image, srcRect, destRect, paint);
                    }
                }
            }
            if (method == DrawMethod.IMAGE && image != null && bitmap != null)
            {
                canvas_from_image.Dispose();
                bitmap.Dispose();
            }
        }
    }
}
