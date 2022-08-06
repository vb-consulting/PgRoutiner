﻿namespace PgRoutiner.SettingsManagement;

public class Info
{
    public static void ShowVersion()
    {
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine($"PgRoutiner ({Program.Version})");
        Console.WriteLine();
    }

    public static void ShowStartupInfo()
    {
        Program.WriteLine(ConsoleColor.Yellow, $"PgRoutiner: {Program.Version}");
        Program.Write(ConsoleColor.Yellow, "Type ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Settings.HelpArgs.Alias}");
        Program.Write(ConsoleColor.Yellow, " or ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Settings.HelpArgs.Name}");
        Program.Write(ConsoleColor.Yellow, " to see help on available commands and settings.");
        Program.WriteLine("");
        Program.Write(ConsoleColor.Yellow, "Type ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Settings.SettingsArgs.Alias}");
        Program.Write(ConsoleColor.Yellow, " or ");
        Program.Write(ConsoleColor.Cyan, $"pgroutiner {Settings.SettingsArgs.Name}");
        Program.WriteLine(ConsoleColor.Yellow, " to see the currently selected settings.");
        Program.Write(ConsoleColor.Yellow, "Issues");
        Program.WriteLine(ConsoleColor.Cyan, "   https://github.com/vb-consulting/PgRoutiner/issues");
        Program.Write(ConsoleColor.Yellow, "Donate");
        Program.Write(ConsoleColor.Cyan, "   bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv");
        Program.WriteLine(ConsoleColor.Cyan, "   https://www.paypal.com/paypalme/vbsoftware/");
        Program.WriteLine(ConsoleColor.Yellow, $"Copyright (c) VB Consulting and VB Software {DateTime.Now.Year}.",
            "This program and source code is licensed under the MIT license.");
        Program.WriteLine(ConsoleColor.Cyan, "https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE");
    }

    public static bool ShowDebug(bool customSettings)
    {
        if (Switches.Value.Settings && !customSettings)
        {
            Settings.ShowSettings();
            return true;
        }

        if (Switches.Value.Debug)
        {
            Program.WriteLine("", "Debug: ");
            Program.WriteLine("Version: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + Program.Version);
            var path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Program.WriteLine("Executable dir: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + path);
            Program.WriteLine("OS: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + Environment.OSVersion);
            Program.WriteLine("Run: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Routines}");
            Program.WriteLine("CommitComments: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Settings.Value.CommitMd}");
            Program.WriteLine("Dump: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Dump}");
            Program.WriteLine("Execute: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Execute ?? "<null>"}");
            Program.WriteLine("Diff: ");
            Program.WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Diff}");
            return true;
        }

        return false;
    }

    public static void ShowInfo()
    {
        ShowVersion();
        Console.WriteLine("Usage: ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  pgroutiner [swith1] [swith2] [swith3] [option1 argument1] [option2 argument2] [option3 argument3]");
        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine("General options and settings: ");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/1.-WORKING-WITH-SETTINGS#general-settings");

        WriteSetting(Settings.DirArgs, "Set working directory, default is current.", "DIR");
        WriteSetting(Settings.HelpArgs, "Show command-line help.");
        WriteSetting(Settings.VersionArgs, "Show current version.");
        WriteSetting(Settings.SettingsArgs, "Show current settings.");
        WriteSetting(Settings.InfoArgs, "Display current info (dir, config files, used settings and connection) and exit.");
        WriteSetting(Settings.DumpArgs, "Displays either SQL to be executed or the file to be created to console output.");
        WriteSetting(Settings.ExecuteArgs, "Executes the content of the SQL file from the argument or execute PSQL command if file doesn't exists and display the results.", "FILE_OR_PSQL_COMMAND", newLine: true);
        WriteSetting(Settings.ConnectionArgs, "Sets the working connection string name. Default is first available.", "NAME");
        WriteSetting(nameof(Settings.SkipConnectionPrompt), "If connection part value exists as enviorment variable, skip the prompt (don't ask).");
        WriteSetting(Settings.SchemaArgs, "Use only schemas similar to this expression. Default is null (all schemas).", "EXP");
        WriteSetting(nameof(Settings.SkipIfExists),
            "List of filenames without a path to be skipped. Default is an empty list.",
            "NAME", "INDEX");
        WriteSetting(nameof(Settings.SkipUpdateReferences), "Don't ask to update project references required by generated code.");
        WriteSetting(Settings.PgDumpArgs, "File path for pg_dump command. Default is pg_dump.", "FILEPATH");
        WriteSetting(nameof(Settings.PgDumpFallback), "Fall-back path for pg_dump command if pg_dump version doesn't match connection version. The default depends on OS.", "FILEPATH");
        WriteSetting(nameof(Settings.ConfigPath), "Additional json configuration file path. You can reference another project in another dir to use that connection string.", "FILEPATH");

        Console.WriteLine();
        Console.WriteLine("Code generation general settings: ");
        WriteSetting(nameof(Settings.Namespace), "Namespace for generated source code. Default is project default.", "NAME");
        WriteSetting(nameof(Settings.UseRecords), "Use records instead of classes for generated models.");
        WriteSetting(nameof(Settings.UseExpressionBody), "Use expression body instead of statement body for generated methods.");
        WriteSetting(nameof(Settings.Mapping), "Custom type mappings. Key is PostgreSQL type name and value is C# type. See documentation for defaults.", "VALUE", "KEY");
        WriteSetting(nameof(Settings.CustomModels), "Custom model names. Key is generated model name and value new model name. Default is empty (generated).", "VALUE", "KEY");
        WriteSetting(nameof(Settings.ModelDir), "Directory for the model classes files. Default is null (models in source files).", "DIR");

        WriteSetting(nameof(Settings.ModelCustomNamespace), "Custom namespace for model files, if, for example they are created in another project.", "NAME", newLine: true);
        WriteSetting(nameof(Settings.EmptyModelDir), "Should the model dir be emptied before file generation. If this dir is project root, this option will only yield a warning.");

        WriteSetting(Settings.SkipSyncMethodsArgs, "Do not generate synchronous methods.");
        WriteSetting(Settings.SkipAsyncMethodsArgs, "Do not generate asynchronous methods.");
        WriteSetting(nameof(Settings.MinNormVersion), "Minimum Norm package version that works with generated code. Default is 4.0.0.");
        WriteSetting(nameof(Settings.SourceHeaderLines), "Header text in generated source files. Format placeholder {0} is replaced with time stamp. .Default is \"// <auto-generated />\"", "STRING");
        WriteSetting(nameof(Settings.Ident), "Number of indentations spaces for generated code. Default is 4.", "NUMBER");
        WriteSetting(nameof(Settings.ReturnMethod), "Linq method name that should be used to yield a single value from enumaration. Default is \"SingleOrDefault\". Other values can be Single, First or FirstOrDefault", "STRING", newLine: true);

        Console.WriteLine();
        Console.WriteLine("Routines data-access extensions code-generation options and settings: ");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/2.-WORKING-WITH-ROUTINES#routines-data-access-extensions-code-generation-settings");

        WriteSetting(Settings.RoutinesArgs, "Run routines data-access extensions code generation.");
        WriteSetting(Settings.OutputDirArgs, "Output dir for generated source code. Default is \"./DataAccess\".", "DIR");
        WriteSetting(nameof(Settings.RoutinesEmptyOutputDir), "Should the routines dir be emptied before file generation. If this dir is project root, this option will only yield a warning.");
        WriteSetting(Settings.RoutinesOverwriteArgs, "If this switch is included routine files will be overwritten.");
        WriteSetting(Settings.RoutinesAskOverwriteArgs, "If this switch is included prompt with the question will be displayed for routine file overwrite.", newLine: true);
        WriteSetting(Settings.NotSimilarToArgs, "Include routines not similar to expression. Default is null (disabled).", "EXP");
        WriteSetting(Settings.SimilarToArgs, "Include routines similar to expression. Default is null (disabled).", "EXP");
        WriteSetting(nameof(Settings.RoutinesReturnMethods), $"Linq method name that should be used to yield a single value from enumaration for individual routine. Overrides {nameof(Settings.ReturnMethod)} setting. Key is either routine name or method name and value is linq method name that returns the result. If value is null routine yields enumeration.", "VALUE", "KEY", newLine: true);

        Console.WriteLine();
        Console.WriteLine("Unit tests code-generation options and settings: ");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/3.-WORKING-WITH-UNIT-TESTS#unit-tests-code-generation-settings");
        WriteSetting(Settings.UnitTestsArgs, "Run unit test project code generation.");
        WriteSetting(Settings.UnitTestsDirArgs, "Output dir for the unit test project. {0} placeholder is for namespace or main project name. Default is \"../{0}Tests\".", "DIR");
        WriteSetting(nameof(Settings.UnitTestsAskRecreate), "Ask should the project file be recreated. Default is false.");

        Console.WriteLine();
        Console.WriteLine("Schema dump script options and settings: ");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/4.-WORKING-WITH-SCHEMA-DUMP-SCRIPT#schema-dump-script-settings");
        WriteSetting(Settings.SchemaDumpArgs, "Run schema dump file creation.");
        WriteSetting(Settings.SchemaDumpFileArgs, "Schema dump script file name. Format placeholder {0} is replaced with connection name. Default is \"./Database/{0}/Schema.sql\".", "FILE");
        WriteSetting(Settings.SchemaDumpOverwriteArgs, "If this switch is included schema dump file will be overwritten.", newLine: true);
        WriteSetting(Settings.SchemaDumpAskOverwriteArgs, "If this switch is included prompt with the question will be displayed for schema file overwrite.", newLine: true);
        WriteSetting(nameof(Settings.SchemaDumpOwners), "Include object owners in schema script.");
        WriteSetting(nameof(Settings.SchemaDumpPrivileges), "Include object privileges in schema script.");
        WriteSetting(nameof(Settings.SchemaDumpNoDropIfExists), "Don't include \"drop object if exists\" in schema script.", newLine: true);
        WriteSetting(nameof(Settings.SchemaDumpOptions), "Additional option parameters for pg_dump that creates schema script. Default is null (disabled).", "OPTIONS", newLine: true);
        WriteSetting(nameof(Settings.SchemaDumpNoTransaction), "Don't wrap schema script in a transaction block.");

        Console.WriteLine();
        Console.WriteLine("Data dump script options and settings: ");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/5.-WORKING-WITH-DATA-DUMP-SCRIPT#data-dump-script-settings");
        WriteSetting(Settings.DataDumpArgs, "Run data dump file creation.");
        WriteSetting(Settings.DataDumpFileArgs, "Schema dump script file name. Format placeholder {0} is replaced with connection name. Default is \"./Database/{0}/Data.sql\".", "FILE");
        WriteSetting(Settings.DataDumpOverwriteArgs, "If this switch is included data dump file will be overwritten.");
        WriteSetting(Settings.DataDumpAskOverwriteArgs, "If this switch is included prompt with the question will be displayed for data file overwrite.", newLine: true);
        WriteSetting(nameof(Settings.DataDumpTables),
            "List of the tables to be included in the data dump. Default is empty list that includes all tables.",
            "NAME", "INDEX", newLine: true);
        WriteSetting(nameof(Settings.DataDumpOptions), "Additional option parameters for pg_dump that creates data script. Default is null (disabled).", "OPTIONS");
        WriteSetting(nameof(Settings.SchemaDumpNoTransaction), "Don't wrap data script in a transaction block.");

        Console.WriteLine();
        Console.WriteLine("Object file tree options and settings: ");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/6.-WORKING-WITH-OBJECT-FILES-TREE#object-file-tree-settings");
        WriteSetting(Settings.DbObjectsArgs, "Run database object files tree creation.");
        WriteSetting(Settings.DbObjectsDirArgs, "Database object files tree root directory. Format placeholder {0} is replaced with connection name. Default is null \"./Database/{0}/\".", "DIR");
        WriteSetting(Settings.DbObjectsOverwriteArgs, "If this switch is included data object files will be overwritten.");
        WriteSetting(Settings.DbObjectsAskOverwriteArgs, "If this switch is included prompt with the question will be displayed for object data file overwrite.", newLine: true);
        WriteSetting(nameof(Settings.DbObjectsDirNames), "Database object subdirectory names mapping. Default is Tables, Views, Function and Procedures", "VALUE", "KEY", newLine: true);
        WriteSetting(nameof(Settings.DbObjectsSkipDeleteDir), "Don't delete any existing file in tree subdirectories (Tables, Views, Functions, Procedures).");
        WriteSetting(nameof(Settings.DbObjectsOwners), "Include object owners in each object file.");
        WriteSetting(nameof(Settings.DbObjectsPrivileges), "Include object privileges in each object file. Default is false.");
        WriteSetting(nameof(Settings.DbObjectsCreateOrReplace), "Use \"create or replace\" for views and routines in object files.", newLine: true);
        WriteSetting(nameof(Settings.DbObjectsRaw), "Use raw dump without any parsing for each object file.");

        Console.WriteLine();
        Console.WriteLine("Markdown (MD) database dictionaries options and settings:");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/7.-WORKING-WITH-MARKDOWN-DATABASE-DICTIONARIES#markdown-md-database-dictionaries-settings");
        WriteSetting(Settings.MarkdownArgs, "Run markdown (MD) file creation.");
        WriteSetting(Settings.MdFileArgs, "Markdown (MD) file built from object comments. Default is null \"./Database/{0}/Dictionary.md\".", "FILE");
        WriteSetting(Settings.MdOverwriteArgs, "If this switch is included markdown (MD) file will be overwritten.");
        WriteSetting(Settings.MdAskOverwriteArgs, "If this switch is included prompt with the question will be displayed for markdown (MD) file overwrite.");
        WriteSetting(nameof(Settings.MdSkipRoutines), "Don't include routines in markdown (MD) file.");
        WriteSetting(nameof(Settings.MdSkipViews), "Don't include views in the markdown (MD) file.");
        WriteSetting(nameof(Settings.MdNotSimilarTo), "Include objects not similar to expression in markdown (MD) file. Default is null (disabled).", "EXP");
        WriteSetting(nameof(Settings.MdSimilarTo), "Include objects similar to the expression in the markdown (MD) file. Default is null (disabled).", "EXP");
        WriteSetting(Settings.CommitMdArgs, "Commit manual changes in comments inside markdown (MD) file back to database and exits.");
        Console.WriteLine();
        Console.WriteLine("PSQL command-line utility options and settings:");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/8.-WORKING-WITH-PSQL#psql-command-line-utility-settings");
        WriteSetting(Settings.PsqlArgs, "Run \"psql\" interactive terminal for the connection and open in new terminal window.");
        WriteSetting(nameof(Settings.PsqlTerminal), "Default terminal program for \"psql\" interactive terminal. Default is \"wt\" (windows terminal).", "CMD");
        WriteSetting(nameof(Settings.PsqlCommand), "Default \"psql\" command or file path to \"psql\" program. Default is \"psql\"", "CMD");
        WriteSetting(nameof(Settings.PsqlOptions), "Additional options passed to \"psql\" command. Default is null (not used).", "OPTIONS");
        Console.WriteLine();
        Console.WriteLine("Diff scripts options and settings:");
        Console.WriteLine("  https://github.com/vb-consulting/PgRoutiner/wiki/9.-WORKING-WITH-DIFF-SCRIPTS#diff-scripts-settings");
        WriteSetting(Settings.DiffArgs, "Run a diff script creation.");
        WriteSetting(Settings.DiffTargetArgs, "Name of the target connection to create a diff for. Default is null (not used).", "NAME", newLine: true);
        WriteSetting(nameof(Settings.DiffFilePattern), "Diff script file pattern. {0} is source connection name, {1} is target connection name, {2} is script number and {3} is current date. Set to null or empty to dump difference script content to console output. Default is \"./Database/{0}-{1}/{2}-diff-{3:yyyyMMdd}.sql\".", "FILEPATH");
        WriteSetting(Settings.DiffPgDumpArgs, "File path for pg_dump command for the target connection. Default is pg_dump.", "FILEPATH");
        WriteSetting(nameof(Settings.DiffPrivileges), "Include object privileges in the diff script file. Default is false.");

        Console.WriteLine();
        Console.WriteLine("CRUD data-access extensions code-generation options and settings: ");
        Console.WriteLine("  ");
        WriteSetting(Settings.CrudArgs, "Run CRUD data-access extensions code generation.");
        WriteSetting(Settings.CrudOutputDirArgs, "Output dir for generated source code. Default is \"./Extensions\".", "DIR", newLine: true);
        WriteSetting(nameof(Settings.CrudEmptyOutputDir), "Should the CRUD dir be emptied before file generation. If this dir is project root, this option will only yield a warning.");

        WriteSetting(nameof(Settings.CrudOverwrite), "If this switch is included CRUD files will be overwritten.");
        WriteSetting(nameof(Settings.CrudAskOverwrite), "If this switch is included prompt with the question will be displayed for CRUD file overwrite.");
        WriteSetting(nameof(Settings.CrudNoPrepare), "If this switch is included CRUD extensions will not prepare command. All CRUD commands are prepared in advance.");
        WriteSetting(nameof(Settings.CrudReturnMethods), $"Linq method name that should be used to yield a single value from enumaration for individual crud routine. Overrides {nameof(Settings.ReturnMethod)} setting. Key is either table name or method name and value is linq method name that returns the result. If value is null routine yields enumeration.", "VALUE", "KEY", newLine: true);
        WriteSetting(nameof(Settings.CrudCreate), "CRUD create tables list.", "NAME", "INDEX");
        WriteSetting(nameof(Settings.CrudCreateReturning), "CRUD create returning tables list.", "NAME", "INDEX", newLine: true);
        WriteSetting(nameof(Settings.CrudCreateOnConflictDoNothing), "CRUD create on conflict do nothing tables list.", "NAME", "INDEX", newLine: true);
        WriteSetting(nameof(Settings.CrudCreateOnConflictDoNothingReturning), "CRUD create on conflict do nothing returning tables list.", "NAME", "INDEX", newLine: true);
        WriteSetting(nameof(Settings.CrudCreateOnConflictDoUpdate), "CRUD create on conflict do update tables list.", "NAME", "INDEX", newLine: true);
        WriteSetting(nameof(Settings.CrudCreateOnConflictDoUpdateReturning), "CRUD create on conflict do update returning tables list.", "NAME", "INDEX", newLine: true);
        WriteSetting(nameof(Settings.CrudReadBy), "CRUD ready by primary keys tables list.", "NAME", "INDEX");
        WriteSetting(nameof(Settings.CrudReadAll), "CRUD read all tables list.", "NAME", "INDEX");
        WriteSetting(nameof(Settings.CrudUpdate), "CRUD update tables list.", "NAME", "INDEX");
        WriteSetting(nameof(Settings.CrudUpdateReturning), "CRUD update returning tables list.", "NAME", "INDEX", newLine: true);
        WriteSetting(nameof(Settings.CrudDeleteBy), "CRUD delete by primary keys tables list.", "NAME", "INDEX");
        WriteSetting(nameof(Settings.CrudDeleteByReturning), "CRUD delete by primary keys returning tables list.", "NAME", "INDEX", newLine: true);

        Settings.ShowSettingsLink();
    }

    private static void WriteSetting(Arg setting, string value, string parameter = null, string key = null, bool newLine = false)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($" {Join(setting)}{(key != null ? $":{key}" : "")}");
        if (parameter != null)
        {
            Console.Write($" {parameter}");
        }
        Console.ResetColor();
        if (newLine)
        {
            Console.WriteLine();
        }
        Console.CursorLeft = 30;
        Console.WriteLine(value);
    }

    private static void WriteSetting(string setting, string value, string parameter = null, string key = null, bool newLine = false)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($" --{setting.ToKebabCase()}{(key != null ? $":{key}" : "")}");
        if (parameter != null)
        {
            Console.Write($" {parameter}");
        }
        Console.ResetColor();
        if (newLine)
        {
            Console.WriteLine();
        }
        Console.CursorLeft = 30;
        Console.WriteLine(value);
    }

    private static string Join(Arg value) =>
        string.Join(",", new[] { value.Alias.ToKebabCase(), $"--{value.Original.ToKebabCase()}" }.Where(i => i != null));
}
