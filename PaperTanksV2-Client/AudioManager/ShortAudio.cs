using System;
using System.Collections.Generic;
using System.Text;
using SFML.Audio;
using System.IO;

namespace PaperTanksV2Client.AudioManager
{
    class ShortAudio : Audio
    {
        private string fullPath = null;
        private SoundBuffer buffer = null;
        private Sound sound = null;
        public bool load(string fullPath)
        {
            if (fullPath.Length == 0 || fullPath == null) return false;
            if (!File.Exists(fullPath)) return false;
            this.fullPath = fullPath;
            this.buffer = new SoundBuffer(fullPath);
            this.sound = new Sound(this.buffer);
            return true;
        }

        public void play()
        {
            if (this.buffer == null) return;
            if (this.sound == null) return;
            this.sound.Play();
        }
        public void Dispose()
        {
            if (this.sound != null)
            {
                this.sound.Dispose();
                this.sound = null;
            }
            if (this.buffer != null)
            {
                this.buffer.Dispose();
                this.buffer = null;
            }
        }
    }
}
