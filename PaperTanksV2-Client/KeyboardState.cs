using Newtonsoft.Json;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PaperTanksV2Client
{
    public class KeyboardState
    {
        private Dictionary<Keyboard.Key, bool> keyStates;
        private Dictionary<Keyboard.Key, bool> keyJustPressed;
        private Dictionary<Keyboard.Key, bool> keyJustReleased;
        private Dictionary<string, Keyboard.Key> keyBindings;

        public KeyboardState(RenderWindow window)
        {
            this.keyStates = new Dictionary<Keyboard.Key, bool>();
            this.keyJustPressed = new Dictionary<Keyboard.Key, bool>();
            this.keyJustReleased = new Dictionary<Keyboard.Key, bool>();
            this.keyBindings = new Dictionary<string, Keyboard.Key>();
            SetDefaultKeyBindings();
            window.KeyPressed += this.OnKeyPressed;
            window.KeyReleased += this.OnKeyReleased;
        }
        public static string GetKeyText(Keyboard.Key key)
        {
            switch (key) {
                case Keyboard.Key.A: return "A";
                case Keyboard.Key.B: return "B";
                case Keyboard.Key.C: return "C";
                case Keyboard.Key.D: return "D";
                case Keyboard.Key.E: return "E";
                case Keyboard.Key.F: return "F";
                case Keyboard.Key.G: return "G";
                case Keyboard.Key.H: return "H";
                case Keyboard.Key.I: return "I";
                case Keyboard.Key.J: return "J";
                case Keyboard.Key.K: return "K";
                case Keyboard.Key.L: return "L";
                case Keyboard.Key.M: return "M";
                case Keyboard.Key.N: return "N";
                case Keyboard.Key.O: return "O";
                case Keyboard.Key.P: return "P";
                case Keyboard.Key.Q: return "Q";
                case Keyboard.Key.R: return "R";
                case Keyboard.Key.S: return "S";
                case Keyboard.Key.T: return "T";
                case Keyboard.Key.U: return "U";
                case Keyboard.Key.V: return "V";
                case Keyboard.Key.W: return "W";
                case Keyboard.Key.X: return "X";
                case Keyboard.Key.Y: return "Y";
                case Keyboard.Key.Z: return "Z";
                case Keyboard.Key.Num0: return "0";
                case Keyboard.Key.Num1: return "1";
                case Keyboard.Key.Num2: return "2";
                case Keyboard.Key.Num3: return "3";
                case Keyboard.Key.Num4: return "4";
                case Keyboard.Key.Num5: return "5";
                case Keyboard.Key.Num6: return "6";
                case Keyboard.Key.Num7: return "7";
                case Keyboard.Key.Num8: return "8";
                case Keyboard.Key.Num9: return "9";
                case Keyboard.Key.Escape: return "Escape";
                case Keyboard.Key.Space: return "Space";
                case Keyboard.Key.Enter: return "Enter";
                case Keyboard.Key.Backspace: return "Backspace";
                case Keyboard.Key.Tab: return "Tab";
                case Keyboard.Key.LShift: return "Left Shift";
                case Keyboard.Key.RShift: return "Right Shift";
                case Keyboard.Key.LControl: return "Left Control";
                case Keyboard.Key.RControl: return "Right Control";
                case Keyboard.Key.LAlt: return "Left Alt";
                case Keyboard.Key.RAlt: return "Right Alt";
                case Keyboard.Key.Left: return "Left Arrow";
                case Keyboard.Key.Right: return "Right Arrow";
                case Keyboard.Key.Up: return "Up Arrow";
                case Keyboard.Key.Down: return "Down Arrow";
                // Add other keys as needed
                default: return key.ToString();
            }
        }
        private void SetDefaultKeyBindings()
        {
            keyBindings["Action0"] = Keyboard.Key.Escape;
            keyBindings["Action1"] = Keyboard.Key.Space;
            keyBindings["Action2"] = Keyboard.Key.Z;
            keyBindings["Action3"] = Keyboard.Key.X;
            keyBindings["Action4"] = Keyboard.Key.C;
            keyBindings["Action5"] = Keyboard.Key.LShift;
            keyBindings["Action6"] = Keyboard.Key.LControl;
            keyBindings["Action7"] = Keyboard.Key.Enter;
            keyBindings["Action8"] = Keyboard.Key.Tab;
            keyBindings["Action9"] = Keyboard.Key.Backspace;
            keyBindings["Left"] = Keyboard.Key.A;
            keyBindings["Right"] = Keyboard.Key.D;
            keyBindings["Down"] = Keyboard.Key.S;
            keyBindings["Up"] = Keyboard.Key.W;
        }
        public void RemapKey(string action, Keyboard.Key newKey)
        {
            if (keyBindings.ContainsKey(action)) {
                keyBindings[action] = newKey;
            } else {
                keyBindings.Add(action, newKey);
            }
        }

        public Keyboard.Key GetKeyForAction(string action)
        {
            return keyBindings.GetValueOrDefault(action);
        }

        public Dictionary<string, Keyboard.Key> GetAllKeyBindings()
        {
            return new Dictionary<string, Keyboard.Key>(keyBindings);
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (!this.keyStates.ContainsKey(e.Code) || !this.keyStates[e.Code]) {
                this.keyStates[e.Code] = true;
                this.keyJustPressed[e.Code] = true;
            }
        }

        private void OnKeyReleased(object sender, KeyEventArgs e)
        {
            if (this.keyStates.ContainsKey(e.Code) && this.keyStates[e.Code]) {
                this.keyStates[e.Code] = false;
                this.keyJustReleased[e.Code] = true;
            }
        }

        public void Update()
        {
            this.keyJustPressed.Clear();
            this.keyJustReleased.Clear();
        }

        public bool IsActionPressed(string action)
        {
            if (keyBindings.TryGetValue(action, out Keyboard.Key key)) {
                return IsKeyPressed(key);
            }
            return false;
        }

        public bool IsActionJustPressed(string action)
        {
            if (keyBindings.TryGetValue(action, out Keyboard.Key key)) {
                return IsKeyJustPressed(key);
            }
            return false;
        }

        public bool IsActionJustReleased(string action)
        {
            if (keyBindings.TryGetValue(action, out Keyboard.Key key)) {
                return IsKeyJustReleased(key);
            }
            return false;
        }

        // Original key checking methods
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

        public string GetKeyBindingsAsJSON()
        {
            var bindingsToSave = keyBindings.ToDictionary(
                kvp => kvp.Key,
                kvp => (int) kvp.Value
            );
            return JsonConvert.SerializeObject(bindingsToSave, Formatting.Indented);
        }
        public void SaveKeyBindingsToFile(string path) {
            string jsonBindings = GetKeyBindingsAsJSON();
            Helper.EnsureDirectoryExists(path);
            File.WriteAllText(path, jsonBindings);
        }
        public void LoadKeyBindingsFromFile(string path) {
            try {
                if (!File.Exists(path)) throw new Exception("File not found: " + path);
                string loadedJson = File.ReadAllText(path);
                this.LoadKeyBindingsFromJSON(loadedJson);
            } catch (JsonException ex) {
                Console.WriteLine($"Error loading key bindings from file: {ex.Message}");
                SetDefaultKeyBindings();
            }
        }
        public void LoadKeyBindingsFromJSON(string jsonString)
        {
            try {
                var loadedBindings = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonString);
                keyBindings = loadedBindings.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (Keyboard.Key) kvp.Value
                );
            } catch (JsonException ex) {
                Console.WriteLine($"Error loading key bindings from json: {ex.Message}");
                SetDefaultKeyBindings();
            }
        }
    }
}
