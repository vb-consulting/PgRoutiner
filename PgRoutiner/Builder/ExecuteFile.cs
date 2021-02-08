using System;
using System.IO;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        public static bool ExecuteFile(NpgsqlConnection connection)
        {
            if (Settings.Value.Execute == null)
            {
                return false;
            }
            var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.Execute));
            if (!File.Exists(file))
            {
                Program.WriteLine(ConsoleColor.Red, $"ERROR: File {Settings.Value.Execute} does not exists.");
                return true;
            }
            try
            {
                DumpRelativePath("Executing file {0} ...", file);
                Execute(connection, File.ReadAllText(file));
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Failed to execute file {file} content.", $"ERROR: {e.Message}");
            }
            return true;
        }
    }
}
