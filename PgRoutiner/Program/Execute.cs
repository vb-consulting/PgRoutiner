using System;
using System.IO;
using System.Linq;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    static partial class Program
    {
        public static void WriteFile(string path, string content)
        {
            if (Settings.Value.Dump)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(content);
                Console.ResetColor();
                return;
            }
            File.WriteAllText(path, content);
        }

        public static void Execute(NpgsqlConnection connection, string content)
        {
            if (Settings.Value.Dump)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(content);
                Console.ResetColor();
                return;
            }
            const int max = 1000;
            var len = content.Length;
            var dump = content.Substring(0, max > len ? len : max);
            WriteLine("");
            WriteLine("Executing SQL:");
            WriteLine(ConsoleColor.Cyan, dump);
            if (len > max)
            {
                WriteLine("...", $"[{len - max} more characters]");
            }
            WriteLine("");

            if (!Settings.Value.Dump)
            {
                connection.Execute(content);
            }
        }
    }
}
