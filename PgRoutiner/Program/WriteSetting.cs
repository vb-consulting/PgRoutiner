using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void WriteSetting(string key, string value)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {key}");
            Console.ResetColor();
            Console.WriteLine($" = {value}");
        }
    }
}
