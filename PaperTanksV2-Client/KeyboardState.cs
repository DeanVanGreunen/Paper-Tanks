using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;

namespace PaperTanksV2_Client
{
    class KeyboardState
    {
        private Dictionary<Keyboard.Key, bool> keyStates;
        private Dictionary<Keyboard.Key, bool> keyJustPressed;
        private Dictionary<Keyboard.Key, bool> keyJustReleased;

        public KeyboardState(RenderWindow window)
        {
            this.keyStates = new Dictionary<Keyboard.Key, bool>();
            this.keyJustPressed = new Dictionary<Keyboard.Key, bool>();
            this.keyJustReleased = new Dictionary<Keyboard.Key, bool>();
            window.KeyPressed += this.OnKeyPressed;
            window.KeyReleased += this.OnKeyReleased;
        }
        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (!this.keyStates.ContainsKey(e.Code) || !this.keyStates[e.Code])
            {
                this.keyStates[e.Code] = true;
                this.keyJustPressed[e.Code] = true;
            }
        }
        private void OnKeyReleased(object sender, KeyEventArgs e)
        {
            if (this.keyStates.ContainsKey(e.Code) && this.keyStates[e.Code])
            {
                this.keyStates[e.Code] = false;
                this.keyJustReleased[e.Code] = true;
            }
        }
        public void Update()
        {
            this.keyJustPressed.Clear();
            this.keyJustReleased.Clear();
        }
        public bool IsKeyPressed(Keyboard.Key key)
        {
            return this.keyStates.ContainsKey(key) && this.keyStates[key];
        }
        public bool IsKeyJustPressed(Keyboard.Key key)
        {
            return this.keyJustPressed.ContainsKey(key) && this.keyJustPressed[key];
        }
        public bool IsKeyJustReleased(Keyboard.Key key)
        {
            return this.keyJustReleased.ContainsKey(key) && this.keyJustReleased[key];
        }
    }
}
