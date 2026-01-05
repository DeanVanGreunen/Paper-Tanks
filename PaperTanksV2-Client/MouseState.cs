using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace PaperTanksV2Client
{
    public class MouseState
    {
        private Dictionary<Mouse.Button, bool> buttonStates;
        private Dictionary<Mouse.Button, bool> buttonStatesPrevious;
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
            this.displayWidth = (int) window.Size.X;
            this.displayHeight = (int) window.Size.Y;

            this.buttonStates = new Dictionary<Mouse.Button, bool>();
            this.buttonStatesPrevious = new Dictionary<Mouse.Button, bool>();

            // Initialize all mouse buttons to false
            foreach (Mouse.Button button in Enum.GetValues(typeof(Mouse.Button)))
            {
                this.buttonStates[button] = false;
                this.buttonStatesPrevious[button] = false;
            }

            window.MouseButtonPressed += this.OnMouseButtonPressed;
            window.MouseButtonReleased += this.OnMouseButtonReleased;
            window.MouseMoved += this.OnMouseMoved;
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            this.buttonStates[e.Button] = true;
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            this.buttonStates[e.Button] = false;
        }

        private void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            this.RawMousePosition = new Vector2i(e.X, e.Y);
            this.ScaledMousePosition = ScaleMousePosition(this.RawMousePosition);
        }

        public void Update()
        {
            // Store current states as previous for next frame
            foreach (Mouse.Button button in Enum.GetValues(typeof(Mouse.Button)))
            {
                this.buttonStatesPrevious[button] = this.buttonStates[button];
            }
        }

        // Check if a mouse button is currently pressed
        public bool IsButtonPressed(Mouse.Button button)
        {
            return this.buttonStates.ContainsKey(button) && this.buttonStates[button];
        }

        // Check if button was just pressed this frame (transition from not pressed to pressed)
        public bool IsButtonJustPressed(Mouse.Button button)
        {
            return this.buttonStates[button] && !this.buttonStatesPrevious[button];
        }

        // Check if button was just released this frame (transition from pressed to not pressed)
        public bool IsButtonJustReleased(Mouse.Button button)
        {
            return !this.buttonStates[button] && this.buttonStatesPrevious[button];
        }

        private Vector2i ScaleMousePosition(Vector2i mousePos)
        {
            int scaledX = (int) ( mousePos.X * (float) this.targetWidth / this.displayWidth );
            int scaledY = (int) ( mousePos.Y * (float) this.targetHeight / this.displayHeight );
            return new Vector2i(scaledX, scaledY);
        }
    }
}