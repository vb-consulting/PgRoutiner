using System;
using System.Collections.Generic;
using System.IO;
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
            if (!Settings.Value.Diff)
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
            Dump("");
            var targetName = (Settings.Value.DiffTarget ?? $"{target.Host}_{target.Port}_{target.Database}").SanitazePath();
            var title = string.Format("{0}__diff__{1}", ConnectionName, targetName).SanitazeName();
            var builder = new PgDiffBuilder(Settings.Value, connection, target, sourceBuilder, targetBuilder, title);
            var content = builder.Build((msg, step, total) => 
            {
                DumpFormat(string.Concat("Step ", step.ToString(), " of ", total.ToString(), ": {0}"), msg);
            });
            if (string.IsNullOrEmpty(content))
            {
                Dump($"No differences found between {ConnectionName} and {targetName}...");
                return;
            }
            if (Settings.Value.DiffFilePattern != null)
            {
                var dir = Path.GetFullPath(Path.GetDirectoryName(string.Format(Settings.Value.DiffFilePattern, ConnectionName, targetName, 1)));
                if (!Directory.Exists(dir))
                {
                    DumpFormat("Creating dir: {0}", dir);
                    Directory.CreateDirectory(dir);
                }
                int i = 1;
                foreach (var existingFile in Directory.EnumerateFiles(dir))
                {
                    var fileContent = File.ReadAllText(existingFile);
                    if (Equals(content, fileContent))
                    {
                        DumpRelativePath("File with same difference script {0} already exists ...", existingFile);
                        return;
                    }
                    i++;
                }

                var file = string.Format(Settings.Value.DiffFilePattern, ConnectionName, targetName, i);
                DumpRelativePath("Creating new diff file: {0} ...", Path.GetFullPath(file));
                WriteFile(file, content);
            }
            else
            {
                Dump("");
                DumpFormat("Source connection: {0}", ConnectionName);
                DumpFormat("Target connection: {0}", targetName);
                Dump("Difference script:");
                Console.Write(content);
            }
        }
    }
}
