using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PaperTanksV2Client
{
    public class TypedSetting<T>
    {
        public T Value { get; set; }
        public TypedSetting(T value)
        {
            Value = value;
        }
    }
    public class ConfigManager
    {
        private Dictionary<string, object> settings;

        public ConfigManager()
        {
            this.settings = new Dictionary<string, object>();
        }

        public void loadDefaults()
        {
            this.set("Music", true);
            this.set("SFX", true);
        }

        public void set<T>(string name, T value)
        {
            var typedSetting = new TypedSetting<T>(value);
            if (this.settings.ContainsKey(name)) {
                this.settings[name] = typedSetting;
            } else {
                this.settings.Add(name, typedSetting);
            }
        }

        public T get<T>(string name, T defaultValue)
        {
            if (this.settings.ContainsKey(name)) {
                var setting = this.settings[name] as TypedSetting<T>;
                if (setting != null) {
                    return setting.Value;
                }
            }

            set(name, defaultValue);
            return defaultValue;
        }

        public void loadFromFile(string path)
        {
            try {
                if (!File.Exists(path)) {
                    loadDefaults();
                    return;
                }
                string loadedJson = File.ReadAllText(path);
                loadFromJSON(loadedJson);
            } catch (JsonException ex) {
                Console.WriteLine($"Error loading settings from file: {ex.Message}");
                loadDefaults();
            }
        }
        public void saveToFile(string path)
        {
            string jsonData = saveToJSON();
            Helper.EnsureDirectoryExists(path);
            File.WriteAllText(path, jsonData);
        }

        public void loadFromJSON(string jsonString)
        {
            try {
                settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    jsonString,
                    new JsonSerializerSettings {
                        TypeNameHandling = TypeNameHandling.All
                    }
                );
            } catch (JsonException ex) {
                Console.WriteLine($"Error loading settings from json: {ex.Message}");
                loadDefaults();
            }
        }

        public string saveToJSON()
        {
            return JsonConvert.SerializeObject(
                settings,
                Formatting.Indented,
                new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.All
                }
            );
        }
    }
}
