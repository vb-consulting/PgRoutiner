using System;
using System.IO;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        private static void BuilMd(NpgsqlConnection connection)
        {
            if (string.IsNullOrEmpty(Settings.Value.CommentsMdFile))
            {
                return;
            }

            var file = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.CommentsMdFile));
            var shortName = Settings.Value.CommentsMdFile;
            var shortFilename = Path.GetFileName(file);
            var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
            var exists = File.Exists(file);

            if (!Settings.Value.Dump && !Directory.Exists(dir))
            {
                Dump($"Creating dir: {dir}");
                Directory.CreateDirectory(dir);
            }

            if (!Settings.Value.Dump && exists && Settings.Value.Overwrite == false)
            {
                Dump($"File {shortName} exists, overwrite is set to false, skipping ...");
                return;
            }
            if (!Settings.Value.Dump && exists && Settings.Value.SkipIfExists != null && (
                Settings.Value.SkipIfExists.Contains(shortName) || Settings.Value.SkipIfExists.Contains(shortFilename))
                )
            {
                Dump($"Skipping {shortName}, already exists...");
                return;
            }
            if (!Settings.Value.Dump && exists && Settings.Value.AskOverwrite && Program.Ask($"File {shortName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                Dump($"Skipping {shortName}...");
                return;
            }

            Dump($"Creating markdown file {shortName}...");
            try
            {
                var builder = new MarkdownDocument(Settings.Value, connection);
                WriteFile(file, builder.Build());
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not write markdown file {file}", $"ERROR: {e.Message}");
            }
        }
    }
}
