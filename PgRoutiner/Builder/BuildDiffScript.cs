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
        /*
    "Diff": false,
    "DiffPgDump": "pg_dump",
    "DiffTarget": null,
    "DiffFilePattern": "./Database/{0}-{1}/{2}-diff.sql"
         */
        private static void BuildDiffScript(NpgsqlConnection connection)
        {
            if (!Settings.Value.Diff || Settings.Value.DiffFilePattern == null)
            {
                return;
            }
            using var target = new ConnectionManager(Program.Config, nameof(Settings.DiffTarget), Settings.Value.Connection).ParseConnectionString();
            if (target == null)
            {
                return;
            }

            var sourceBuilder = new PgDumpBuilder(Settings.Value, connection);
            if (!Settings.CheckPgDumpVersion(sourceBuilder))
            {
                return;
            }

            var targetBuilder = new PgDumpBuilder(Settings.Value, target, nameof(Settings.DiffPgDump));
            if (!Settings.CheckPgDumpVersion(targetBuilder))
            {
                return;
            }

            var targetName = (Settings.Value.DiffTarget ?? $"{target.Host}_{target.Port}_{target.Database}").SanitazePath();
            var dir = Path.GetFullPath(Path.GetDirectoryName(string.Format(Settings.Value.DiffFilePattern, ConnectionName, targetName, 1)));

            if (!Directory.Exists(dir))
            {
                DumpPath("Creating dir: {0}", dir);
                Directory.CreateDirectory(dir);
            }

            var title = string.Format("{0}__diff__{1}", ConnectionName, targetName).SanitazeName();
            var builder = new PgDiffBuilder(Settings.Value, connection, target, sourceBuilder, targetBuilder, title);
            var content = builder.Build();

            Console.WriteLine(content);
        }
    }
}
