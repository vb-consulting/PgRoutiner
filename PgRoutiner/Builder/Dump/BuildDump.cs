using System;
using System.IO;

namespace PgRoutiner
{
    partial class Builder
    {
        private static void BuildDump(string dumpFile, string file, bool overwrite, bool askOverwrite, Func<string> contentFunc)
        {
            if (string.IsNullOrEmpty(dumpFile))
            {
                return;
            }
            var shortFilename = Path.GetFileName(file);
            var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
            var exists = File.Exists(file);
            var relative = file.GetRelativePath();

            if (!Settings.Value.Dump && !Directory.Exists(dir))
            {
                DumpRelativePath("Creating dir: {0} ...", dir);
                Directory.CreateDirectory(dir);
            }

            if (!Settings.Value.Dump && exists && overwrite == false)
            {
                DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
                return;
            }
            if (!Settings.Value.Dump && exists && Settings.Value.SkipIfExists != null &&
                (
                Settings.Value.SkipIfExists.Contains(shortFilename) ||
                Settings.Value.SkipIfExists.Contains(relative))
                )
            {
                DumpFormat("Skipping {0}, already exists ...", relative);
                return;
            }
            if (!Settings.Value.Dump && exists && askOverwrite &&
                Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                DumpFormat("Skipping {0} ...", relative);
                return;
            }

            DumpFormat("Creating dump file {0} ...", relative);
            try
            {
                WriteFile(file, contentFunc());
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {relative}", $"ERROR: {e.Message}");
            }
        }
    }
}
