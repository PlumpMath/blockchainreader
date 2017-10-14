using System;

namespace blockchain_parser
{
    public sealed class Logger
    {
        private static object consoleLock = new object();
        public static void LogStatus(ConsoleColor color, string message)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = color;
                var datetime = DateTime.UtcNow;
                Console.WriteLine("[" + datetime + "] " +  message);
                Console.ResetColor();
            }
        }
    }
}