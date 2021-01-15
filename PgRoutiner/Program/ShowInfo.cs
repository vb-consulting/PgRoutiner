﻿using System;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowInfo()
        {
            WriteLine("");
            Console.Write("Usage: ");

            WriteLine(ConsoleColor.Yellow, "PgRoutiner [dir] [-h,--help] [run] [settings]");
            WriteSetting("dir", "set working directory for your project");
            WriteSetting("-h,--help", "show help");
            WriteSetting("run", "start source code generation");

            WriteLine("", "Settings:");
            WriteSetting("connection", "[name] - Connection string name from your configuration connection string to be used. Default first available connection string.");
            WriteSetting("outputDir", "[path] - Relative path where generated source files will be saved. Default is current dir.");
            WriteSetting("modelDir", "[path] -  Relative path where model classes source files will be saved. Default value saves model classes in the same file as a related data-access code.");
            WriteSetting("schema", "[name] - PostgreSQL schema name used to search for routines. Default is public");
            WriteSetting("overwrite", "[true|false] - should existing generated source file be overwritten (true, default) or skipped if they exist (false)");
            WriteSetting("askOverwrite", "[true|false] - prompt a question should overwrite a file (Y/N). Only if overwrite is set true.");
            WriteSetting("namespace", "[name] - Root namespace for generated source files. Default is project root namespace. ");
            WriteSetting("notSimilarTo", "[expressions] - `NOT SIMILAR TO` SQL regular expression used to search routine names. Default skips this matching.");
            WriteSetting("similarTo", "[expressions] - `SIMILAR TO` SQL regular expression used to search routine names. Default skips this matching.");
            WriteSetting("sourceHeader", "[string] - Insert the following content to the start of each generated source code file. Default is \"// [auto-generated at timestamp /]\")");
            WriteSetting("syncMethod", "[true|false] - Generate a `sync` method, true or false. Default is true.");
            WriteSetting("asyncMethod", "[true|false] - Generate a `async` method, true or false. Default is true.");
            WriteSetting("ident", "[number] - Number of default indentation spaces for the generated code. Default is 4.");
            WriteSetting("mapping:key=value", "Key-values to override default type mapping. Key is PostgreSQL UDT type name and value is the corresponding C# type name.");
            WriteSetting("customModels:key=value", "Key-value dictionary to set custom models name. Key is expected model name and value is the new model name. Default is none (empty).");
            WriteSetting("skipIfExists:key=value", "Key-values list of file names (without dir) that will be skipped if they already exist.");


            WriteLine(ConsoleColor.Yellow, "", 
                "INFO: Settings can be set in json settings file (appsettings.json or appsettings.Development.json) under section \"PgRoutiner\", or trough command line.",
                "Command line will take precedence over settings json file.",
                "");
        }
    }
}
