using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
        public static void DumpError(string msg)
        {
            WriteLine(ConsoleColor.Red, "", $"ERROR: {msg}");
        }
    }
}
