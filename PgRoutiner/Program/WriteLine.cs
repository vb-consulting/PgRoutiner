using System;

namespace PgRoutiner
{
    static partial class Program
    {
        public static void WriteLine(ConsoleColor? color, params string[] lines)
        {
            if (Mute)
            {
                return;
            }
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
            if (Mute)
            {
                return;
            }
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

        public static void Write(string line)
        {
            if (Mute)
            {
                return;
            }
            Write(null, line);
        }

        public static void WriteLine(params string[] lines)
        {
            if (Mute)
            {
                return;
            }
            WriteLine(null, lines);
        }
    }
}
