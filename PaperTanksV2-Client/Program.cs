using Gtk; // Ensure you have GTK# installed and referenced in your project.
using System;

namespace PaperTanksV2Client
{
    class Program
    {
        static bool DEBUG_SHOW_STACK_TRACE = true;
        [STAThread] // Required for using GTK in a Console Application
        static int Main(string[] args)
        {
            int exit_code = -1;
            try {
                using (GameEngine game = new GameEngine()) {
                    exit_code = game.run();
                }
            } catch (Exception ex) {
                ShowMessageBox(null, new UnhandledExceptionEventArgs(ex, false));
            }
            return exit_code;
        }

        // Handles unhandled exceptions
        static void ShowMessageBox(object sender, UnhandledExceptionEventArgs e)
        {
            // Ensure this is an actual exception object
            Exception ex = e.ExceptionObject as Exception;
            if (ex == null) {
                ex = new Exception("An unknown error occurred.");
            }

            // Initialize GTK application (necessary if running in console)
            Application.Init();

            // Create a simple Window to be used as the parent for the MessageDialog
            using (Window parentWindow = new Window(WindowType.Toplevel)) {
                parentWindow.Hide(); // Hide the parent window to avoid showing an extra window
#pragma warning disable CA1305 // Specify IFormatProvider
                string error_message = DEBUG_SHOW_STACK_TRACE ? ex.Message + " -> " + ex.StackTrace.ToString() : ex.Message;
#pragma warning restore CA1305 // Specify IFormatProvider
                // Create and show the message dialog
                using (MessageDialog md = new MessageDialog(parentWindow,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Ok,
                    error_message)) {
                    md.Run();
                    md.Destroy(); // Ensure the dialog is destroyed after use
                }

                // Properly dispose of the parent window
                parentWindow.Destroy();
            }

            // Exit GTK application after message dialog is shown
            Application.Quit();
        }
    }
}