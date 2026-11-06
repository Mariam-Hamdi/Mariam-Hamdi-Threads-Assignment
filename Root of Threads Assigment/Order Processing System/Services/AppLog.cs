using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Services
{
    public static class AppLog
    {
        private static readonly Lock _lockObj = new();

        public static void Info(string message)
        {
            Print(message, ConsoleColor.Gray);
        }
        public static void Success(string message)
        {
            Print(message, ConsoleColor.Green);
        }
        public static void Error(string message)
        {
            Print(message, ConsoleColor.Red);
        }
        public static void Print(string message, ConsoleColor color)
        {
            lock (_lockObj)
            {
                Console.ForegroundColor = color;
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                int threadid = Environment.CurrentManagedThreadId;
                Console.WriteLine($"[{time}] [Thread {threadid}] {message}");
                Console.ResetColor();
            }

        }
    }
}
