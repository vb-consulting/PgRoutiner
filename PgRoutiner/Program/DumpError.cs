using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
        public static void DumpError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine($"ERROR: {msg}");
            Console.ResetColor();
        }
    }
}
