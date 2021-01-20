using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security;
using Npgsql;
using System.Reflection;
using System.IO;

namespace PgRoutiner
{
    public class PgDumpBuilder
    {
        private readonly Settings settings;
        private readonly NpgsqlConnection connection;
        private readonly string baseArg;

        public PgDumpBuilder(Settings settings, NpgsqlConnection connection)
        {
            this.settings = settings;
            this.connection = connection;
            var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
            baseArg = $"--dbname=postgresql://{connection.UserName}:{password}@{connection.Host}:{connection.Port}/{connection.Database} --encoding=UTF8";
        }

        public IEnumerable<(string name, string content, PgType type)> GetDatabaseObjects()
        {
            var args = string.Concat(
                baseArg,
                " --schema-only",
                settings.DbObjectsNoOwner ? " --no-owner" : "",
                settings.DbObjectsNoPrivileges ? " --no-acl" : "",
                settings.DbObjectsDropIfExists ? " --clean --if-exists" : "");

            foreach(var table in connection.GetTables(settings))
            {
                var name = $"{table.Name}.sql";
                if (table.Schema != "public")
                {
                    name = $"{table.Schema}.{name}";
                }

#pragma warning disable CS8509
                yield return table.Type switch
#pragma warning restore CS8509
                {
                    PgType.Table => (name, GetTableContent(table, args), table.Type),
                    PgType.View => (name, GetViewContent(table, args), table.Type)
                };
            }
        }

        public string GetSchemaContent()
        {
            var args = string.Concat(
                baseArg,
                " --schema-only",
                settings.SchemaDumpNoOwner ? " --no-owner" : "",
                settings.SchemaDumpNoPrivileges ? " --no-acl" : "",
                settings.SchemaDumpDropIfExists ? " --clean --if-exists" : "",
                settings.Schema != null ? $" --schema={settings.Schema}" : "",
                string.IsNullOrEmpty(settings.SchemaDumpAdditionalOptions) ? "" :
                    (settings.SchemaDumpAdditionalOptions.StartsWith(" ") ? 
                    settings.SchemaDumpAdditionalOptions : 
                    string.Concat(" ", settings.SchemaDumpAdditionalOptions)));

            if (settings.SchemaDumpUnderTransaction)
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
                settings.Schema != null ? $" --schema={settings.Schema}" : "",
                settings.DataDumpTables == null || settings.DataDumpTables.Count == 0 ? "" :
                    $" {string.Join(" ", settings.DataDumpTables.Select(t => $"--table={t}"))}",
                string.IsNullOrEmpty(settings.DataDumpAdditionalOptions) ? "" :
                    (settings.DataDumpAdditionalOptions.StartsWith(" ") ?
                    settings.DataDumpAdditionalOptions :
                    string.Concat(" ", settings.DataDumpAdditionalOptions)));

            if (settings.DataDumpUnderTransaction)
            {
                return GetPgDumpTransactionContent(args, $"{settings.Connection}_data");
            }
            return GetPgDumpContent(args);
        }

        private string GetTableContent(PgTable table, string args)
        {
            var name = $"{table.Schema}.{table.Name}";
            if (settings.DbObjectsRaw)
            {
                return GetPgDumpContent($"{args} --table={name}");
            }

            bool before = true;
            bool body = false;
            bool after = false;
            List<string> beforeContent = new();
            List<string> bodyContent = new();
            List<string> afterContent = new();
            
            //!!!
            string sequence = null;
            
            string lineFunc(string line)
            {
                if (before && line.StartsWith($"CREATE TABLE {name}"))
                {
                    before = false;
                    body = true;
                    bodyContent.Add(line);
                }
                else if (body && line.Equals(");"))
                {
                    body = false;
                    after = true;
                    bodyContent.Add(line);
                }
                else
                {
                    if (before)
                    {
                        if (line.StartsWith("DROP "))
                        {
                            beforeContent.Add(line);
                        }
                    }
                    if (body)
                    {
                        bodyContent.Add(line);
                    }
                    if (after)
                    {
                        if (line.StartsWith("-- ") && line.Contains("SEQUENCE"))
                        {
                            sequence = "";
                        }

                        if (!line.StartsWith("ALTER TABLE ONLY") && line.Contains(name))
                        {
                            afterContent.Add(line);
                        }
                        else if (line.StartsWith("    ADD CONSTRAINT"))
                        {
                            bodyContent[bodyContent.Count - 2] = string.Concat(bodyContent[bodyContent.Count - 2], ",");
                            bodyContent.Insert(bodyContent.Count - 1, line.Replace("    ADD", "   ").Replace(";", ""));
                        }
                    }
                }
                return null;
            }
            GetPgDumpContent($"{args} --table={name}", lineFunc: lineFunc);
            StringBuilder sb = new();
            if (beforeContent.Any())
            {
                sb.Append(string.Join(Environment.NewLine, beforeContent));
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(string.Join(Environment.NewLine, bodyContent));
            if (afterContent.Any())
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.Append(string.Join(Environment.NewLine, afterContent));
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        private string GetViewContent(PgTable table, string args)
        {
            var content = GetPgDumpContent($"{args} --table={table.Schema}.{table.Name}");

            return content;
        }

        private string GetPgDumpTransactionContent(string args, string name)
        {
            var insideBlock = false;
            string lineFunc(string line)
            {
                if (line.Contains("AS $$"))
                {
                    insideBlock = true;
                    return line;
                }
                if (line.Contains("$$;"))
                {
                    insideBlock = false;
                    return line;
                }
                if (insideBlock)
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
            process.StartInfo.FileName = settings.PgDumpCommand;
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
