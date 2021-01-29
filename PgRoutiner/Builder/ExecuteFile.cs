using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        public static bool ExecuteFile(string connectionStr)
        {
            if (Settings.Value.Execute == null)
            {
                return false;
            }
            using var connection = new NpgsqlConnection(connectionStr);
            var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.Execute));
            if (!File.Exists(file))
            {
                Program.WriteLine(ConsoleColor.Red, $"ERROR: File {Settings.Value.Execute} does not exists.");
                return true;
            }
            try
            {
                Program.Execute(connection, File.ReadAllText(file));
                Dump("Executed successfully!");
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Failed to execute file {file} content.", $"ERROR: {e.Message}");
            }
            return true;
        }
    }
}
