using System;

namespace Tsubasa
{
    public static class Helper
    {
        public static void Write(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(DateTime.Now.ToLongTimeString().PadRight(12));

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
    }
}