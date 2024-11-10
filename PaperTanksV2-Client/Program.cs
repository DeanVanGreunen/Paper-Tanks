using System;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace PaperTanksV2_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an SFML RenderWindow
            var window = new RenderWindow(new VideoMode(500, 500), "SkiaSharp with SFML");
            window.Closed += (sender, e) => window.Close();

            // SkiaSharp bitmap and drawing setup
            var info = new SKImageInfo(500, 500);
            using (var bitmap = new SKBitmap(info))
            {
                // Create an SFML Texture from the SkiaSharp bitmap
                var texture = new Texture((uint)bitmap.Width, (uint)bitmap.Height);
                var sprite = new Sprite(texture);

                while (window.IsOpen)
                {
                    // Handle events
                    window.DispatchEvents();

                    // Draw with SkiaSharp
                    using (var surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes))
                    {
                        var canvas = surface.Canvas;

                        // Clear with white background
                        canvas.Clear(SKColors.White);

                        // Draw a blue rectangle
                        using (var paint = new SKPaint())
                        {
                            paint.Color = SKColors.Blue;
                            canvas.DrawRect(100, 100, 200, 200, paint);
                        }
                    }

                    // Update SFML texture from SkiaSharp bitmap data
                    byte[] pixels = new byte[bitmap.Width * bitmap.Height * 4]; // 4 bytes per pixel (RGBA)
                    Marshal.Copy(bitmap.GetPixels(), pixels, 0, pixels.Length);
                    // Update the SFML texture with the byte array
                    texture.Update(pixels);


                    // Render the SFML texture to the window
                    window.Clear();
                    window.Draw(sprite);
                    window.Display();
                }
            }
        }

    }
}


/* TODO List:
 * - Create Game Loop (init, input, update, render)
 * - Create Resource Manager (Images, Fonts, Audio)
 * - Create PageState Interface
 * - Create Splash Page (single page)
 * - Create Main Menu Page (double page) [New Game, Load Game, MultiPlayer (Client<->Server), Downloadable Content, Settings, About, Exit]
 * - Create Downloadable Content Page (double page) [A List of downloadable content, multiplayer map packs, campaign level extensions]
 * - Create Settings Page (double page) [Input Bindings, SFX Volume, Music Volume, Voice Over Volume, SpeedRun Timer Enabled]
 * - Create About Page (double page) [Game Build Version)
 * - Create Credits Page (double page) [Only Shown when the base campaign is completed]
 * 
 * - Create GamePlayer (Support Campaign and Multiplayer Modes) [Multiplayer Models will eventually extend to have Peer-To-Peer Support Eventually]
 * 
 * Future Planned Feature:
 * - Level Editor
 */
