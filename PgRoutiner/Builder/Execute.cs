using System;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
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
            Program.WriteLine("");
            Program.WriteLine("Executing SQL:");
            Program.WriteLine(ConsoleColor.Cyan, dump);
            if (len > max)
            {
                Program.WriteLine("...", $"[{len - max} more characters]");
            }
            Program.WriteLine("");

            if (!Settings.Value.Dump)
            {
                connection.Execute(content);
            }
        }
    }
}
