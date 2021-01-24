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
            CreateDir(baseDir);
            var tablesDir = Path.GetFullPath(Path.Combine(baseDir, "Tables"));
            var viewsDir = Path.GetFullPath(Path.Combine(baseDir, "Views"));
            var functionsDir = Path.GetFullPath(Path.Combine(baseDir, "Functions"));
            var proceduresDir = Path.GetFullPath(Path.Combine(baseDir, "Procedures"));
            CreateDir(tablesDir, true);
            CreateDir(viewsDir, true);
            CreateDir(functionsDir, true);
            CreateDir(proceduresDir, true);

            foreach(var (shortFilename, content, type) in builder.GetDatabaseObjects())
            {
                var dir = type switch 
                { 
                    PgType.Table => tablesDir, 
                    PgType.View => viewsDir,
                    PgType.Function => functionsDir,
                    PgType.Procedure => proceduresDir,
                    _ => null 
                };
                if (dir == null)
                {
                    continue;
                }

                var file = Path.GetFullPath(Path.Combine(dir, shortFilename));
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
                File.WriteAllText(file, content);
            }
        }

        private static void CreateDir(string dir, bool clean = false)
        {
            if (!Directory.Exists(dir))
            {
                Dump($"Creating dir: {dir}");
                Directory.CreateDirectory(dir);
            }
            else if (clean && Directory.GetFiles(dir).Length > 0)
            {
                Dump($"Deleting all existing files in dir: {dir}...");
                foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                {
                    fi.Delete();
                }
            }
        }
    }
}
