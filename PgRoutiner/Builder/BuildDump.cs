using System;
using System.IO;

namespace PgRoutiner
{
    partial class Builder
    {
        private static void BuildDump(string dumpFile, Func<string> contentFunc, Action<string> successFunc)
        {
            if (string.IsNullOrEmpty(dumpFile))
            {
                return;
            }

            var file = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, dumpFile)), ConnectionName);
            var shortName = string.Format(dumpFile, ConnectionName);
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

            Dump($"Creating dump file {shortName}...");
            try
            {
                WriteFile(file, contentFunc());
                successFunc(file);
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {file}", $"ERROR: {e.Message}");
            }
        }
    }
}
