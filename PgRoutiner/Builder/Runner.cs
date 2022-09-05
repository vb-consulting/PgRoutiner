
namespace PgRoutiner.Builder;

public class Runner
{
    public static void Run(NpgsqlConnection connection)
    {
        var connectionName = (Current.Value.Connection ?? $"{connection.Host}_{connection.Port}_{connection.Database}").SanitazePath();

        if (Current.Value.Execute != null)
        {
            Writer.DumpTitle("** EXECUTION **");
            Executor.ExecuteFromSetting(connection);
            //return;
        }

        if (Current.Value.Psql)
        {
            Writer.DumpTitle("** PSQL TERMINAL **");
            new PsqlRunner(Current.Value, connection).TryRunFromTerminal();
            //return;
        }

        if (Current.Value.CommitMd)
        {
            Writer.DumpTitle("** COMMIT MARKDOWN (MD) EDITS **");
            Md.MarkdownBuilder.BuildMdDiff(connection);
            //return;
        }

        if (Current.Value.List)
        {
            Writer.DumpTitle("** LIST OBJECTS **");
            var builder = new Dump.PgDumpBuilder(Current.Value, connection);
            if (Dump.PgDumpVersion.Check(builder))
            {
                builder.DumpObjects(connection);
            }
            //return;
        }

        if (!string.IsNullOrEmpty(Current.Value.Backup))
        {
            Writer.DumpTitle("** BACKUP **");
            var builder = new Dump.PgDumpBuilder(Current.Value, connection, utf8: false);
            if (Dump.PgDumpVersion.Check(builder))
            {
                var split = Current.Value.Backup.Split(" ");
                var file = string.Format(split[0], DateTime.Now, connectionName)
                    .Replace(" ", "_")
                    .Replace(":", "_");
                var options = string.Join(' ', split[1..]);
                builder.Run($"-j 10 -Fd -Z 9 -d {connection.Database}{(options ?? "")}{(Current.Value.BackupOwner ? "" : " --no-owner")} -f {file}");
            }
            //return;
        }

        if (!string.IsNullOrEmpty(Current.Value.Restore))
        {
            Writer.DumpTitle("** RESTORE **");
            var builder = new Dump.PgDumpBuilder(Current.Value, connection, dumpName: nameof(Current.PgRestore), utf8: false);
            if (Dump.PgDumpVersion.Check(builder, restore: true))
            {
                var split = Current.Value.Restore.Split(" ");
                var file = split[0];
                var options = string.Join(' ', split[1..]);
                builder.Run($"-j 10 -Fd -d {connection.Database}{(options ?? "")}{(Current.Value.RestoreOwner ? "" : " --no-owner")} {file}");
            }
            //return;
        }

        string schemaFile = null;
        string dataFile = null;

        if (Current.Value.SchemaDumpFile != null)
        {
            schemaFile = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Current.Value.SchemaDumpFile)), connectionName);
        }

        if (Current.Value.DataDumpFile != null)
        {
            dataFile = string.Format(Path.GetFullPath(Path.Combine(Program.CurrentDir, Current.Value.DataDumpFile)), connectionName);
        }

        if (Current.Value.DbObjects || Current.Value.SchemaDump || Current.Value.DataDump || !string.IsNullOrEmpty(Current.Value.Definition))
        {
            var builder = new Dump.PgDumpBuilder(Current.Value, connection);
            if (Dump.PgDumpVersion.Check(builder))
            {
                if (Current.Value.DbObjects)
                {
                    Writer.DumpTitle("** DATA OBJECTS SCRIPTS TREE GENERATION **");
                    Dump.DumpBuilder.BuildObjectDumps(builder, connectionName);
                }
                if (Current.Value.SchemaDump)
                {
                    Writer.DumpTitle("** SCHEMA DUMP SCRIPT GENERATION **");
                    Dump.DumpBuilder.BuildDump(
                        dumpFile: Current.Value.SchemaDumpFile,
                        file: schemaFile,
                        overwrite: Current.Value.SchemaDumpOverwrite,
                        askOverwrite: Current.Value.SchemaDumpAskOverwrite,
                        contentFunc: () => builder.GetSchemaContent());
                }
                if (Current.Value.DataDump)
                {
                    Writer.DumpTitle("** DATA DUMP SCRIPT GENERATION **");
                    Dump.DumpBuilder.BuildDump(
                        dumpFile: Current.Value.DataDumpFile,
                        file: dataFile,
                        overwrite: Current.Value.DataDumpOverwrite,
                        askOverwrite: Current.Value.DataDumpAskOverwrite,
                        contentFunc: () => builder.GetDataContent());
                }

                if (!string.IsNullOrEmpty(Current.Value.Definition))
                {
                    builder.DumpObjectDefintion();
                }
            }
        }

        if (Current.Value.Diff)
        {
            Writer.DumpTitle("** DIFF  SCRIPT GENERATION **");
            DiffBuilder.DiffScript.BuildDiffScript(connection, connectionName);
        }

        if (Current.Value.Routines)
        {
            Writer.DumpTitle("** ROUTINE SOURCE CODE GENERATION **");
            new CodeBuilders.CodeRoutinesBuilder(connection, Current.Value, CodeSettings.ToRoutineSettings(Current.Value)).Build();
        }

        if (Current.Value.Crud)
        {
            Writer.DumpTitle("** CRUD SOURCE CODE GENERATION **");
            new CodeBuilders.Crud.CodeCrudBuilder(connection, Current.Value, CodeSettings.ToCrudSettings(Current.Value)).Build();
        }

        if (Current.Value.UnitTests)
        {
            Writer.DumpTitle("** UNIT TEST PROJECT TEMPLATE CODE GENERATION **");
            CodeBuilders.UnitTests.UnitTestBuilder.BuildUnitTests(connection, schemaFile, dataFile);
        }

        if (Current.Value.Markdown)
        {
            Writer.DumpTitle("** MARKDOWN (MD) GENERATION **");
            Md.MarkdownBuilder.BuilMd(connection, connectionName);
        }

        Writer.DumpTitle("", "", "**** FINISHED ****");
    }
}
