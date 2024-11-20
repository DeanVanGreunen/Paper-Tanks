using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.AudioManager
{
    interface Audio : IDisposable
    {
        public abstract bool load(string fullPath);
        public abstract void play();
    }
}
