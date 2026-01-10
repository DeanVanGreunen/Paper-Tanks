using Newtonsoft.Json;
using PaperTanksV2Client.AudioManager;
using PaperTanksV2Client.GameEngine;
using PaperTanksV2Client.GameEngine.data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PaperTanksV2Client

{
    public enum ResourceManagerFormat
    {
        Image,
        AudioShort,
        AudioLong,
        Video,
        Font,
        Level,
        MultiPlayerLevel,
        Levels,
        Player
    }
    public class ResourceManager
    {
        private Dictionary<string, object> resources;

        public ResourceManager()
        {
            resources = new Dictionary<string, object>();
        }

        public string GetResourcePath(ResourceManagerFormat type, string filename)
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string baseDirectory = "resources";
            string subFolder = "other";
            switch (type) {
                case ResourceManagerFormat.Player:
                    subFolder = "player";
                    break;
                case ResourceManagerFormat.Level:
                    subFolder = "level";
                    break;
                case ResourceManagerFormat.MultiPlayerLevel:
                    subFolder = "multiplayer-level";
                    break;
                case ResourceManagerFormat.Levels:
                    subFolder = "levels";
                    break;
                case ResourceManagerFormat.Image:
                    subFolder = "image";
                    break;
                case ResourceManagerFormat.AudioShort:
                case ResourceManagerFormat.AudioLong:
                    subFolder = "audio";
                    break;
                case ResourceManagerFormat.Font:
                    subFolder = "font";
                    break;
                case ResourceManagerFormat.Video:
                    subFolder = "video";
                    break;
            }
            return Path.Combine(executablePath, baseDirectory, subFolder, filename);
        }

        public List<string> GetList()
        {
            try {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string baseDirectory = "resources";
                string subFolder = "level";
                string levelsFolder = Path.Combine(executablePath, baseDirectory, subFolder);
                return Directory.GetFiles(levelsFolder).ToList();
            } catch (Exception e) {
                return new List<string>();
            }
        }
        
        public List<string> GetMultiPlayerList()
        {
            try {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string baseDirectory = "resources";
                string subFolder = "multiplayer-level";
                string levelsFolder = Path.Combine(executablePath, baseDirectory, subFolder);
                return Directory.GetFiles(levelsFolder).ToList();
            } catch (Exception e) {
                return new List<string>();
            }
        }
        
        
        // Verify if the resource exists by checking the file path
        public bool Verify(ResourceManagerFormat type, string filename)
        {
            string fullPath = GetResourcePath(type, filename);
            return File.Exists(fullPath);  // Checks if the file exists
        }

        // Load the resource, storing it in a dictionary for later retrieval
        public bool Load(ResourceManagerFormat type, string filename)
        {
            string fullPath = GetResourcePath(type, filename);
            if (File.Exists(fullPath)) {
                object resource = null;
                switch (type) {
                    case ResourceManagerFormat.Player:
                        try {
                            string jsonString = File.ReadAllText(fullPath);
                            resource = JsonConvert.DeserializeObject<PlayerData>(jsonString);
                        } catch (Exception e) {
                            resource = null;
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(e);
                        }
                        break;
                    case ResourceManagerFormat.Level:
                    case ResourceManagerFormat.MultiPlayerLevel:
                        try {
                            var settings = new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            };
                            string jsonString = File.ReadAllText(fullPath);
                            resource = JsonConvert.DeserializeObject<Level>(jsonString, settings);
                        } catch (Exception e) {
                            resource = null;
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(e);
                        }
                        break;
                    case ResourceManagerFormat.Levels:
                        try {
                            string jsonString = File.ReadAllText(fullPath);
                            resource = JsonConvert.DeserializeObject<List<string>>(jsonString);
                        } catch (Exception e) {
                            resource = null;
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(e);
                        }
                        break;
                    case ResourceManagerFormat.Image:
                        try {
                            resource = SKImage.FromEncodedData(fullPath);

                        } catch (Exception e) {
                            resource = null;
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(e);
                        }
                        break;
                    case ResourceManagerFormat.AudioShort:
                        try {
                            resource = new ShortAudio();
                            bool loaded = ( (ShortAudio) resource ).load(fullPath);
                            if (!loaded) {
                                resource = null;
                                break;
                            }
                        } catch (Exception e) {
                            resource = null;
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(e);
                        }
                        break;
                    case ResourceManagerFormat.AudioLong:
                        try {
                            resource = new LongAudio();
                            bool loaded = ( (LongAudio) resource ).load(fullPath);
                            if (!loaded) {
                                resource = null;
                                break;
                            }
                        } catch (Exception e) {
                            resource = null;
                            if(TextData.DEBUG_MODE == true) Console.WriteLine(e);
                        }
                        break;
                    case ResourceManagerFormat.Font:
                        resource = SKData.Create(fullPath); // TODO: LOAD FONT FILE HERE
                        break;
                    case ResourceManagerFormat.Video:
                        resource = new object(); // TODO: LOAD VIDEO FILE HERE
                        break;
                }
                if (resource != null) {
                    resources[fullPath] = resource;
                }
                return resource != null;
            }
            return false;
        }

        public object Get(ResourceManagerFormat type, string filename)
        {
            string fullPath = GetResourcePath(type, filename);
            if (resources.ContainsKey(fullPath)) {
                return resources[fullPath];
            } else {
                bool verify_success = this.Verify(type, filename);
                if (!verify_success) return null;
                bool verify_load = this.Load(type, filename);
                if (!verify_load) return null;
                if (resources.ContainsKey(fullPath)) {
                    return resources[fullPath];
                }
            }
            return null;
        }
    }
}
