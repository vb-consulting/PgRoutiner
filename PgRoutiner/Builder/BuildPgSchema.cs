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
        private static void BuildPgSchema(NpgsqlConnection connection)
        {
            if (string.IsNullOrEmpty(Settings.Value.SchemaDumpFile))
            {
                return;
            }

            var schemaFile = Path.Combine(Program.CurrentDir, Settings.Value.SchemaDumpFile);
            var shortName = Settings.Value.SchemaDumpFile;
            var shortFilename = Path.GetFileName(schemaFile);
            var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(schemaFile)));
            var exists = File.Exists(schemaFile);

            if (!Directory.Exists(dir))
            {
                Dump($"Creating dir: {dir}");
                Directory.CreateDirectory(dir);
            }

            if (exists && Settings.Value.Overwrite == false)
            {
                Dump($"File {shortName} exists, overwrite is set to false, skipping ...");
                return;
            }
            if (exists && Settings.Value.SkipIfExists != null && (
                Settings.Value.SkipIfExists.Contains(shortName) || Settings.Value.SkipIfExists.Contains(shortFilename))
                )
            {
                Dump($"Skipping {shortName}, already exists...");
                return;
            }
            if (exists && Settings.Value.AskOverwrite && Program.Ask($"File {shortName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
            {
                Dump($"Skipping {shortName}...");
                return;
            }


            Dump($"Building {Settings.Value.SchemaDumpFile}...");
            var builder = new PgSchemaBuilder(Settings.Value, connection);

            Content.Add((builder.GetPgDumpContent(), schemaFile));
        }
    }
}
