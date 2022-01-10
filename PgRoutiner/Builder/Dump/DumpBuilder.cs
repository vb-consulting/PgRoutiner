using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.Dump;

public class DumpBuilder
{
    public static void BuildDump(string dumpFile, string file, bool overwrite, bool askOverwrite, Func<string> contentFunc)
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
            Writer.DumpRelativePath("Creating dir: {0} ...", dir);
            Directory.CreateDirectory(dir);
        }

        if (!Settings.Value.Dump && exists && overwrite == false)
        {
            Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
            return;
        }
        if (!Settings.Value.Dump && exists && Settings.Value.SkipIfExists != null &&
            (
            Settings.Value.SkipIfExists.Contains(shortFilename) ||
            Settings.Value.SkipIfExists.Contains(relative))
            )
        {
            Writer.DumpFormat("Skipping {0}, already exists ...", relative);
            return;
        }
        if (!Settings.Value.Dump && exists && askOverwrite &&
            Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
        {
            Writer.DumpFormat("Skipping {0} ...", relative);
            return;
        }

        Writer.DumpFormat("Creating dump file {0} ...", relative);
        try
        {
            Writer.WriteFile(file, contentFunc());
        }
        catch (Exception e)
        {
            Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {relative}", $"ERROR: {e.Message}");
        }
    }

    public static void BuildObjectDumps(PgDumpBuilder builder, string connectionName)
    {
        if (string.IsNullOrEmpty(Settings.Value.DbObjectsDir))
        {
            return;
        }

        static string GetOrDefault(string name, string @default)
        {
            if (Settings.Value.DbObjectsDirNames.TryGetValue(name, out var value))
            {
                return string.IsNullOrEmpty(value) ? @default : value;
            }
            return @default;
        }

        var baseDir = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.DbObjectsDir)), connectionName);
        CreateDir(baseDir, true);

        string GetDir(string name)
        {
            var dir = GetOrDefault(name, null);
            if (dir == null)
            {
                return dir;
            }
            return Path.GetFullPath(Path.Combine(string.Format(baseDir, connectionName), dir.TrimStart('/').TrimStart('\\')));
        }

        var baseTablesDir = GetDir("Tables");
        var baseViewsDir = GetDir("Views");
        var baseFunctionsDir = GetDir("Functions");
        var baseProceduresDir = GetDir("Procedures");
        var baseDomainsDir = GetDir("Domains");
        var baseTypesDir = GetDir("Types");
        var baseSchemasDir = GetDir("Schemas");
        var baseSequencesDir = GetDir("Sequences");


        static string ParseSchema(string dir, string schema)
        {
            if (dir == null)
            {
                return null;
            }
            return string.Format(dir, schema == "public" ? "" : schema).Replace("//", "/").Replace("\\\\", "\\");
        }


        HashSet<string> dirs = new();

        foreach (var (shortFilename, content, type, schema) in builder.GetDatabaseObjects())
        {
            var dir = type switch
            {
                PgType.Table => ParseSchema(baseTablesDir, schema),
                PgType.View => ParseSchema(baseViewsDir, schema),
                PgType.Function => ParseSchema(baseFunctionsDir, schema),
                PgType.Procedure => ParseSchema(baseProceduresDir, schema),
                PgType.Domain => ParseSchema(baseDomainsDir, schema),
                PgType.Type => ParseSchema(baseTypesDir, schema),
                PgType.Schema => ParseSchema(baseSchemasDir, schema),
                PgType.Sequence => ParseSchema(baseSequencesDir, schema),
                _ => null
            };

            if (dir == null)
            {
                continue;
            }

            var file = string.Format(Path.GetFullPath(Path.Combine(dir, shortFilename)), connectionName);
            var relative = file.GetRelativePath();

            if (!dirs.Contains(dir.TrimEnd('/').TrimEnd('\\')))
            {
                CreateDir(dir);
                dirs.Add(dir.TrimEnd('/').TrimEnd('\\'));
            }

            var exists = File.Exists(file);

            if (exists && Settings.Value.DbObjectsOverwrite == false)
            {
                Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
                continue;
            }
            if (exists && Settings.Value.SkipIfExists != null &&
                (Settings.Value.SkipIfExists.Contains(shortFilename) ||
                Settings.Value.SkipIfExists.Contains(relative)))
            {
                Writer.DumpFormat("Skipping {0}, already exists ...", relative);
                continue;
            }
            if (exists && Settings.Value.DbObjectsAskOverwrite &&
                Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                Writer.DumpFormat("Skipping {0} ...", relative);
                continue;
            }

            Writer.DumpFormat("Creating dump file {0} ...", relative);
            Writer.WriteFile(file, content);
        }

        foreach (var dir in Directory.GetDirectories(baseDir, "*", SearchOption.TopDirectoryOnly))
        {
            if (Directory.GetDirectories(dir).Length > 0)
            {
                continue;
            }
            if (!dirs.Contains(dir.TrimEnd('/').TrimEnd('\\')))
            {
                RemoveDir(dir);
            }
        }
    }

    private static void CreateDir(string dir, bool skipDelete = false)
    {
        if (!Settings.Value.Dump && !Directory.Exists(dir))
        {
            Writer.DumpRelativePath("Creating dir: {0} ...", dir);
            Directory.CreateDirectory(dir);
        }
        else if (!skipDelete && !Settings.Value.Dump && !Settings.Value.DbObjectsSkipDeleteDir)
        {
            if (Directory.GetFiles(dir).Length > 0)
            {
                Writer.DumpRelativePath("Deleting all existing files in dir: {0} ...", dir);
                foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                {
                    fi.Delete();
                }
            }
        }
    }

    private static void RemoveDir(string dir)
    {
        if (!Settings.Value.Dump && !Settings.Value.DbObjectsSkipDeleteDir && Directory.Exists(dir))
        {
            Writer.DumpRelativePath("Removing dir: {0} ...", dir);
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
