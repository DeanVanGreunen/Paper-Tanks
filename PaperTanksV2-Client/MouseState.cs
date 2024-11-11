using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Collections.Generic;

namespace PaperTanksV2Client
{
    class MouseState
    {
        private Dictionary<Mouse.Button, bool> buttonStates;
        private Dictionary<Mouse.Button, bool> buttonJustPressed;
        private Dictionary<Mouse.Button, bool> buttonJustReleased;
        public Vector2i ScaledMousePosition { get; private set; }
        public Vector2i RawMousePosition { get; private set; }

        private int targetWidth;
        private int targetHeight;
        private int displayWidth;
        private int displayHeight;

        public MouseState(RenderWindow window, int targetWidth, int targetHeight)
        {
            this.targetWidth = targetWidth;
            this.targetHeight = targetHeight;
            this.displayWidth = (int)window.Size.X;
            this.displayHeight = (int)window.Size.Y;

            this.buttonStates = new Dictionary<Mouse.Button, bool>();
            this.buttonJustPressed = new Dictionary<Mouse.Button, bool>();
            this.buttonJustReleased = new Dictionary<Mouse.Button, bool>();

            window.MouseButtonPressed += this.OnMouseButtonPressed;
            window.MouseButtonReleased += this.OnMouseButtonReleased;
            window.MouseMoved += this.OnMouseMoved;
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (!this.buttonStates.ContainsKey(e.Button) || !this.buttonStates[e.Button])
            {
                this.buttonStates[e.Button] = true;
                this.buttonJustPressed[e.Button] = true;
            }
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (this.buttonStates.ContainsKey(e.Button) && buttonStates[e.Button])
            {
                this.buttonStates[e.Button] = false;
                this.buttonJustReleased[e.Button] = true;
            }
        }

        // Event handler for mouse movement
        private void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            this.RawMousePosition = new Vector2i(e.X, e.Y);
            this.ScaledMousePosition = ScaleMousePosition(this.RawMousePosition);
        }
        public void Update()
        {
            this.buttonJustPressed.Clear();
            this.buttonJustReleased.Clear();
        }

        // Check if a mouse button is currently pressed
        public bool IsButtonPressed(Mouse.Button button)
        {
            return this.buttonStates.ContainsKey(button) && this.buttonStates[button];
        }
        public bool IsButtonJustPressed(Mouse.Button button)
        {
            return this.buttonJustPressed.ContainsKey(button) && this.buttonJustPressed[button];
        }
        public bool IsButtonJustReleased(Mouse.Button button)
        {
            return this.buttonJustReleased.ContainsKey(button) && this.buttonJustReleased[button];
        }
        private Vector2i ScaleMousePosition(Vector2i mousePos)
        {
            int scaledX = (int)(mousePos.X * (float)this.targetWidth / this.displayWidth);
            int scaledY = (int)(mousePos.Y * (float)this.targetHeight / this.displayHeight);
            return new Vector2i(scaledX, scaledY);
        }
    }
}
