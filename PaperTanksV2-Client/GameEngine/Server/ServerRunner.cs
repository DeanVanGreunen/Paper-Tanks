using Gtk;

namespace PaperTanksV2Client.GameEngine.Server
{
    public interface ServerRunner
    {
        public void Init();
        public void Start();
        public void Update(float deltaTime);
        public void Stop();
    }
}