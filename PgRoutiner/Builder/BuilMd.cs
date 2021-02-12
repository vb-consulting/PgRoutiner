using System;
using System.IO;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        private static void BuilMd(NpgsqlConnection connection)
        {
            if (string.IsNullOrEmpty(Settings.Value.MdFile))
            {
                return;
            }
            var file = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.MdFile)), ConnectionName);
            var relative = file.GetRelativePath();
            var shortFilename = Path.GetFileName(file);
            var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
            var exists = File.Exists(file);

            if (!Settings.Value.Dump && !Directory.Exists(dir))
            {
                DumpRelativePath("Creating dir: {0} ...", dir);
                Directory.CreateDirectory(dir);
            }

            if (!Settings.Value.Dump && exists && Settings.Value.Overwrite == false)
            {
                DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
                return;
            }
            if (!Settings.Value.Dump && exists && Settings.Value.SkipIfExists != null && (
                Settings.Value.SkipIfExists.Contains(shortFilename) || Settings.Value.SkipIfExists.Contains(relative))
                )
            {
                DumpFormat("Skipping {0}, already exists ...", relative);
                return;
            }
            if (!Settings.Value.Dump && exists && Settings.Value.AskOverwrite && 
                Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                DumpFormat("Skipping {0} ...", relative);
                return;
            }

            DumpFormat("Creating markdown file {0} ...", relative);
            var builder = new MarkdownDocument(Settings.Value, connection);
            WriteFile(file, builder.Build());
        }
    }
}
