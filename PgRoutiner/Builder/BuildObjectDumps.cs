using System;
using System.IO;
using System.Linq;

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
            var baseDir = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.DbObjectsDir)), ConnectionName);
            CreateDir(baseDir, true);
            var tablesDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Tables", "Tables"))), ConnectionName);
            var viewsDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Views", "Views"))), ConnectionName);
            var functionsDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Functions", "Functions"))), ConnectionName);
            var proceduresDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Procedures", "Procedures"))), ConnectionName);
            var domainsDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Domains", "Domains"))), ConnectionName);
            var typesDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Types", "Types"))), ConnectionName);
            var schemasDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Schemas", "Schemas"))), ConnectionName);
            var sequencesDir = string.Format(Path.GetFullPath(Path.Combine(baseDir, GetOrDefault("Sequences", "Sequences"))), ConnectionName);

            var tableCreated = false;
            var viewCreated = false;
            var functionCreated = false;
            var proceduresCreated = false;
            var domainsCreated = false;
            var typesCreated = false;
            var schemaCreated = false;
            var sequencesCreated = false;

            foreach (var (shortFilename, content, type) in builder.GetDatabaseObjects())
            {
                var dir = type switch 
                { 
                    PgType.Table => tablesDir, 
                    PgType.View => viewsDir,
                    PgType.Function => functionsDir,
                    PgType.Procedure => proceduresDir,
                    PgType.Domain => domainsDir,
                    PgType.Type => typesDir,
                    PgType.Schema => schemasDir,
                    PgType.Sequence => sequencesDir,
                    _ => null 
                };

                if (dir == null)
                {
                    continue;
                }

                var file = string.Format(Path.GetFullPath(Path.Combine(dir, shortFilename)), ConnectionName);
                var relative = file.GetRelativePath();

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
                    case PgType.Domain:
                        if (!domainsCreated)
                        {
                            CreateDir(domainsDir);
                        }
                        domainsCreated = true;
                        break;
                    case PgType.Type:
                        if (!typesCreated)
                        {
                            CreateDir(typesDir);
                        }
                        typesCreated = true;
                        break;
                    case PgType.Schema:
                        if (!schemaCreated)
                        {
                            CreateDir(schemasDir);
                        }
                        schemaCreated = true;
                        break;
                    case PgType.Sequence:
                        if (!sequencesCreated)
                        {
                            CreateDir(sequencesDir);
                        }
                        sequencesCreated = true;
                        break;
                }

                var exists = File.Exists(file);

                if (exists && Settings.Value.DbObjectsOverwrite == false)
                {
                    DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
                    continue;
                }
                if (exists && Settings.Value.SkipIfExists != null && 
                    (Settings.Value.SkipIfExists.Contains(shortFilename) || 
                    Settings.Value.SkipIfExists.Contains(relative)))
                {
                    DumpFormat("Skipping {0}, already exists ...", relative);
                    continue;
                }
                if (exists && Settings.Value.DbObjectsAskOverwrite && 
                    Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                {
                    DumpFormat("Skipping {0} ...", relative);
                    continue;
                }

                DumpFormat("Creating dump file {0} ...", relative);
                WriteFile(file, content);
                

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
                if (!domainsCreated)
                {
                    RemoveDir(domainsDir);
                }
                if (!typesCreated)
                {
                    RemoveDir(typesDir);
                }
                if (!schemaCreated)
                {
                    RemoveDir(schemasDir);
                }
                if (!sequencesCreated)
                {
                    RemoveDir(sequencesDir);
                }
            }
        }

        private static void CreateDir(string dir, bool skipDelete = false)
        {
            if (!Settings.Value.Dump && !Directory.Exists(dir))
            {
                DumpRelativePath("Creating dir: {0} ...", dir);
                Directory.CreateDirectory(dir);
            }
            else if (!skipDelete && !Settings.Value.Dump && !Settings.Value.DbObjectsSkipDelete)
            {
                if (Directory.GetFiles(dir).Length > 0)
                {
                    DumpRelativePath("Deleting all existing files in dir: {0} ...", dir);
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
                DumpRelativePath("Removing dir: {0} ...", dir);
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
