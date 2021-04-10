using System;
using System.IO;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        public static bool BuildMdDiff(NpgsqlConnection connection)
        {
            if (!Settings.Value.CommitMd)
            {
                return false;
            }
            if (Settings.Value.MdFile == null)
            {
                Program.WriteLine(ConsoleColor.Red,  $"ERROR: Markdown file setting is not set (CommentsMdFile setting). Can't commit comments.");
                return true;
            }
            var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.MdFile));
            if (!File.Exists(file))
            {
                Program.WriteLine(ConsoleColor.Red, $"ERROR: Markdown file {Settings.Value.MdFile} does not exists. Can't commit comments.");
                return true;
            }

            string content = null;
            try
            {
                var builder = new MarkdownDocument(Settings.Value, connection);
                content = builder.BuildDiff(file);
            }
            catch (Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not parse {Settings.Value.MdFile} file.", $"ERROR: {e.Message}");
            }

            try
            {
                Execute(connection, content);
                Dump("Executed successfully!");
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Failed to execute comments script.", $"ERROR: {e.Message}");
            }

            return true;
        }
    }
}
