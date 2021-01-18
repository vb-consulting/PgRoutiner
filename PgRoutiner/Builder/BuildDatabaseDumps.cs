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
        private static void BuildDatabaseDumps(NpgsqlConnection connection)
        {
            var builder = new PgDumpBuilder(Settings.Value, connection);
            BuildDump(connection, Settings.Value.SchemaDumpFile, () => builder.GetSchemaContent());
            BuildDump(connection, Settings.Value.DataDumpFile, () => builder.GetDataContent());
        }

        private static void BuildDump(NpgsqlConnection connection, string dumpFile, Func<string> contentFunc)
        {
            if (string.IsNullOrEmpty(dumpFile))
            {
                return;
            }

            var file = Path.Combine(Program.CurrentDir, dumpFile);
            var shortName = dumpFile;
            var shortFilename = Path.GetFileName(file);
            var dir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(file)));
            var exists = File.Exists(file);

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

            Dump($"Creating dump file {dumpFile}...");
            try
            {
                File.WriteAllText(file, contentFunc());
            }
            catch(Exception e)
            {
                Program.WriteLine(ConsoleColor.Red, $"Could not write dump file {file}", $"ERROR: {e.Message}");
            }
        }
    }
}
