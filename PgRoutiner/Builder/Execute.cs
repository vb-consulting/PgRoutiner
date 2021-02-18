using System;
using System.IO;
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

        public static bool ExecuteFromSetting(NpgsqlConnection connection)
        {
            if (Settings.Value.Execute == null)
            {
                return false;
            }
            var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.Execute));
            if (!File.Exists(file))
            {
                new PsqlRunner(Settings.Value, connection).Run($"-c \"{Settings.Value.Execute}\"");
                return true;
            }
            try
            {
                DumpRelativePath("Executing file {0} ...", file);
                Execute(connection, File.ReadAllText(file));
            }
            catch (Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Failed to execute file {file} content.", $"ERROR: {e.Message}");
            }
            return true;
        }
    }
}
