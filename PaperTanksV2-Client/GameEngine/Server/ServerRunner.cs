using Gtk;

namespace PaperTanksV2Client.GameEngine.Server
{
    public interface ServerRunner
    {
        public void Init();
        public int Run();
        public void Update(float deltaTime);
    }
}