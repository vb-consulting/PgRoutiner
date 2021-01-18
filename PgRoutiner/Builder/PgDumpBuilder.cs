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

        private string GetPgDumpTransactionContent(string args, string name)
        {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            var insideBlock = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
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
            process.OutputDataReceived += (sender, data) =>
            {
                if (!string.IsNullOrEmpty(data.Data))
                {
                    content = string.Concat(content, lineFunc == null ? data.Data : lineFunc(data.Data), Environment.NewLine);
                }
            };
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (sender, data) =>
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
