using SFML.Window;
using SkiaSharp;
using System;
namespace PaperTanksV2Client.UI
{
    public class TextInput : MenuItem
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
        private const int INPUT_DELAY_MS = 1000;
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
                // Check if enough time has passed since last input
                var timeSinceLastInput = (DateTime.Now - lastInputTime).TotalMilliseconds;
                
                bool inputProcessed = false;
                Keyboard.Key currentKey = Keyboard.Key.Unknown;
                
                if (game.keyboard.IsKeyPressed(Keyboard.Key.A)) {
                    currentKey = Keyboard.Key.A;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "A" : "a";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.B)) {
                    currentKey = Keyboard.Key.B;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "B" : "b";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.C)) {
                    currentKey = Keyboard.Key.C;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "C" : "c";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.D)) {
                    currentKey = Keyboard.Key.D;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "D" : "d";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.E)) {
                    currentKey = Keyboard.Key.E;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "E" : "e";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.F)) {
                    currentKey = Keyboard.Key.F;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "F" : "f";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.G)) {
                    currentKey = Keyboard.Key.G;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "G" : "g";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.H)) {
                    currentKey = Keyboard.Key.H;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "H" : "h";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.I)) {
                    currentKey = Keyboard.Key.I;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "I" : "i";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.J)) {
                    currentKey = Keyboard.Key.J;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "J" : "j";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.K)) {
                    currentKey = Keyboard.Key.K;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "K" : "k";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.L)) {
                    currentKey = Keyboard.Key.L;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "L" : "l";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.M)) {
                    currentKey = Keyboard.Key.M;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "M" : "m";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.N)) {
                    currentKey = Keyboard.Key.N;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "N" : "n";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.O)) {
                    currentKey = Keyboard.Key.O;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "O" : "o";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.P)) {
                    currentKey = Keyboard.Key.P;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "P" : "p";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Q)) {
                    currentKey = Keyboard.Key.Q;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "Q" : "q";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.R)) {
                    currentKey = Keyboard.Key.R;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "R" : "r";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.S)) {
                    currentKey = Keyboard.Key.S;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "S" : "s";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.T)) {
                    currentKey = Keyboard.Key.T;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "T" : "t";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.U)) {
                    currentKey = Keyboard.Key.U;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "U" : "u";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.V)) {
                    currentKey = Keyboard.Key.V;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "V" : "v";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.W)) {
                    currentKey = Keyboard.Key.W;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "W" : "w";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.X)) {
                    currentKey = Keyboard.Key.X;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "X" : "x";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Y)) {
                    currentKey = Keyboard.Key.Y;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "Y" : "y";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Z)) {
                    currentKey = Keyboard.Key.Z;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += game.keyboard.IsKeyPressed(Keyboard.Key.LShift) ? "Z" : "z";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Hyphen)) {
                    currentKey = Keyboard.Key.Hyphen;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += "-";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Space)) {
                    currentKey = Keyboard.Key.Space;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += " ";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Period)) {
                    currentKey = Keyboard.Key.Period;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        this.text += ".";
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Delete)) {
                    currentKey = Keyboard.Key.Delete;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        if (this.text.Length > 0) this.text = this.text.Substring(0, this.text.Length - 1);
                        inputProcessed = true;
                    }
                } else if (game.keyboard.IsKeyPressed(Keyboard.Key.Backspace)) {
                    currentKey = Keyboard.Key.Backspace;
                    if (timeSinceLastInput >= INPUT_DELAY_MS || lastPressedKey != currentKey) {
                        if (this.text.Length > 0) this.text = this.text.Substring(0, this.text.Length - 1);
                        inputProcessed = true;
                    }
                } else {
                    // No key is pressed, reset the last pressed key
                    lastPressedKey = Keyboard.Key.Unknown;
                }
                
                // Only update the timestamp and call callback if input was actually processed
                if (inputProcessed) {
                    lastInputTime = DateTime.Now;
                    lastPressedKey = currentKey;
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
