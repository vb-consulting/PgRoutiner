using System;
using System.Collections.Generic;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        public static List<(List<Method> Methods, string @namespace)> Modules = new();
        public static List<(string Content, string FullFileName)> Content = new();
        public static string SchemaFile = null;
        public static string DataFile = null;
        public static string ConnectionName = null;

        public static void Run(NpgsqlConnection connection)
        {
            ConnectionName = Settings.Value.Connection ?? connection.Database.ToUpperCamelCase();
            if (Settings.Value.Execute == null)
            {
                Dump("Running file execution...");
                ExecuteFile(connection);
            }
            if (Settings.Value.Psql)
            {
                Dump("Running psql terminal...");
                new PsqlRunner(Settings.Value, connection).Run();
            }

            if (Settings.Value.Routines)
            {
                Dump("Running routine source code generation...");
                BuildDataAccess(connection);
                DumpContent();
            }
            if (Settings.Value.UnitTests)
            {
                Dump("Running unit test project source code generation...");
                BuildUnitTests(connection);
            }

            if (Settings.Value.DbObjects || Settings.Value.SchemaDump || Settings.Value.DataDump)
            {
                var builder = new PgDumpBuilder(Settings.Value, connection);
                if (Settings.CheckPgDumpVersion(connection, builder, false))
                {
                    if (Settings.Value.SchemaDump)
                    {
                        Dump("Running schema dump file generation...");
                        BuildDump(Settings.Value.SchemaDumpFile, () => builder.GetSchemaContent(), file => SchemaFile = file);
                    }
                    if (Settings.Value.DataDump)
                    {
                        Dump("Running data dump file generation...");
                        BuildDump(Settings.Value.DataDumpFile, () => builder.GetDataContent(), file => DataFile = file);
                    }
                    if (Settings.Value.DbObjects)
                    {
                        Dump("Running schema dump file generation...");
                        BuildObjectDumps(builder);
                    }
                }
            }
            if (Settings.Value.CommitMd)
            {
                Dump("Running commit markdown edits to database...");
                BuildMdDiff(connection);
            }
            if (Settings.Value.Markdown)
            {
                Dump("Running markdown file generation...");
                BuilMd(connection);
            }
        }

        private static void DumpContent()
        {
            if (Content.Count == 0)
            {
                return;
            }
            Dump("Creating source code files...");
            foreach (var item in Content)
            {
                WriteFile(item.FullFileName, item.Content);
            }
        }

        private static void Dump(params string[] lines)
        {
            if (Settings.Value.Dump)
            {
                return;
            }
            Program.WriteLine(ConsoleColor.Yellow, lines);
        }

        private static void Error(string msg)
        {
            Program.WriteLine(ConsoleColor.Red, $"ERROR: {msg}");
        }
    }
}
