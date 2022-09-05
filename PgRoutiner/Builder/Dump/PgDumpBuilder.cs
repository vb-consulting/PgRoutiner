﻿using System.Drawing;
using System.Reflection;
using System.Xml.Linq;
using Norm;
using PgRoutiner.Builder.CodeBuilders.Models;
using PgRoutiner.Builder.DiffBuilder;
using PgRoutiner.DataAccess;
using PgRoutiner.DataAccess.Models;
using PgRoutiner.DumpTransformers;
using PgRoutiner.SettingsManagement;
using static System.Net.Mime.MediaTypeNames;
using static PgRoutiner.Builder.Dump.DumpBuilder;

namespace PgRoutiner.Builder.Dump;

public class PgDumpBuilder
{
    private readonly Current settings;
    private readonly string baseArg;
    private string pgDumpCmd;
    private const string excludeTables = "--exclude-table=*";

    public NpgsqlConnection Connection { get; }
    public string PgDumpName { get; }

    public string Command { get => pgDumpCmd; }

    public PgDumpBuilder(Current settings, NpgsqlConnection connection, string dumpName = nameof(Current.PgDump), bool utf8 = true)
    {
        this.settings = settings;
        Connection = connection;

        //var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
        //baseArg = $"--dbname=postgresql://{connection.UserName}:{password}@{connection.Host}:{connection.Port}/{connection.Database} --encoding=UTF8";

        baseArg = $"-h {connection.Host} -p {connection.Port} -U {connection.UserName} {(utf8 ? "--encoding=UTF8" : "")}";

        PgDumpName = dumpName;
        pgDumpCmd = typeof(Current).GetProperty(dumpName).GetValue(settings) as string;
    }

    public void SetPgDumpName(string name)
    {
        typeof(Current).GetProperty(PgDumpName).SetValue(Current.Value, name);
        pgDumpCmd = name;
    }

    public void Run(string args)
    {
        var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Connection) as string;
        Environment.SetEnvironmentVariable("PGPASSWORD", password);

        using var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = pgDumpCmd;
        process.StartInfo.Arguments = $"{baseArg} {args ?? ""}".Trim();
        if (Current.Value.DumpPgCommands)
        {
            Program.WriteLine(ConsoleColor.White, $"{pgDumpCmd} {process.StartInfo.Arguments}");
        }

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.OutputDataReceived += (_, data) =>
        {
            if (!string.IsNullOrEmpty(data.Data))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(data.Data);
                Console.ResetColor();
            }
        };
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (_, data) =>
        {
            if (!string.IsNullOrEmpty(data.Data))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(data.Data);
                Console.ResetColor();
            }
        };
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
        process.Close();
    }

    public IEnumerable<(string name, string content, PgType type, string schema)> GetDatabaseObjects()
    {
        var args = string.Concat(
            baseArg,
            " --schema-only",
            settings.DbObjectsOwners ? "" : " --no-owner",
            settings.DbObjectsPrivileges ? "" : " --no-acl");

        bool HasKey(DumpType key)
        {
            if (settings.DbObjectsDirNames.TryGetValue(key.ToString(), out var value))
            {
                return !string.IsNullOrEmpty(value);
            }
            return false;
        }

        if (HasKey(DumpType.Tables))
        {
            var tables = Connection.GetTables(settings).ToList();
            foreach (var table in tables)
            {
                var name = table.GetFileName();
                string content = null;
                try
                {
#pragma warning disable CS8509
                    content = table.Type switch
#pragma warning restore CS8509
                    {
                        PgType.Table => GetTableContent(table, args),
                        PgType.View => GetViewContent(table, args)
                    };
                }
                catch (Exception e)
                {
                    Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                    continue;
                }

                yield return (name, content, table.Type, table.Schema);
            }
        }

        if (HasKey(DumpType.Sequences))
        {
            foreach (var seq in Connection.GetSequences(settings))
            {
                var name = seq.GetFileName();
                string content;
                try
                {
                    content = GetSeqContent(seq, args);
                }
                catch (Exception e)
                {
                    Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                    continue;
                }
                yield return (name, content, seq.Type, seq.Schema);
            }
        }

        if (HasKey(DumpType.Functions) || HasKey(DumpType.Procedures) || HasKey(DumpType.Domains) || HasKey(DumpType.Types) || HasKey(DumpType.Schemas))
        {
            List<string> lines = null;
            List<PgItem> types = new();
            try
            {
                lines = GetDumpItemLines(args, null, out types);
            }
            catch (Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not create pg_dump for functions and procedures", $"ERROR: {e.Message}");
            }

            if (lines != null)
            {
                if (HasKey(DumpType.Functions) || HasKey(DumpType.Procedures))
                {
                    foreach (var routine in Connection.GetRoutines(settings))
                    {
                        var name = routine.GetFileName();
                        string content;
                        try
                        {
                            content = new RoutineDumpTransformer(routine, lines)
                                .BuildLines(dbObjectsCreateOrReplace: settings.DbObjectsCreateOrReplace)
                                .ToString();
                        }
                        catch (Exception e)
                        {
                            Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                            continue;
                        }
                        yield return (name, content, routine.Type, routine.Schema);
                    }
                }

                if (HasKey(DumpType.Domains))
                {
                    foreach (var domain in Connection.GetDomains(settings))
                    {
                        var name = domain.GetFileName();
                        string content;
                        try
                        {
                            content = new DomainDumpTransformer(domain, lines)
                                .BuildLines()
                                .ToString();
                        }
                        catch (Exception e)
                        {
                            Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                            continue;
                        }
                        yield return (name, content, domain.Type, domain.Schema);
                    }
                }

                if (HasKey(DumpType.Types))
                {
                    foreach (var type in Connection.FilterTypes(types, settings))
                    {
                        var name = type.GetFileName();
                        string content;
                        try
                        {
                            content = new TypeDumpTransformer(type, lines)
                                .BuildLines()
                                .ToString();
                        }
                        catch (Exception e)
                        {
                            Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                            continue;
                        }
                        yield return (name, content, type.Type, type.Schema);
                    }
                }

                if (HasKey(DumpType.Schemas))
                {
                    foreach (var schema in Connection.GetSchemas(settings))
                    {
                        if (string.Equals(schema, "public"))
                        {
                            continue;
                        }
                        var name = $"{schema}.sql";
                        string content;
                        try
                        {

                            content = new SchemaDumpTransformer(schema, lines)
                                .BuildLines()
                                .ToString();

                        }
                        catch (Exception e)
                        {
                            Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                            continue;
                        }
                        yield return (name, content, PgType.Schema, null);
                    }
                }

                if (HasKey(DumpType.Extensions))
                {
                    foreach (var ext in Connection.GetExtensions())
                    {
                        var name = ext.GetFileName();
                        string content;
                        try
                        {
                            content = new ExtensionDumpTransformer(ext.Name, lines)
                                .BuildLines()
                                .ToString();
                        }
                        catch (Exception e)
                        {
                            Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                            continue;
                        }
                        yield return (name, content, ext.Type, ext.Schema);
                    }
                }
            }
        }
    }

    public void DumpObjects(NpgsqlConnection connection)
    {
        List<PgItem> types;
        GetDumpItemLines(string.Concat(baseArg, " --schema-only"), null, out types);
        Console.ForegroundColor = ConsoleColor.Cyan;
        foreach (var item in connection.GetSchemas(Current.Value))
        {
            Console.WriteLine($"SCHEMA {item}");
        }
        foreach (var item in connection.GetExtensions())
        {
            Console.WriteLine($"{item.TypeName} {item.Name}");
        }
        foreach (var item in Connection.FilterTypes(types, settings))
        {
            Console.WriteLine($"{item.TypeName} {item.Name}");
        }
        foreach (var item in connection.GetDomains(Current.Value))
        {
            Console.WriteLine($"{item.TypeName} {item.Schema}.{item.Name}");
        }
        foreach (var item in connection.GetTables(Current.Value))
        {
            Console.WriteLine($"{item.TypeName.Replace("BASE ", "")} {item.Schema}.{item.Name}");
        }
        foreach (var item in connection.GetSequences(Current.Value))
        {
            Console.WriteLine($"{item.TypeName} {item.Schema}.{item.Name}");
        }
        foreach (var group in connection.GetRoutineGroups(Current.Value))
        {
            var name = group.Key.Name;
            var schema = group.Key.Schema;
            foreach(var item in group)
            {
                
                Console.WriteLine($"{item.RoutineType.ToUpper()} {schema}.{name}({string.Join(", ", item.Parameters.Select(p => p.DataType))})");
            }
            
        }
        Console.ResetColor();
    }

    public void DumpObjectDefintion()
    {
        var args = string.Concat(baseArg, " --schema-only");

        foreach(var def in settings.Definition.Split(';'))
        {
            List<string> lines = null;
            List<PgItem> types = new();
            try
            {
                lines = GetDumpItemLines(args, null, out types);
            }
            catch (Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not create pg_dump for functions and procedures", $"ERROR: {e.Message}");
            }

            foreach (var item in Connection.GetPgItems(def, types))
            {
                string content = item.Type switch
                {
                    PgType.Table => GetTableContent(item, args),
                    PgType.View => GetViewContent(item, args),
                    PgType.Function => new RoutineDumpTransformer(item, lines)
                                    .BuildLines(dbObjectsCreateOrReplace: settings.DbObjectsCreateOrReplace)
                                    .ToString(),
                    PgType.Procedure => new RoutineDumpTransformer(item, lines)
                                    .BuildLines(dbObjectsCreateOrReplace: settings.DbObjectsCreateOrReplace)
                                    .ToString(),
                    PgType.Domain => new DomainDumpTransformer(item, lines)
                                    .BuildLines()
                                    .ToString(),
                    PgType.Type => new TypeDumpTransformer(item, lines)
                                    .BuildLines()
                                    .ToString(),
                    PgType.Schema => new SchemaDumpTransformer(item.Schema, lines)
                                    .BuildLines()
                                    .ToString(),
                    //PgType.Sequence => throw new NotImplementedException(),
                    PgType.Extension => new ExtensionDumpTransformer(item.Name, lines)
                                    .BuildLines()
                                    .ToString(),
                    PgType.Unknown => null,
                    _ => throw null,
                };

                if (!string.IsNullOrEmpty(content))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("--");
                    Console.WriteLine($"-- {item.Type}: {def}");
                    Console.WriteLine("--");
                    Console.WriteLine(content);
                    Console.ResetColor();
                }
            }
        }
    }

    public string GetSchemaContent()
    {
        var args = string.Concat(
            baseArg,
            " --schema-only",
            settings.SchemaDumpOwners ? "" : " --no-owner",
            settings.SchemaDumpPrivileges ? "" : " --no-acl",
            settings.SchemaDumpNoDropIfExists ? "" : " --clean --if-exists",
            settings.SchemaSimilarTo != null ? $" --schema=\\\"{settings.SchemaSimilarTo}\\\"" : "",
            settings.SchemaNotSimilarTo != null ? $" --exclude-schema=\\\"{settings.SchemaNotSimilarTo}\\\"" : "",
            string.IsNullOrEmpty(settings.SchemaDumpOptions) ? "" :
                settings.SchemaDumpOptions.StartsWith(" ") ?
                settings.SchemaDumpOptions :
                string.Concat(" ", settings.SchemaDumpOptions));

        if (!settings.SchemaDumpNoTransaction)
        {
            return GetPgDumpTransactionContent(args, $"{Connection.Database}_schema");
        }
        return GetPgDumpContent(args);
    }

    public string GetDataContent()
    {
        var items = settings.DataDumpTables == null ? new List<string>() : settings.DataDumpTables;
        if (!string.IsNullOrEmpty(settings.DataDumpList))
        {
            foreach(var li in settings.DataDumpList.Split(';'))
            {
                items.Add(li);
            }
        }
        List<string> tables = new();
        Dictionary<string, string> temporary = new();
        bool hasTemp = false;
        try
        {
            foreach (var item in items)
            {
                if (item.Contains(' '))
                {
                    var from = item.GetFrom();
                    var temp = $"{from}_tmp_{Guid.NewGuid().ToString().Substring(0, 8)}";

                    try
                    {
                        Connection.Execute(@$"create table {temp} as {item}");
                        temporary.Add(temp, from);
                        tables.Add(temp);
                        hasTemp = true;
                    }
                    catch (Exception e)
                    {
                        Program.DumpError($"Could not export from expression: {item}. Error is {e.Message}");
                    }
                    
                }
                else
                {
                    tables.Add(item);
                }
            }
            if (tables.Count == 0)
            {
                return null;
            }

            var args = string.Concat(
                baseArg,
                " --data-only --inserts",
                settings.SchemaSimilarTo != null ? $" --schema=\\\"{settings.SchemaSimilarTo}\\\"" : "",
                settings.SchemaNotSimilarTo != null ? $" --exclude-schema=\\\"{settings.SchemaNotSimilarTo}\\\"" : "",
                $" {string.Join(" ", tables.Select(t => $"--table={t}"))}",
                string.IsNullOrEmpty(settings.DataDumpOptions) ? "" :
                    settings.DataDumpOptions.StartsWith(" ") ?
                    settings.DataDumpOptions :
                    string.Concat(" ", settings.DataDumpOptions));

            string lineFunc(string line)
            {
                if (settings.DataDumpRaw)
                {
                    return line;
                }
                if (line.StartsWith("--"))
                {
                    if (!line.Contains("Data for Name"))
                    {
                        return null;
                    }
                }
                if (line.StartsWith("SET "))
                {
                    return null;
                }
                if (line.StartsWith("SELECT "))
                {
                    return null;
                }
                if (line.StartsWith("PERFORM "))
                {
                    return null;
                }
                if (hasTemp)
                {
                    foreach (var (temp, table) in temporary)
                    {
                        line = line.Replace(temp, table);
                    }
                }
                return line;
            }
            if (!settings.DataDumpNoTransaction)
            {
                return GetPgDumpTransactionContent(args, $"{Connection.Database}_data", outerLineFunc: lineFunc);
            }
            return GetPgDumpContent(args, lineFunc: lineFunc);
        }
        finally
        {
            //drop temporary
            if (hasTemp)
            {
                foreach(var table in temporary.Keys)
                {
                    Connection.Execute(@$"drop table if exists {table}");
                }
            }
        }
    }

    public string GetDumpVersion(bool restore = false)
    {
        return GetPgDumpContent("--version").Replace($"{(restore ? "pg_restore" : "pg_dump")} (PostgreSQL) ", "").Split(' ').First().Trim();
    }

    public List<string> GetRawTableDumpLines(PgItem table, bool withPrivileges)
    {
        var args = string.Concat(baseArg, " --schema-only  --no-owner", withPrivileges ? "" : " --no-acl");
        return GetDumpItemLines(args, table);
    }

    public List<string> GetRawRoutinesDumpLines(bool withPrivileges, out List<PgItem> types)
    {
        var args = string.Concat(baseArg, " --schema-only  --no-owner", withPrivileges ? "" : " --no-acl");
        return GetDumpItemLines(args, null, out types);
    }

    private string GetTableContent(PgItem table, string args)
    {
        if (settings.DbObjectsRaw)
        {
            return GetPgDumpContent($"{args} {table.GetTableArg()}");
        }
        return new TableDumpTransformer(table, GetDumpItemLines(args, table), Connection).BuildLines().ToString();
    }

    private string GetSeqContent(PgItem seq, string args)
    {
        if (settings.DbObjectsRaw)
        {
            return GetPgDumpContent($"{args} {seq.GetTableArg()}");
        }
        return new SequenceDumpTransformer(seq, GetDumpItemLines(args, seq)).BuildLines().ToString();
    }

    private string GetViewContent(PgItem table, string args)
    {
        if (settings.DbObjectsRaw)
        {
            return GetPgDumpContent($"{args} {table.GetTableArg()}");
        }
        return new ViewDumpTransformer(GetDumpItemLines(args, table))
            .BuildLines(dbObjectsCreateOrReplace: settings.DbObjectsCreateOrReplace)
            .ToString();
    }

    private List<string> GetDumpItemLines(string args, PgItem item, out List<PgItem> types)
    {
        List<PgItem> found = new();
        var result = GetDumpItemLines(args, item, lineAction: line =>
        {
            var entry = line.FirstWordAfter("CREATE TYPE");
            if (entry != null)
            {
                var type = new PgItem { Type = PgType.Type };
                var parts = entry.Split('.');
                if (parts.Length < 2)
                {
                    type.Schema = "public";
                    type.Name = entry;
                }
                else
                {
                    type.Schema = parts[0];
                    type.Name = parts[1];
                }
                found.Add(type);
            }
        });
        types = found;
        return result;
    }

    private List<string> GetDumpItemLines(string args, PgItem item, Action<string> lineAction = null)
    {
        if (item == null)
        {
            List<string> lines = new();
            GetPgDumpContent($"{args} {excludeTables}", lineFunc: line =>
            {
                lines.Add(line);
                if (lineAction != null)
                {
                    lineAction(line);
                }
                return null;
            });
            return lines;
        }
        else
        {
            List<string> lines = new();
            foreach (var line in EnumeratePgDumpItem(args, item))
            {
                foreach(var split in line.Split(Environment.NewLine))
                {
                    lines.Add(split);
                    if (lineAction != null)
                    {
                        lineAction(split);
                    }
                }
            }
            return lines;
        }

        /*
        string itemArg;
        if (item == null)
        {
            itemArg = excludeTables;
        }
        else
        {
            itemArg = item.GetTableArg();
        }
        List<string> lines = new();
        GetPgDumpContent($"{args} {itemArg}", lineFunc: line =>
        {
            lines.Add(line);
            if (lineAction != null)
            {
                lineAction(line);
            }
            return null;
        });
        return lines;
       */
    }

    private string GetPgDumpTransactionContent(string args, string name, Func<string, string> outerLineFunc = null)
    {
        var insideBlock = false;
        var insideView = false;
        string endSequence = null;

        var partitions = Connection.GetAllPartitionTables().ToList();

        string lineFunc(string line)
        {
            if (line.Contains("CREATE FUNCTION ") || line.Contains("CREATE PROCEDURE "))
            {
                insideBlock = true;
                return line;
            }
            if (insideBlock && endSequence == null)
            {
                endSequence = line.GetSequence();
            }
            if (insideBlock && endSequence != null && line.Contains($"{endSequence};"))
            {
                insideBlock = false;
                endSequence = null;
                return line;
            }
            if (line.StartsWith("CREATE VIEW "))
            {
                insideView = true;
                return line;
            }
            if (insideView && line.Contains(";"))
            {
                insideView = false;
                return line;
            }
            if (insideBlock || insideView)
            {
                return line;
            }
            var selectIndex = line.IndexOf("SELECT ");
            if (selectIndex == 0)
            {
                line = string.Concat("PERFORM ", line.Substring("SELECT ".Length));
            }
            //ONLY public.company_areas9 DROP CONSTRAINT
            if (partitions.Select(p => $"ONLY {p.Schema}.{p.Table} DROP CONSTRAINT").Where(p => line.Contains(p)).Any())
            {
                line = $"-- {line} -- ERROR:  cannot drop inherited constraint ";
            }
            else if (partitions.Select(p => $"ONLY {p.Schema}.\"{p.Table}\" DROP CONSTRAINT").Where(p => line.Contains(p)).Any())
            {
                line = $"-- {line} -- ERROR:  cannot drop inherited constraint ";
            }
            else if (partitions.Select(p => $"ONLY \"{p.Schema}\".\"{p.Table}\" DROP CONSTRAINT").Where(p => line.Contains(p)).Any())
            {
                line = $"-- {line} -- ERROR:  cannot drop inherited constraint ";
            }
            else if (partitions.Select(p => $"ONLY \"{p.Schema}\".{p.Table} DROP CONSTRAINT").Where(p => line.Contains(p)).Any())
            {
                line = $"-- {line} -- ERROR:  cannot drop inherited constraint ";
            }
            if (outerLineFunc != null)
            {
                return outerLineFunc(line);
            }
            return line;
        };
        return GetPgDumpContent(args,
            start: string.Concat($"DO ${name}$", Environment.NewLine, "BEGIN", Environment.NewLine),
            end: string.Concat($"END ${name}$", Environment.NewLine, "LANGUAGE plpgsql;", Environment.NewLine),
            lineFunc: lineFunc);
    }

    private IEnumerable<string> EnumeratePgDumpItem(string args, PgItem item)
    {
        string nameUsed = null;

        foreach (var (line, isView) in EnumeratePgDumpCommands(args))
        {
            if (item.Type != PgType.View && isView)
            {
                continue;
            }
            bool match = false;
            if (nameUsed == null)
            {
                if (line.Contains($"{item.Schema}.{item.Name}"))
                {
                    nameUsed = $"{item.Schema}.{item.Name}";
                    match = true;  
                }
                else if (line.Contains($"{item.Schema}.\"{item.Name}\""))
                {
                    nameUsed = $"{item.Schema}.\"{item.Name}\"";
                    match = true;
                }
                else if (line.Contains($"\"{item.Schema}\".{item.Name}"))
                {
                    nameUsed = $"\"{item.Schema}\".{item.Name}";
                    match = true;
                }
                else if (line.Contains($"\"{item.Schema}\".\"{item.Name}\""))
                {
                    nameUsed = $"\"{item.Schema}\".\"{item.Name}\"";
                    match = true;
                }
            }
            else
            {
                if (line.Contains(nameUsed))
                {
                    match = true;
                }
            }
            if (match)
            {
                if (item.Type == PgType.Table)
                {
                    if (line.Contains($"REFERENCES {nameUsed}"))
                    {
                        continue;
                    }
                }
                var pos = line.IndexOf(nameUsed);
                if (pos + nameUsed.Length < line.Length)
                {
                    var posChar = line[pos + nameUsed.Length];
                    if (posChar != ' ' && posChar != ';' && posChar != '.' && posChar != '\r' && posChar != '\n')
                    {
                        match = false;
                    }
                }
            }
            if (match)
            {
                yield return line;
            }
        }
    }

    private IEnumerable<(string line, bool isView)> EnumeratePgDumpCommands(string args)
    {
        var error = PgDumpCache.GetError(Connection, args, pgDumpCmd);
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }
        string command = "";
        var insideBlock = false;
        string blockStart = null;
        var insideView = false;

        foreach (var line in PgDumpCache.GetLines(Connection, args, pgDumpCmd))
        {
            if (line.StartsWith("--"))
            {
                continue;
            }

            if (insideBlock == false && insideView == false && line.Contains("CREATE VIEW "))
            {
                command = "";
                insideView = true;
            }
            if (insideView)
            {
                command = string.Concat(command, Environment.NewLine, line);
                if (line.Contains(";"))
                {
                    yield return (command, true);
                    insideView = false;
                    command = "";
                }

                continue;
            }


            if (insideBlock == false && line.Contains("CREATE FUNCTION ") || line.Contains("CREATE PROCEDURE "))
            {
                insideBlock = true;
            }
            if (insideBlock)
            {
                if (blockStart == null)
                {
                    var s = "AS $";
                    var pos = line.IndexOf(s);
                    if (pos > -1)
                    {
                        blockStart = line.Substring(pos + s.Length - 1, line.LastIndexOf("$") - (pos + s.Length - 1) + 1);
                    }

                    if (line.Contains("RETURN "))
                    {
                        blockStart = "";
                        if (line.Contains(";"))
                        {
                            insideBlock = false;
                            blockStart = null;
                        }
                    }
                    if (line.Contains("BEGIN "))
                    {
                        blockStart = "END";
                    }
                }
                else
                {
                    if (line.EndsWith($"{blockStart};"))
                    {
                        insideBlock = false;
                        blockStart = null;
                    }
                }
                continue;
            }

            if (line.EndsWith(";"))
            {
                command = string.Concat(command, Environment.NewLine, line);
                yield return (command, false);
                command = "";
            }
            else
            {
                command = string.Concat(command, Environment.NewLine, line);
            }
        }
    }

    private string GetPgDumpContent(string args, string start = null, string end = null, Func<string, string> lineFunc = null)
    {
        var content = "";
        if (start != null)
        {
            content = start;
        }

        var error = PgDumpCache.GetError(Connection, args, pgDumpCmd);
        if (!string.IsNullOrEmpty(error))
        {
            //throw new Exception(error);
            Program.WriteLine(ConsoleColor.Red, error);
        }

        foreach(var line in PgDumpCache.GetLines(Connection, args, pgDumpCmd))
        {
            var newLine = lineFunc == null ? line : lineFunc(line);
            if (newLine != null)
            {
                content = string.Concat(content, newLine, Environment.NewLine);
            }
        }
        if (end != null)
        {
            content = string.Concat(content, end);
        }
        return content;
    }
}
