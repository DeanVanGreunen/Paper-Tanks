using System;
using System.Collections.Generic;
using System.IO;
using PaperTanksV2Client.AudioManager;
using SFML.Audio;

namespace PaperTanksV2Client

{
    public enum ResourceManagerFormat
    {
        Image,
        AudioShort,
        AudioLong,
        Video,
        Font
    }
    class ResourceManager
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
            switch (type)
            {
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
            if (File.Exists(fullPath))
            {
                object resource = null;
                switch (type)
                {
                    case ResourceManagerFormat.Image:
                        try
                        {
                            resource = SkiaSharp.SKImage.FromEncodedData(fullPath);
                        }
                        catch (Exception)
                        {
                            resource = null;
                        }
                        break;
                    case ResourceManagerFormat.AudioShort:
                        try
                        {
                            resource = new ShortAudio();
                            bool loaded = ((ShortAudio)resource).load(fullPath);
                            if (!loaded)
                            {
                                resource = null;
                                break;
                            }
                        } 
                        catch (Exception)
                        {
                            resource = null;
                        }
                        break;
                    case ResourceManagerFormat.AudioLong:
                        try
                        {
                            resource = new LongAudio();
                            bool loaded = ((LongAudio)resource).load(fullPath);
                            if (!loaded)
                            {
                                resource = null;
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            resource = null;
                        }
                        break;
                    case ResourceManagerFormat.Font:
                        resource = new object(); // TODO: LOAD FONT FILE HERE
                        break;
                    case ResourceManagerFormat.Video:
                        resource = new object(); // TODO: LOAD VIDEO FILE HERE
                        break;
                }
                if (resource != null)
                {
                    resources[fullPath] = resource;
                }
                return resource != null;
            }
            return false;
        }

        public object Get(ResourceManagerFormat type, string filename)
        {
            string fullPath = GetResourcePath(type, filename);
            if (resources.ContainsKey(fullPath))
            {
                return resources[fullPath];
            } else
            {
                bool verify_success = this.Verify(type, filename);
                if (!verify_success) return null;
                bool verify_load = this.Load(type, filename);
                if (!verify_load) return null;
                if (resources.ContainsKey(fullPath))
                {
                    return resources[fullPath];
                }
            }
            return null;
        }
    }
}
