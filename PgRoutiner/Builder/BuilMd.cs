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

            if (!Directory.Exists(dir))
            {
                Dump($"Creating dir: {dir}");
                Directory.CreateDirectory(dir);
            }

            if (exists && Settings.Value.Overwrite == false)
            {
                Dump($"File {shortName} exists, overwrite is set to false, skipping ...");
                return;
            }
            if (exists && Settings.Value.SkipIfExists != null && (
                Settings.Value.SkipIfExists.Contains(shortName) || Settings.Value.SkipIfExists.Contains(shortFilename))
                )
            {
                Dump($"Skipping {shortName}, already exists...");
                return;
            }
            if (exists && Settings.Value.AskOverwrite && Program.Ask($"File {shortName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                Dump($"Skipping {shortName}...");
                return;
            }

            Dump($"Creating markdown file {shortName}...");
            try
            {
                var builder = new MarkdownDocument(Settings.Value, connection);
                File.WriteAllText(file, builder.Build());
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not write markdown file {file}", $"ERROR: {e.Message}");
            }
        }
    }
}
