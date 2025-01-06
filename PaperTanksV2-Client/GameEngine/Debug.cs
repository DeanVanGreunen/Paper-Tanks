using System;

namespace PaperTanksV2Client.GameEngine
{
    public static class Debug
    {
        public static void LogError(string message) => Console.WriteLine($"ERROR: {message}");
        public static void LogWarning(string message) => Console.WriteLine($"WARNING: {message}");
    }
}
