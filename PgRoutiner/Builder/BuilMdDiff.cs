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
        public static bool BuilMdDiff(string connectionStr)
        {
            if (!Settings.Value.CommitComments)
            {
                return false;
            }
            if (Settings.Value.CommentsMdFile == null)
            {
                Program.WriteLine(ConsoleColor.Red,  $"ERROR: Markdown file setting is not set (CommentsMdFile setting). Can't commit comments.");
                return true;
            }

            using var connection = new NpgsqlConnection(connectionStr);
            var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.CommentsMdFile));
            if (!File.Exists(file))
            {
                Program.WriteLine(ConsoleColor.Red, $"ERROR: Markdown file {Settings.Value.CommentsMdFile} does not exists. Can't commit comments.");
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
                Program.WriteLine(ConsoleColor.Red, $"Could not parse {Settings.Value.CommentsMdFile} file.", $"ERROR: {e.Message}");
            }

            try
            {
                Program.Execute(connection, content);
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
