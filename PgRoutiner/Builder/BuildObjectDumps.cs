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

            static string GetOrDefault(string name, string @default)
            {
                if (Settings.Value.DbObjectsDirNames.TryGetValue(name, out var value))
                {
                    return value;
                }
                return @default;
            }
            var baseDir = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.DbObjectsDir)), Settings.Value.Connection);
            CreateDir(baseDir, true);
            var tablesDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Tables", "Tables"))), Settings.Value.Connection);
            var viewsDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Views", "Views"))), Settings.Value.Connection);
            var functionsDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Functions", "Functions"))), Settings.Value.Connection);
            var proceduresDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Procedures", "Procedures"))), Settings.Value.Connection);

            var tableCreated = false;
            var viewCreated = false;
            var functionCreated = false;
            var proceduresCreated = false;

            foreach (var (shortFilename, content, type) in builder.GetDatabaseObjects())
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

                var file = string.Format(Path.GetFullPath(Path.Combine(dir, shortFilename)), Settings.Value.Connection);
                var shortName = string.Format($"{dir.Split(Path.DirectorySeparatorChar).Last()}{Path.DirectorySeparatorChar}{shortFilename}", Settings.Value.Connection);
                var exists = File.Exists(file);

                switch (type)
                {
                    case PgType.Table:
                        if (!tableCreated)
                        {
                            CreateDir(tablesDir);
                        }
                        tableCreated = true;
                        break;
                    case PgType.View:
                        if (!viewCreated)
                        {
                            CreateDir(viewsDir);
                        };
                        viewCreated = true;
                        break;
                    case PgType.Function:
                        if (!functionCreated)
                        {
                            CreateDir(functionsDir);
                        }
                        functionCreated = true;
                        break;
                    case PgType.Procedure:
                        if (!proceduresCreated)
                        {
                            CreateDir(proceduresDir);
                        }
                        proceduresCreated = true;
                        break;
                }

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
                    Program.WriteFile(file, content);
                }
                catch (Exception e)
                {
                    Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {file}", $"ERROR: {e.Message}");
                }

                if (!tableCreated)
                {
                    RemoveDir(tablesDir);
                }
                if (!viewCreated)
                {
                    RemoveDir(viewsDir);
                }
                if (!functionCreated)
                {
                    RemoveDir(functionsDir);
                }
                if (!proceduresCreated)
                {
                    RemoveDir(proceduresDir);
                }
            }
        }

        private static void CreateDir(string dir, bool skipDelete = false)
        {
            if (!Settings.Value.Dump && !Directory.Exists(dir))
            {
                Dump($"Creating dir: {dir}");
                Directory.CreateDirectory(dir);
            }
            else if (!skipDelete && !Settings.Value.Dump && !Settings.Value.DbObjectsSkipDelete)
            {
                if (Directory.GetFiles(dir).Length > 0)
                {
                    Dump($"Deleting all existing files in dir: {dir}...");
                    foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                    {
                        fi.Delete();
                    }
                }
            }
        }

        private static void RemoveDir(string dir)
        {
            if (!Settings.Value.Dump && !Settings.Value.DbObjectsSkipDelete && Directory.Exists(dir))
            {
                Dump($"Removing dir: {dir}...");
                if (Directory.GetFiles(dir).Length > 0)
                {
                    foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                    {
                        fi.Delete();
                    }
                }
                Directory.Delete(dir);
            }
        }
    }
}
