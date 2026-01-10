using Gtk;
using PaperTanksV2Client.GameEngine.Server; 
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
                            string PublicIPAddress = GetPublicIPWithFallback();
                            Console.WriteLine($"Server Running on Port {port} on IP {PublicIPAddress}");
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

        public static string GetPublicIPWithFallback()
        {
            string[] services = new string[]
            {
                "https://api.ipify.org",
                "https://icanhazip.com",
                "https://checkip.amazonaws.com",
                "https://ipinfo.io/ip",
                "https://ifconfig.me/ip"
            };

            using (WebClient client = new WebClient())
            {
                foreach (string service in services)
                {
                    try
                    {
                        string ip = client.DownloadString(service);
                        return ip.Trim();
                    }
                    catch
                    {
                        // Try next service
                        continue;
                    }
                }
            }
    
            return null; // All services failed
        }
    }
}