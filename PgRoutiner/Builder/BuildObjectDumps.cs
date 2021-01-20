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
        private static void BuildObjectDumps(PgDumpBuilder builder)
        {
            if (string.IsNullOrEmpty(Settings.Value.DbObjectsDir))
            {
                return;
            }
            var baseDir = Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.DbObjectsDir));
            if (!Directory.Exists(baseDir))
            {
                Dump($"Creating dir: {baseDir}");
                Directory.CreateDirectory(baseDir);
            }

            HashSet<string> cleared = new();
            foreach(var (shortFilename, content, type) in builder.GetDatabaseObjects())
            {
                var dir = Path.GetFullPath(Path.Combine(baseDir, type switch { PgType.Table => "Tables", PgType.View => "Views", _ => "" }));
                if (!Directory.Exists(dir))
                {
                    Dump($"Creating dir: {dir}");
                    Directory.CreateDirectory(dir);
                }
                else if (Settings.Value.DbObjectsDirClearIfNotEmpty && !cleared.Contains(dir))
                {
                    if (Directory.GetFiles(dir).Length > 0)
                    {
                        Dump($"Deleting all existing files in dir: {dir}...");
                        foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                        {
                            fi.Delete();
                        }
                        cleared.Add(dir);
                    }
                }

                var file = Path.Combine(dir, shortFilename);
                var shortName = $"{dir.Split(Path.DirectorySeparatorChar).Last()}{Path.DirectorySeparatorChar}{shortFilename}";
                var exists = File.Exists(file);

                if (exists && Settings.Value.Overwrite == false)
                {
                    Dump($"File {shortName} exists, overwrite is set to false, skipping ...");
                    continue;
                }
                if (exists && Settings.Value.SkipIfExists != null && (
                    Settings.Value.SkipIfExists.Contains(shortName) || Settings.Value.SkipIfExists.Contains(shortFilename))
                    )
                {
                    Dump($"Skipping {shortName}, already exists...");
                    continue;
                }
                if (exists && Settings.Value.AskOverwrite && Program.Ask($"File {shortName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                {
                    Dump($"Skipping {shortName}...");
                    continue;
                }

                Dump($"Creating dump file {shortName}...");
                try
                {
                    File.WriteAllText(file, content);
                }
                catch (Exception e)
                {
                    Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {file}", $"ERROR: {e.Message}");
                }
            }
        }
    }
}
