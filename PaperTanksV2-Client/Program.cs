using Gtk;
using PaperTanksV2Client.GameEngine.Server;
using System;

namespace PaperTanksV2Client
{
    class Program
    {
        static bool DEBUG_SHOW_STACK_TRACE = true;
        [STAThread]
        static int Main(string[] args)
        {
            int exit_code = -1;
            try {
                if (args.Length >= 2 && args[0] == "--server") {
                    if (short.TryParse(args[1], out short port)) {
                        using (Server server = new Server(port)) {
                            exit_code = server.Run();
                        }
                    } else {
                        Console.WriteLine("Invalid port number specified.");
                        exit_code = 1;
                    }
                } else {
                    using (Game game = new Game()) {
                        exit_code = game.Run();
                    }
                }
            } catch (Exception ex) {
                ShowMessageBox(null, new UnhandledExceptionEventArgs(ex, false));
            }
            return exit_code;
        }
        static void ShowMessageBox(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex == null) {
                ex = new Exception("An unknown error occurred.");
            }
            Application.Init();
            using (Window parentWindow = new Window(WindowType.Toplevel)) {
                parentWindow.Hide();
                string error_message =
                    DEBUG_SHOW_STACK_TRACE ? ex.Message + " -> " + ex.StackTrace.ToString() : ex.Message;
                using (MessageDialog md = new MessageDialog(parentWindow,
                           DialogFlags.Modal,
                           MessageType.Error,
                           ButtonsType.Ok,
                           error_message)) {
                    md.Run();
                    md.Destroy();
                }
                parentWindow.Destroy();
            }
            Application.Quit();
        }
    }
}