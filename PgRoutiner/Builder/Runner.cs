
namespace PgRoutiner.Builder;

public class Runner
{
    public static void Run(NpgsqlConnection connection)
    {
        var connectionName = (Current.Value.Connection ?? $"{connection.Host}_{connection.Port}_{connection.Database}").SanitazePath();
        if (connectionName.Contains("=") || connectionName.Contains(":") || connectionName.Contains("/"))
        {
            connectionName = $"{connection.Host}_{connection.Port}_{connection.Database}".SanitazePath();
        }

        Crud.BuildCrudRoutines(connection);

        if (Current.Value.Execute != null)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** EXECUTION **");
            Executor.ExecuteFromSetting(connection);
            //return;
        }

        if (Current.Value.Psql)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** PSQL TERMINAL **");
            new PsqlRunner(Current.Value, connection).TryRunFromTerminal();
            //return;
        }

        if (Current.Value.CommitMd)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** COMMIT MARKDOWN (MD) EDITS **");
            Md.MarkdownBuilder.BuildMdDiff(connection);
            //return;
        }

        if (Current.Value.List)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** LIST OBJECTS **");
            var builder = new Dump.PgDumpBuilder(Current.Value, connection);
            if (Dump.PgDumpVersion.Check(builder))
            {
                builder.DumpObjects(connection);
            }
            //return;
        }

        if (Current.Value.ModelOutput != null)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** MODEL OUTPUT **");
            ModelOutput.BuilModelOutput(connection, connectionName);
            //return;
        }

        if (!string.IsNullOrEmpty(Current.Value.Backup))
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** BACKUP **");
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
            if (Current.Value.Verbose) Writer.DumpTitle("** RESTORE **");
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
                    if (Current.Value.Verbose) Writer.DumpTitle("** DATA OBJECTS SCRIPTS TREE GENERATION **");
                    Dump.DumpBuilder.BuildObjectDumps(builder, connectionName);
                }
                if (Current.Value.SchemaDump)
                {
                    if (Current.Value.Verbose) Writer.DumpTitle("** SCHEMA DUMP SCRIPT GENERATION **");
                    Dump.DumpBuilder.BuildDump(
                        dumpFile: Current.Value.SchemaDumpFile,
                        file: schemaFile,
                        //overwrite: Current.Value.SchemaDumpOverwrite,
                        //askOverwrite: Current.Value.SchemaDumpAskOverwrite,
                        overwrite: Current.Value.Overwrite,
                        askOverwrite: Current.Value.AskOverwrite,
                        contentFunc: () => builder.GetSchemaContent());
                }
                if (Current.Value.DataDump)
                {
                    if (Current.Value.Verbose) Writer.DumpTitle("** DATA DUMP SCRIPT GENERATION **");
                    Dump.DumpBuilder.BuildDump(
                        dumpFile: Current.Value.DataDumpFile,
                        file: dataFile,
                        //overwrite: Current.Value.DataDumpOverwrite,
                        //askOverwrite: Current.Value.DataDumpAskOverwrite,
                        overwrite: Current.Value.Overwrite,
                        askOverwrite: Current.Value.AskOverwrite,
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
            if (Current.Value.Verbose) Writer.DumpTitle("** DIFF  SCRIPT GENERATION **");
            DiffBuilder.DiffScript.BuildDiffScript(connection, connectionName);
        }

        if (Current.Value.Routines)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** ROUTINE SOURCE CODE GENERATION **");
            new CodeBuilders.CodeRoutinesBuilder(connection, Current.Value, CodeSettings.ToRoutineSettings(Current.Value)).Build();
        }

        if (Current.Value.UnitTests)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** UNIT TEST PROJECT TEMPLATE CODE GENERATION **");
            CodeBuilders.UnitTests.UnitTestBuilder.BuildUnitTests(connection, schemaFile, dataFile);
        }

        if (Current.Value.Markdown)
        {
            if (Current.Value.Verbose) Writer.DumpTitle("** MARKDOWN (MD) GENERATION **");
            Md.MarkdownBuilder.BuilMd(connection, connectionName);
        }

        if (Current.Value.Verbose) Writer.DumpTitle("", "", "**** FINISHED ****");
    }
}
