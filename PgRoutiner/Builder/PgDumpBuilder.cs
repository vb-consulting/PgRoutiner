using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using System.Reflection;

namespace PgRoutiner
{
    public class PgDumpBuilder
    {
        private readonly Settings settings;
        private readonly string baseArg;
        private string pgDumpCmd;
        private const string excludeTables = "--exclude-table=*";

        public NpgsqlConnection Connection { get; }
        public string PgDumpName { get; }

        public PgDumpBuilder(Settings settings, NpgsqlConnection connection, string dumpName = nameof(Settings.PgDump))
        {
            this.settings = settings;
            this.Connection = connection;
            var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
            baseArg = $"--dbname=postgresql://{connection.UserName}:{password}@{connection.Host}:{connection.Port}/{connection.Database} --encoding=UTF8";
            PgDumpName = dumpName;
            pgDumpCmd = typeof(Settings).GetProperty(dumpName).GetValue(settings) as string;
        }

        public void SetPgDumpName(string name)
        {
            typeof(Settings).GetProperty(PgDumpName).SetValue(Settings.Value, name);
            pgDumpCmd = name;
        }

        public IEnumerable<(string name, string content, PgType type)> GetDatabaseObjects()
        {
            var args = string.Concat(
                baseArg,
                " --schema-only",
                settings.DbObjectsOwners ? "" : " --no-owner",
                settings.DbObjectsPrivileges ? "" : " --no-acl",
                settings.DbObjectsDropIfExists ? " --clean --if-exists" : "");

            foreach(var table in Connection.GetTables(settings))
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

                yield return (name, content, table.Type);
            }

            List<string> lines = null;
            try
            {
                lines = GetDumpLines(args, excludeTables);
            }
            catch (Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not create pg_dump for functions and procedures", $"ERROR: {e.Message}");
            }

            if (lines != null)
            {
                foreach (var routine in Connection.GetRoutines(settings))
                {
                    var name = routine.GetFileName();
                    string content;
                    try
                    {
                        content = DumpTransformer.TransformRoutine(routine, lines, dbObjectsNoCreateOrReplace: settings.DbObjectsNoCreateOrReplace);
                    }
                    catch (Exception e)
                    {
                        Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {name}", $"ERROR: {e.Message}");
                        continue;
                    }
                    yield return (name, content, routine.Type);
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
                settings.Schema != null ? $" --schema=\\\"{settings.Schema}\\\"" : "",
                string.IsNullOrEmpty(settings.SchemaDumpOptions) ? "" :
                    (settings.SchemaDumpOptions.StartsWith(" ") ? 
                    settings.SchemaDumpOptions : 
                    string.Concat(" ", settings.SchemaDumpOptions)));

            if (!settings.SchemaDumpNoTransaction)
            {
                return GetPgDumpTransactionContent(args, $"{settings.Connection}_schema");
            }
            return GetPgDumpContent(args);
        }

        public string GetDataContent()
        {
            var args = string.Concat(
                baseArg,
                " --data-only --inserts",
                settings.Schema != null ? $" --schema=\\\"{settings.Schema}\\\"" : "",
                settings.DataDumpTables == null || settings.DataDumpTables.Count == 0 ? "" :
                    $" {string.Join(" ", settings.DataDumpTables.Select(t => $"--table={t}"))}",
                string.IsNullOrEmpty(settings.DataDumpOptions) ? "" :
                    (settings.DataDumpOptions.StartsWith(" ") ?
                    settings.DataDumpOptions :
                    string.Concat(" ", settings.DataDumpOptions)));

            if (!settings.DataDumpNoTransaction)
            {
                return GetPgDumpTransactionContent(args, $"{settings.Connection}_data");
            }
            return GetPgDumpContent(args);
        }

        public string GetDumpVersion()
        {
            return GetPgDumpContent("--version").Replace("pg_dump (PostgreSQL) ", "").Split(' ').First().Trim();
        }

        public List<string> GetRawTableDumpLines(PgItem table, bool withPrivileges)
        {
            var tableArg = table.GetTableArg();
            var args = string.Concat(baseArg, " --schema-only  --no-owner", withPrivileges ? "" : " --no-acl");
            return GetDumpLines(args, tableArg);
        }

        public List<string> GetRawRoutinesDumpLines(bool withPrivileges)
        {
            var args = string.Concat(baseArg, " --schema-only  --no-owner", withPrivileges ? "" : " --no-acl");
            return GetDumpLines(args, excludeTables);
        }

        private string GetTableContent(PgItem table, string args)
        {
            var tableArg = table.GetTableArg();
            if (settings.DbObjectsRaw)
            {
                return GetPgDumpContent($"{args} {tableArg}");
            }
            return new TableDumpTransformer(table, GetDumpLines(args, tableArg)).BuildLines().ToCreateString();
        }

        private string GetViewContent(PgItem table, string args)
        {
            var tableArg = table.GetTableArg();
            if (settings.DbObjectsRaw)
            {
                return GetPgDumpContent($"{args} {tableArg}");
            }
            return DumpTransformer.TransformView(GetDumpLines(args, tableArg), settings.DbObjectsNoCreateOrReplace);
        }

        private List<string> GetDumpLines(string args, string tableArg)
        {
            List<string> lines = new();
            GetPgDumpContent($"{args} {tableArg}", lineFunc: line =>
            {
                lines.Add(line);
                return null;
            });
            return lines;
        }

        private string GetPgDumpTransactionContent(string args, string name)
        {
            var insideBlock = false;
            var insideView = false;
            string lineFunc(string line)
            {
                if (line.Contains("AS $$"))
                {
                    insideBlock = true;
                    return line;
                }
                if (insideBlock && line.Contains("$$;"))
                {
                    insideBlock = false;
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
                return line.Replace("SELECT", "PERFORM");
            };
            return GetPgDumpContent(args,
                start: string.Concat($"DO ${name}$", Environment.NewLine, "BEGIN", Environment.NewLine),
                end: string.Concat($"END ${name}$", Environment.NewLine, "LANGUAGE plpgsql;", Environment.NewLine),
                lineFunc: lineFunc);
        }

        private string GetPgDumpContent(string args, string start = null, string end = null, Func<string, string> lineFunc = null)
        {
            var content = "";
            if (start != null)
            {
                content = start;
            }
            var error = "";
            using var process = new Process();
            process.StartInfo.FileName = pgDumpCmd;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (_, data) =>
            {
                if (!string.IsNullOrEmpty(data.Data))
                {
                    var line = lineFunc == null ? data.Data : lineFunc(data.Data);
                    if (line != null)
                    {
                        content = string.Concat(content, line, Environment.NewLine);
                    }
                }
            };
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (_, data) =>
            {
                if (!string.IsNullOrEmpty(data.Data))
                {
                    error = string.Concat(error, data.Data, Environment.NewLine);
                }
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
            if (end != null)
            {
                content = string.Concat(content, end);
            }
            return content;
        }
    }
}
