using System;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowInfo()
        {
            WriteLine("");
            Console.Write("Usage: ");

            WriteLine(ConsoleColor.Yellow, "PgRoutiner [dir] [-h,--help] [-r,--run] [settings]", "");
            WriteSetting("[dir]", "Set working directory for your project other than current.");
            WriteSetting("[-h,--help]", "Show help, your current settings and exit immediately.");
            WriteSetting("[-r,--run]", "Start the source code generation.");
            WriteSetting("[settings]", "Override one of the available settings by using `setting=value` argument format");

            WriteLine("", "Settings:");
            WriteSetting("connection", "[name] - Connection string name from your configuration connection string to be used. Default first available connection string.");
            WriteSetting("outputDir", "[path] - Relative path where generated source files will be saved. Default is current dir.");
            WriteSetting("modelDir", "[path] -  Relative path where model classes source files will be saved. Default value saves model classes in the same file as a related data-access code.");
            WriteSetting("schema", "[pattern] - Use only PostgreSQL schemas matching `SIMILAR TO` pattern. Default is none (not used, all schemas).");
            WriteSetting("overwrite", "[true|false] - should existing generated source file be overwritten (true, default) or skipped if they exist (false)");
            WriteSetting("askOverwrite", "[true|false] - prompt a question should overwrite a file (Y/N). Only if overwrite is set true.");
            WriteSetting("namespace", "[name] - Root namespace for generated source files. Default is project root namespace. ");
            WriteSetting("notSimilarTo", "[pattern] - `NOT SIMILAR TO` pattern used to search routine names. Default skips this matching.");
            WriteSetting("similarTo", "[pattern] - `SIMILAR TO` pattern used to search routine names. Default skips this matching.");
            WriteSetting("sourceHeader", "[string] - Insert the following content to the start of each generated source code file. Default is \"// [auto-generated at timestamp /]\")");
            WriteSetting("syncMethod", "[true|false] - Generate a `sync` method, true or false. Default is true.");
            WriteSetting("asyncMethod", "[true|false] - Generate a `async` method, true or false. Default is true.");
            WriteSetting("ident", "[number] - Number of default indentation spaces for the generated code. Tabs are not supported. Default is 4.");
            WriteSetting("mapping:key=value", "Key-values to override default type mapping. Key is PostgreSQL UDT type name and value is the corresponding C# type name.");
            WriteSetting("customModels:key=value", "Key-value dictionary to set custom models name. Key is expected model name and value is the new model name. Default is none (empty).");
            WriteSetting("skipIfExists:key=value", "Key-values list of file names (without dir) that will be skipped if they already exist.");
            WriteSetting("updateReferences", "[true|false] - Update required project references. Default is true.");
            WriteSetting("minNormVersion", "[version] - Minimal Norm package version. Default is 3.2.2");
            WriteSetting("disableCodeGen", "[true|false] - Disable data access code generation. Default is false.");
            
            WriteSetting("pgDump", "[name] - pg_dump program name. Use this option if you need pg_dump from different version or location. Default is pg_dump.");
            WriteSetting("SchemaDumpFile", "[file] - Creates SQL schema DDL dump file relative to current dir or ignores if NULL. Default is NULL.");

            WriteLine(ConsoleColor.Yellow, "",
                "Settings can be set in JSON settings files (first appsettings.Development.json, second appsettings.json) under section \"PgRoutiner\", or trough command line. Command line settings will take precedence over settings in JSON files.");
        }
    }
}
