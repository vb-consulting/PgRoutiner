using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    public class Extension
    {
        public List<Method> Methods { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
    }

    partial class Builder
    {
        public static List<Extension> Extensions = new();
        public static string SchemaFile = null;
        public static string DataFile = null;

        public static string ConnectionName = null;

        public static void Run(NpgsqlConnection connection)
        {
            ConnectionName = (Settings.Value.Connection ?? $"{connection.Host}_{connection.Port}_{connection.Database}").SanitazePath();

            if (Settings.Value.Execute != null)
            {
                DumpTitle("** EXECUTION **");
                ExecuteFromSetting(connection);
                return;
            }

            if (Settings.Value.Psql)
            {
                DumpTitle("** PSQL TERMINAL **");
                new PsqlRunner(Settings.Value, connection).TryRunFromTerminal();
                return;
            }

            if (Settings.Value.CommitMd)
            {
                DumpTitle("** COMMIT MARKDOWN (MD) EDITS **");
                BuildMdDiff(connection);
                return;
            }

            if (Settings.Value.SchemaDumpFile != null)
            {
                SchemaFile = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.SchemaDumpFile)), ConnectionName);
            }

            if (Settings.Value.DataDumpFile != null)
            {
                DataFile = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.DataDumpFile)), ConnectionName);
            }

            if (Settings.Value.DbObjects || Settings.Value.SchemaDump || Settings.Value.DataDump)
            {
                var builder = new PgDumpBuilder(Settings.Value, connection);
                if (Settings.CheckPgDumpVersion(builder))
                {
                    if (Settings.Value.SchemaDump)
                    {
                        DumpTitle("** SCHEMA DUMP SCRIPT GENERATION **");
                        BuildDump(
                            dumpFile: Settings.Value.SchemaDumpFile, 
                            file: SchemaFile, 
                            overwrite: Settings.Value.SchemaDumpOverwrite, 
                            askOverwrite: Settings.Value.SchemaDumpAskOverwrite, 
                            contentFunc: () => builder.GetSchemaContent());
                    }
                    if (Settings.Value.DataDump)
                    {
                        DumpTitle("** DATA DUMP SCRIPT GENERATION **");
                        BuildDump(
                            dumpFile: Settings.Value.DataDumpFile, 
                            file: DataFile, 
                            overwrite: Settings.Value.DataDumpOverwrite, 
                            askOverwrite: Settings.Value.DataDumpAskOverwrite, 
                            contentFunc: () => builder.GetDataContent());
                    }
                    if (Settings.Value.DbObjects)
                    {
                        DumpTitle("** DATA OBJECTS SCRIPTS TREE GENERATION **");
                        BuildObjectDumps(builder);
                    }
                }
            }

            if (Settings.Value.Diff)
            {
                DumpTitle("** DIFF  SCRIPT GENERATION **");
                BuildDiffScript(connection);
            }

            if (Settings.Value.Routines)
            {
                DumpTitle("** ROUTINE SOURCE CODE GENERATION **");
                BuildDataAccess(connection);
            }

            if (Settings.Value.UnitTests)
            {
                DumpTitle("** UNIT TEST PROJECT TEMPLATE CODE GENERATION **");
                BuildUnitTests(connection);
            }

            if (Settings.Value.Markdown)
            {
                DumpTitle("** MARKDOWN (MD) GENERATION **");
                BuilMd(connection);
            }

            DumpTitle("", "", "**** FINISHED ****");
        }

        private static void Dump(params string[] lines)
        {
            if (Settings.Value.Dump)
            {
                return;
            }
            Program.WriteLine(ConsoleColor.Yellow, lines);
        }

        private static void DumpTitle(params string[] lines)
        {
            if (Settings.Value.Dump)
            {
                return;
            }
            Program.WriteLine(ConsoleColor.Green, lines);
        }

        private static void DumpFormat(string msg, params object[] values)
        {
            if (Settings.Value.Dump)
            {
                return;
            }
            msg = string.Format(msg, values.Select(v => $"`{v}`").ToArray());
            foreach(var (line, i) in msg.Split('`').Select((l, i) => (l, i)))
            {
                if (i % 2 == 0)
                {
                    Program.Write(ConsoleColor.Yellow, line);
                }
                else
                {
                    Program.Write(ConsoleColor.Cyan, line);
                }
            }
            
            Program.WriteLine("");
        }

        private static void DumpRelativePath(string msg, string path)
        {
            DumpFormat(msg, path.GetRelativePath());
        }

        private static void Error(string msg)
        {
            Program.WriteLine(ConsoleColor.Red, $"ERROR: {msg}");
        }
    }
}
