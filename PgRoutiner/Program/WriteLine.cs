using System;

namespace PgRoutiner
{
    static partial class Program
    {
        public static void WriteLine(ConsoleColor? color, params string[] lines)
        { 
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            foreach(var line in lines)
            {
                Console.WriteLine(line);
            }
            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        public static void Write(ConsoleColor? color, string line)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            Console.Write(line);
            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        public static void WriteLine(params string[] lines)
        {
            WriteLine(null, lines);
        }
    }
}
