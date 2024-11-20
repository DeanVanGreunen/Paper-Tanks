using System;
using System.Collections.Generic;
using System.Text;
using SFML.Audio;
using System.IO;

namespace PaperTanksV2Client.AudioManager
{
    class LongAudio : Audio
    {
        private string fullPath = null;
        private Music music = null;
        public bool load(string fullPath)
        {
            if (fullPath.Length == 0 || fullPath == null) return false;
            if (!File.Exists(fullPath)) return false;
            this.fullPath = fullPath;
            this.music = new Music(fullPath);
            return true;
        }

        public void play()
        {
            if (this.music == null) return;
            this.music.Play();
        }

        public void Dispose()
        {
            if(this.music != null)
            {
                this.music.Dispose();
                this.music = null;
            }
        }
    }
}
