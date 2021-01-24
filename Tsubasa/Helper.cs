using System;

namespace Tsubasa
{
    public static class Helper
    {
        public static void Write(string message, ConsoleColor col = ConsoleColor.Gray)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(DateTime.Now.ToLongTimeString().PadRight(12));

            Console.ForegroundColor = col;
            Console.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}