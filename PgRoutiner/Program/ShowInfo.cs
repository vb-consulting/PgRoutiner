﻿using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowInfo()
        {
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine($"PgRoutiner ({Version})");
            Console.WriteLine();
            Console.WriteLine("Usage: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  pgroutiner [swith1] [swith2] [swith3] [option1 argument1] [option2 argument2] [option3 argument3]");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("General options:");
            WriteSetting(Settings.DirArgs, "Set working directory, default is current.", "DIR");
            WriteSetting(Settings.HelpArgs, "Show command line help.");
            WriteSetting(Settings.SettingsArgs, "Show current settings.");
            WriteSetting(Settings.DumpArgs, "Displays either SQL to be executed of file to be created to console output.");
            WriteSetting(Settings.ExecuteArgs, "Executes the content of the SQL file from the argument and exits.", "FILE");
            WriteSetting(Settings.ConnectionArgs, "Sets the working connection string name, default is first available", "NAME");
            WriteSetting(Settings.OverwriteArgs, "If this switch is included files will be overwritten.");
            WriteSetting(Settings.AskOverwriteArgs, "If this switch is included prompt with the question will be displayed for file overwrite.");
            WriteSetting(nameof(Settings.SkipIfExists), 
                "List of file names without path to be skipped. Default is empty list.", 
                "NAME", "INDEX");
            WriteSetting(nameof(Settings.SkipUpdateReferences), "Don't ask to update project references required by generated code.");
            WriteSetting(nameof(Settings.Ident),"Number of indentations spaces for generated code. Default is 4.", "NUMBER");
            WriteSetting(Settings.PgDumpArgs, "File path for pg_dump command. Default is pg_dump.", "FILEPATH");
            WriteSetting(nameof(Settings.SourceHeader), "Header text in generated source files. Format placeholder {0} is replaced with time stamp. .Default is \"// <auto-generated />\"", "STRING");
            
            Console.WriteLine();
            Console.WriteLine("Routines data-access extensions code generation options:");
            WriteSetting(Settings.RoutinesArgs, "Run routines data-access extensions code generation.");
            WriteSetting(Settings.OutputDirArgs, "Output dir for generated source code. Default is \"./DataAccess\".", "DIR");
            WriteSetting(nameof(Settings.Namespace), "Namespace for generated source code. Default is project default.", "NAME");
            WriteSetting(Settings.NotSimilarToArgs, "Include routines not similar to expression. Default is null (disabled).", "EXP");
            WriteSetting(Settings.SimilarToArgs, "Include routines similar to expression. Default is null (disabled).", "EXP");
            WriteSetting(nameof(Settings.MinNormVersion), "Minimum Norm package version that works with generated code. Default is 3.1.2.");
            WriteSetting(Settings.SkipSyncMethodsArgs, "Do not generate synchronous methods.");
            WriteSetting(Settings.SkipAsyncMethodsArgs, "Do not generate asynchronous methods.");
            WriteSetting(nameof(Settings.ModelDir), "Directory for model classes files. Default is null (models in source files).", "DIR");
            WriteSetting(nameof(Settings.Mapping),"Custom type mappings. Key is PostgeSQL type name and value is C# type. See documentation for defaults.","VALUE", "KEY");
            WriteSetting(nameof(Settings.CustomModels), "Custom model names. Key is generated model name and value new model name. Default is empty (generated).", "VALUE", "KEY");
            WriteSetting(nameof(Settings.UseRecords), "Use records instead of classes for generated models.");

            Console.WriteLine();
            Console.WriteLine("Schema SQL script options:");
            WriteSetting(Settings.SchemaDumpArgs, "Run schema dump file creation.");
            WriteSetting(Settings.SchemaDumpFileArgs, "Schema dump script file name. Format placeholder {0} is replaced with connection name. Default is \"./Database/{0}/Schema.sql\".", "FILE");
            WriteSetting(nameof(Settings.SchemaDumpOwners), "Include object owners in schema script.");
            WriteSetting(nameof(Settings.SchemaDumpPrivileges), "Include object privileges in schema script.");
            WriteSetting(nameof(Settings.SchemaDumpNoDropIfExists), "Don't include \"drop object if exists\" in schema script.", newLine: true);
            WriteSetting(nameof(Settings.SchemaDumpOptions), "Additional option parameters for pg_dump that creates schema script. Default is null (disabled).", "OPTIONS", newLine: true);
            WriteSetting(nameof(Settings.SchemaDumpNoTransaction), "Don't wrap schema script in transaction block.");

            Console.WriteLine();
            Console.WriteLine("Data SQL script options:");
            WriteSetting(Settings.DataDumpArgs, "Run data dump file creation.");
            WriteSetting(Settings.DataDumpFileArgs, "Schema dump script file name. Format placeholder {0} is replaced with connection name. Default is \"./Database/{0}/Data.sql\".", "FILE");
            WriteSetting(nameof(Settings.DataDumpTables),
                "List of tables to be included into data dump. Default is empty list that includes all tables.",
                "NAME", "INDEX", newLine: true);
            WriteSetting(nameof(Settings.DataDumpOptions), "Additional option parameters for pg_dump that creates data script. Default is null (disabled).", "OPTIONS");
            WriteSetting(nameof(Settings.SchemaDumpNoTransaction), "Don't wrap data script in transaction block.");

            Console.WriteLine();
            Console.WriteLine("Database object files tree (Tables, Views, Functions, Procedures) options:");
            WriteSetting(Settings.DbObjectsArgs, "Run database object files tree creation.");
            WriteSetting(Settings.DbObjectsDirArgs, "Database object files tree root directory. Format placeholder {0} is replaced with connection name. Default is null \"./Database/{0}/\".", "DIR");
            WriteSetting(nameof(Settings.DbObjectsDirNames), "Database object subdirectory names mapping. Default is Tables, Views, Function and Procedures", "VALUE", "KEY", newLine: true);
            WriteSetting(nameof(Settings.DbObjectsSkipDelete), "Don't delete any existing file in tree sub directories (Tables, Views, Functions, Procedures).");
            WriteSetting(nameof(Settings.DbObjectsOwners), "Include object owners in each object file.");
            WriteSetting(nameof(Settings.DbObjectsPrivileges), "Include object privileges in each object file.");
            WriteSetting(nameof(Settings.DbObjectsDropIfExists), "Include drop \"drop object if exists\" for each object in a file.");
            WriteSetting(nameof(Settings.DbObjectsNoCreateOrReplace), "Use \"create or replace\" for views and routines in object files.", newLine: true);
            WriteSetting(nameof(Settings.DbObjectsRaw), "Use raw dump without any parsing for each object file.");
            
            Console.WriteLine();
            Console.WriteLine("Comments markdown (MD) file generation options:");
            WriteSetting(Settings.MarkdownArgs, "Run markdown (MD) file creation.");
            WriteSetting(Settings.MdFileArgs, "Markdown (MD) file built from object comments. Default is null \"./Database/{0}/Dictionary.md\".", "FILE");
            WriteSetting(nameof(Settings.MdSkipRoutines), "Don't include routines in markdown (MD) file.");
            WriteSetting(nameof(Settings.MdSkipViews), "Don't include views in markdown (MD) file.");
            WriteSetting(nameof(Settings.MdNotSimilarTo), "Include objects not similar to expression in markdown (MD) file. Default is null (disabled).", "EXP");
            WriteSetting(nameof(Settings.MdSimilarTo), "Include objects similar to expression in markdown (MD) file. Default is null (disabled).", "EXP");
            WriteSetting(Settings.CommitMdArgs, "Commit manual changes in comments inside markdown (MD) file back to database and exits.", "FILE");
            Console.WriteLine();
            Console.WriteLine("The \"psql\" interactive terminal options:");
            WriteSetting(Settings.PsqlArgs, "Run \"psql\" interactive terminal for the connection and open in new terminal window.");
            WriteSetting(nameof(Settings.PsqlTerminal), "Default terminal program for \"psql\" interactive terminal. Default is \"wt\" (windows terminal).");
            WriteSetting(nameof(Settings.PsqlCommand), "Default \"psql\" command or file path to \"psql\" program. Default is \"psql\"");
            WriteSetting(nameof(Settings.PsqlOptions), "Additional options passed to \"psql\" command. Default is null (not used).");
            ShowSettingsLink();
        }

        private static void ShowSettingsLink()
        {
            WriteLine("", "To learn how to work with settings, visit: ");
            WriteLine(ConsoleColor.Cyan, " https://github.com/vb-consulting/PgRoutiner/blob/master/SETTINGS.MD", "");
        }

        private static void WriteSetting(Arg setting, string value, string parameter = null, string key = null, bool newLine = false)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {Join(setting)}{(key != null ? $":{key}" :"")}");
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
}
