using System;
using System.IO;

namespace PgRoutiner
{
    static partial class Program
    {
        public static string CurrentDir { get; private set; } = Directory.GetCurrentDirectory();

        public static void SetCurrentDir(string[] args)
        {
            foreach (var arg in args)
            {
                if (!arg.Contains("=") && !arg.StartsWith("-") && arg.ToLower() != "run")
                {
                    CurrentDir = Path.Join(CurrentDir, arg);
                    break;
                }
            }
            WriteLine("", "Using dir: ");
            WriteLine(ConsoleColor.Cyan, " " + Path.GetFullPath(CurrentDir));
        }
    }
}
