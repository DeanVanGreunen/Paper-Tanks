using PaperTanksV2Client.GameEngine.data;
using System;

namespace PaperTanksV2Client.GameEngine
{
    public static class Debug
    {
        public static void LogError(string message)
        {
            if (TextData.DEBUG_MODE == true) Console.WriteLine($"ERROR: {message}");
        }

        public static void LogWarning(string message)
        {
            if (TextData.DEBUG_MODE == true) Console.WriteLine($"WARNING: {message}");
        }
    }
}
