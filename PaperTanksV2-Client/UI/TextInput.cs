using SFML.Window;
using SkiaSharp;
using System;
namespace PaperTanksV2Client.UI
{
    class TextInput : MenuItem
    {
        int x;
        int y;
        int w;
        int h;
        private string text = "";
        SKColor fontColor;
        SKFont font;
        SKTypeface face;
        SKPaint paint = null;
        public SKPaint hoverPaint = null;
        public bool isHover = false;
        public SKPaint paintFilled = null;
        public SKPaint paintStroked = null;
        private Action<Game, string> callback = null;
        
        // Input delay tracking
        private DateTime lastInputTime = DateTime.MinValue;
        private const int INPUT_DELAY_MS = 200;
        private Keyboard.Key lastPressedKey = Keyboard.Key.Unknown;
        
        public TextInput(string text, int x, int y, int w, int h, SKColor fontColor, SKTypeface face, SKFont font, float fontSize, SKTextAlign align, Action<Game, string> callback) : base()
        {
            this.text = text;
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.fontColor = fontColor;
            this.font = font;
            this.face = face;
            this.callback = callback;
            this.paint = new SKPaint {
                Color = fontColor,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            };
            this.hoverPaint = new SKPaint {
                Color = fontColor,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            };
            this.paintFilled  = new SKPaint {
                Style = SKPaintStyle.Fill,
                Color = SKColors.White,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            }; 
            this.paintStroked = new SKPaint {
                Style = SKPaintStyle.Stroke,
                Color = fontColor,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = face,
                IsAntialias = true
            };
            SKRect textBounds = new SKRect();
            this.paint.MeasureText(text, ref textBounds);
        }

        public void updateText(string text)
        {
            this.text = text;
        }
        
        public void Dispose()
        {
        }

        public void Input(Game game)
        {
            this.isHover =
                   game.mouse.ScaledMousePosition.X >= this.x &&
                   game.mouse.ScaledMousePosition.X < ( this.x + this.w ) &&
                   game.mouse.ScaledMousePosition.Y >= this.y &&
                   game.mouse.ScaledMousePosition.Y < ( this.y + this.h );
            
            if (this.isHover) {
                var timeSinceLastInput = (DateTime.Now - lastInputTime).TotalMilliseconds;
                if (timeSinceLastInput < INPUT_DELAY_MS) {
                    return;
                }
                bool inputProcessed = false;
                if (game.keyboard.IsKeyJustPressed(Keyboard.Key.A)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "A" : "a";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.B)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "B" : "b";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.C)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "C" : "c";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.D)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "D" : "d";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.E)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "E" : "e";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.F)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "F" : "f";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.G)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "G" : "g";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.H)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "H" : "h";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.I)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "I" : "i";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.J)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "J" : "j";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.K)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "K" : "k";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.L)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "L" : "l";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.M)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "M" : "m";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.N)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "N" : "n";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.O)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "O" : "o";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.P)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "P" : "p";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Q)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "Q" : "q";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.R)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "R" : "r";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.S)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "S" : "s";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.T)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "T" : "t";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.U)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "U" : "u";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.V)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "V" : "v";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.W)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "W" : "w";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.X)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "X" : "x";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Y)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "Y" : "y";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Z)) {
                    this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "Z" : "z";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Hyphen)) {
                    this.text += "-";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Space)) {
                    this.text += " ";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Period)) {
                    this.text += ".";
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Delete)) {
                    if (this.text.Length > 0) this.text = this.text.Substring(0, this.text.Length - 1);
                    inputProcessed = true;
                } else if (game.keyboard.IsKeyJustPressed(Keyboard.Key.Backspace)) {
                    if (this.text.Length > 0) this.text = this.text.Substring(0, this.text.Length - 1);
                    inputProcessed = true;
                }
                
                if (inputProcessed) {
                    lastInputTime = DateTime.Now;
                    this.callback(game, this.text);
                }
            }
        }

        public void Render(Game game, SKCanvas canvas)
        {
            canvas.Save();
            var metrics = paint.FontMetrics;
            float yAdjusted = y + ( -metrics.Ascent );
            canvas.DrawRect(this.x, this.y, this.w, this.h, this.paintFilled);
            canvas.DrawRect(this.x, this.y, this.w, this.h, this.paintStroked);
            canvas.DrawText(text, x, yAdjusted, isHover ? hoverPaint : paint);
            canvas.Restore();
        }
    }
}