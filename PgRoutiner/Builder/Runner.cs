
namespace PgRoutiner.Builder;

public class Runner
{
    public static void Run(NpgsqlConnection connection)
    {
        var connectionName = (Settings.Value.Connection ?? $"{connection.Host}_{connection.Port}_{connection.Database}").SanitazePath();

        if (Settings.Value.Execute != null)
        {
            Writer.DumpTitle("** EXECUTION **");
            Executor.ExecuteFromSetting(connection);
            return;
        }

        if (Settings.Value.Psql)
        {
            Writer.DumpTitle("** PSQL TERMINAL **");
            new PsqlRunner(Settings.Value, connection).TryRunFromTerminal();
            return;
        }

        if (Settings.Value.CommitMd)
        {
            Writer.DumpTitle("** COMMIT MARKDOWN (MD) EDITS **");
            Md.MarkdownBuilder.BuildMdDiff(connection);
            return;
        }

        string schemaFile = null;
        string dataFile = null;

        if (Settings.Value.SchemaDumpFile != null)
        {
            schemaFile = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.SchemaDumpFile)), connectionName);
        }

        if (Settings.Value.DataDumpFile != null)
        {
            dataFile = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Settings.Value.DataDumpFile)), connectionName);
        }

        if (Settings.Value.DbObjects || Settings.Value.SchemaDump || Settings.Value.DataDump)
        {
            var builder = new Dump.PgDumpBuilder(Settings.Value, connection);
            if (Dump.PgDumpVersion.Check(builder))
            {
                if (Settings.Value.DbObjects)
                {
                    Writer.DumpTitle("** DATA OBJECTS SCRIPTS TREE GENERATION **");
                    Dump.DumpBuilder.BuildObjectDumps(builder, connectionName);
                }
                if (Settings.Value.SchemaDump)
                {
                    Writer.DumpTitle("** SCHEMA DUMP SCRIPT GENERATION **");
                    Dump.DumpBuilder.BuildDump(
                        dumpFile: Settings.Value.SchemaDumpFile,
                        file: schemaFile,
                        overwrite: Settings.Value.SchemaDumpOverwrite,
                        askOverwrite: Settings.Value.SchemaDumpAskOverwrite,
                        contentFunc: () => builder.GetSchemaContent());
                }
                if (Settings.Value.DataDump)
                {
                    Writer.DumpTitle("** DATA DUMP SCRIPT GENERATION **");
                    Dump.DumpBuilder.BuildDump(
                        dumpFile: Settings.Value.DataDumpFile,
                        file: dataFile,
                        overwrite: Settings.Value.DataDumpOverwrite,
                        askOverwrite: Settings.Value.DataDumpAskOverwrite,
                        contentFunc: () => builder.GetDataContent());
                }
            }
        }

        if (Settings.Value.Diff)
        {
            Writer.DumpTitle("** DIFF  SCRIPT GENERATION **");
            DiffBuilder.DiffScript.BuildDiffScript(connection, connectionName);
        }

        if (Settings.Value.Routines)
        {
            Writer.DumpTitle("** ROUTINE SOURCE CODE GENERATION **");
            new CodeBuilders.CodeRoutinesBuilder(connection, Settings.Value, CodeSettings.ToRoutineSettings(Settings.Value)).Build();
        }

        if (Settings.Value.Crud)
        {
            Writer.DumpTitle("** CRUD SOURCE CODE GENERATION **");
            new CodeBuilders.Crud.CodeCrudBuilder(connection, Settings.Value, CodeSettings.ToCrudSettings(Settings.Value)).Build();
        }

        if (Settings.Value.UnitTests)
        {
            Writer.DumpTitle("** UNIT TEST PROJECT TEMPLATE CODE GENERATION **");
            CodeBuilders.UnitTests.UnitTestBuilder.BuildUnitTests(connection, schemaFile, dataFile);
        }

        if (Settings.Value.Markdown)
        {
            Writer.DumpTitle("** MARKDOWN (MD) GENERATION **");
            Md.MarkdownBuilder.BuilMd(connection, connectionName);
        }

        Writer.DumpTitle("", "", "**** FINISHED ****");
    }
}
