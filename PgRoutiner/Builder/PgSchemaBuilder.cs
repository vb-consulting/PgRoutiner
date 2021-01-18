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
    public class PgSchemaBuilder
    {
        private readonly Settings settings;
        private readonly string args;
        
        public PgSchemaBuilder(Settings settings, NpgsqlConnection connection)
        {
            this.settings = settings;
            var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
            args = string.Concat(
                "--dbname=postgresql://",
                connection.UserName,
                ":",
                password,
                "@",
                connection.Host,
                ":",
                connection.Port,
                "/",
                connection.Database,
                " --schema-only --encoding=UTF8",
                settings.PgDumpNoOwner ? " --no-owner" : "",
                settings.PgDumpNoPrivileges ? " --no-acl" : "",
                settings.PgDumpDropIfExists ? " --clean --if-exists" : "",
                settings.Schema != null ? $" --schema={settings.Schema}" : "",
                string.IsNullOrEmpty(settings.PgDumpAdditionalOptions) ? "" : 
                    (settings.PgDumpAdditionalOptions.StartsWith(" ") ? settings.PgDumpAdditionalOptions : string.Concat(" ", settings.PgDumpAdditionalOptions)));
        }

        public string GetPgDumpContent(string table = null)
        {
            var content = "";
            var name = Path.GetFileName(settings.SchemaDumpFile).Replace(".", "");
            if (settings.TransactionalSchema && settings.SchemaDumpFile != null)
            {
                content = string.Concat($"DO ${name}$", Environment.NewLine, "BEGIN", Environment.NewLine);
            }
            var error = "";
            using var process = new Process();
            process.StartInfo.FileName = settings.PgDump;
            process.StartInfo.Arguments = table == null ? args : $"{args} --table={table}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            var insideBlock = false;
            process.OutputDataReceived += (sender, data) =>
            {
                if (!string.IsNullOrEmpty(data.Data))
                {
                    var line = data.Data;
                    if (line.Contains("AS $$"))
                    {
                        insideBlock = true;
                    }
                    else if (line.Contains("$$;"))
                    {
                        insideBlock = false;
                    }
                    else if (settings.TransactionalSchema && settings.SchemaDumpFile != null && !insideBlock)
                    {
                        line = data.Data.Replace("SELECT", "PERFORM");
                    }
                    content = string.Concat(content, line, Environment.NewLine);
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
            if (settings.TransactionalSchema && settings.SchemaDumpFile != null)
            {
                content = string.Concat(content, $"END ${name}$", Environment.NewLine, "LANGUAGE plpgsql;", Environment.NewLine);
            }
            return content;
        }
    }
}
