using System.Data;
using System.Xml.Linq;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.Dump;

public class DumpBuilder
{
    public static Dictionary<string, string> DbObjectsCustomDirs = new();
    
    public enum DumpType { Tables, Views, Functions, Procedures, Domains, Types, Schemas, Sequences, Extensions }

    public static void BuildDump(string dumpFile, string file, bool overwrite, bool askOverwrite, Func<string> contentFunc)
    {
        if (string.IsNullOrEmpty(dumpFile))
        {
            var content = contentFunc();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(content);
            Console.ResetColor();
            return;
        }
        var shortFilename = Path.GetFileName(file);
        var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
        var exists = File.Exists(file);
        var relative = file.GetRelativePath();

        if (!Current.Value.DumpConsole && !Directory.Exists(dir))
        {
            Writer.DumpRelativePath("Creating dir: {0} ...", dir);
            Directory.CreateDirectory(dir);
        }

        if (exists && overwrite == false)
        {
            Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
            return;
        }
        if (exists && Current.Value.SkipIfExists != null &&
            (
            Current.Value.SkipIfExists.Contains(shortFilename) ||
            Current.Value.SkipIfExists.Contains(relative))
            )
        {
            Writer.DumpFormat("Skipping {0}, already exists ...", relative);
            return;
        }
        if (exists && askOverwrite &&
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
        if (string.IsNullOrEmpty(Current.Value.DbObjectsDir))
        {
            return;
        }

        static string GetOrDefault(DumpType type, string @default)
        {
            if (Current.Value.DbObjectsDirNames.TryGetValue(type.ToString(), out var value))
            {
                return string.IsNullOrEmpty(value) ? @default : value;
            }
            return @default;
        }

        var baseDir = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Current.Value.DbObjectsDir)), connectionName);
        CreateDir(baseDir, true);

        string GetDir(DumpType type)
        {
            var dir = GetOrDefault(type, null);
            if (dir == null)
            {
                return dir;
            }
            return Path.GetFullPath(Path.Combine(string.Format(baseDir, connectionName), dir.TrimStart('/').TrimStart('\\')));
        }

        var baseTablesDir = GetDir(DumpType.Tables);
        var baseViewsDir = GetDir(DumpType.Views);
        var baseFunctionsDir = GetDir(DumpType.Functions);
        var baseProceduresDir = GetDir(DumpType.Procedures);
        var baseDomainsDir = GetDir(DumpType.Domains);
        var baseTypesDir = GetDir(DumpType.Types);
        var baseSchemasDir = GetDir(DumpType.Schemas);
        var baseSequencesDir = GetDir(DumpType.Sequences);
        var baseExtensionsDir = GetDir(DumpType.Extensions);

        int tableCount = 0;
        int viewsCount = 0;
        int functionsCount = 0;
        int proceduresCount = 0;
        int domainsCount = 0;
        int typesCount = 0;
        int schemasCount = 0;
        int sequencesCount = 0;
        int extensionsCount = 0;

        /*
        static string ParseSchema(string dir, string schema)
        {
            if (dir == null)
            {
                return null;
            }
            return string.Format(dir, schema == null || schema == "public" ? "" : schema).Replace("//", "/").Replace("\\\\", "\\");
        }
        */

        HashSet<string> dirs = new();

        foreach (var (shortFilename, objectName, content, type, schema) in builder.GetDatabaseObjects().ToList())
        {
            if (content == null || content.Trim() == "")
            {
                continue;
            }
            var schemaDir = string.Concat(schema == null || schema == "public" ? "" : schema, "/");

            var dir = type switch
            {
                PgType.Table => baseTablesDir, //ParseSchema(baseTablesDir, schema),
                PgType.View => baseViewsDir, //ParseSchema(baseViewsDir, schema),
                PgType.Function => baseFunctionsDir, //ParseSchema(baseFunctionsDir, schema),
                PgType.Procedure => baseProceduresDir, //ParseSchema(baseProceduresDir, schema),
                PgType.Domain => baseDomainsDir, //ParseSchema(baseDomainsDir, schema),
                PgType.Type => baseTypesDir, //ParseSchema(baseTypesDir, schema),
                PgType.Schema => baseSchemasDir, //ParseSchema(baseSchemasDir, schema),
                PgType.Sequence => baseSequencesDir, //ParseSchema(baseSequencesDir, schema),
                PgType.Extension => baseExtensionsDir, //ParseSchema(baseExtensionsDir, schema),
                _ => null
            };

            int? count = type switch
            {
                PgType.Table => ++tableCount,
                PgType.View => ++viewsCount,
                PgType.Function => ++functionsCount,
                PgType.Procedure => ++proceduresCount,
                PgType.Domain => ++domainsCount,
                PgType.Type => ++typesCount,
                PgType.Schema => ++schemasCount,
                PgType.Sequence => ++sequencesCount,
                PgType.Extension => ++extensionsCount,
                _ => null
            };

            if (dir == null)
            {
                continue;
            }

            if (objectName != null)
            {
                string extraDir = null;
                if (Current.Value.CustomDirs != null)
                {
                    foreach (var ns in Current.Value.CustomDirs)
                    {
                        //if (builder.Connection.WithParameters(objectName, ns.Key).Read<bool>("select $1 similar to $2").Single())
                        if (builder.Connection.Read<bool>([(objectName, DbType.AnsiString, null), (ns.Key, DbType.AnsiString, null)], "select $1 similar to $2", r => r.Val<bool>(0)).Single())
                        {
                            extraDir = ns.Value;
                            DbObjectsCustomDirs.Add(objectName, ns.Value);
                            break;
                        }
                    }
                }
                if (extraDir != null)
                {
                    dir = Path.Combine(dir, extraDir);
                }
            }

            var shortName = $"{schemaDir}{shortFilename}";
            var file = Path.GetFullPath(dir.FormatByName(
                ("0", shortName),
                ("file", shortName),
                ("fileName", shortName),
                ("1", schema),
                ("schema", schema),
                ("2", objectName),
                ("name", objectName),
                ("3", connectionName),
                ("connection", connectionName),
                ("4", count),
                ("count", count),
                ("number", count)))
                .Replace("//", "/")
                .Replace("\\\\", "\\")
                .SanitazePath();
            
            dir = Path.GetDirectoryName(file);
            var relative = file.GetRelativePath();

            if (!dirs.Contains(dir.TrimEnd('/').TrimEnd('\\')))
            {
                CreateDir(dir);
                dirs.Add(dir.TrimEnd('/').TrimEnd('\\'));
            }

            var exists = File.Exists(file);

            if (exists && Current.Value.Overwrite == false)
            {
                Writer.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relative);
                continue;
            }
            if (exists && Current.Value.SkipIfExists != null &&
                (Current.Value.SkipIfExists.Contains(shortFilename) ||
                Current.Value.SkipIfExists.Contains(relative)))
            {
                Writer.DumpFormat("Skipping {0}, already exists ...", relative);
                continue;
            }
            if (exists && Current.Value.AskOverwrite &&
                Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                Writer.DumpFormat("Skipping {0} ...", relative);
                continue;
            }

            Writer.DumpFormat("Creating dump file {0} ...", relative);
            Writer.WriteFile(file, content);
        }

        /*
        if (Settings.Value.DbObjectsRemoveExistingDirs)
        {
            foreach (var dir in Directory.GetDirectories(baseDir, "*", SearchOption.TopDirectoryOnly))
            {
                if (!dirs.Contains(dir.TrimEnd('/').TrimEnd('\\')))
                {
                    RemoveDir(dir);
                }
            }
        }
        */
    }

    private static void CreateDir(string dir, bool skipDelete = false)
    {
        /*
        if (!Current.Value.DumpConsole)
        {
            return;
        }
        */
        if (!Directory.Exists(dir))
        {
            Writer.DumpRelativePath("Creating dir: {0} ...", dir);
            Directory.CreateDirectory(dir);
        }
        else if (!skipDelete && !Current.Value.DbObjectsSkipDeleteDir)
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

    /*
    private static void RemoveDir(string dir)
    {
        if (!Current.Value.DumpConsole)
        {
            return;
        }
        if (!Current.Value.DbObjectsSkipDeleteDir && Directory.Exists(dir))
        {
            Writer.DumpRelativePath("Removing dir: {0} ...", dir);
            if (Directory.GetFiles(dir).Length > 0)
            {
                foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                {
                    fi.Delete();
                }
            }
            Directory.Delete(dir, true);
        }
    }
    */
}
