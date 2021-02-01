using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;
using System.Xml;

namespace PgRoutiner
{
    public class Project
    {
        public string NormVersion = null;
        public bool AsyncLinqIncluded = false;
        public bool NpgsqlIncluded = false;
    }

    public partial class Settings
    {
        public static bool ParseInitialSettings(NpgsqlConnection connection)
        {
            var count = connection.GetRoutineCount(Value);
            Project project;
            if (Value.OutputDir != null && count > 0)
            {
                project = ParseProjectFile();
                if (project == null)
                {
                    return false;
                }
                UpdateProjectReferences(project);
            }

            var pgroutinerFile = Path.Join(Program.CurrentDir, pgroutinerSettingsFile);
            var exists = File.Exists(Path.Join(pgroutinerFile));
            if (exists ||
                Value.OutputDir != null || 
                Value.SchemaDumpFile != null || 
                Value.DataDumpFile != null || 
                Value.DbObjectsDir != null ||
                Value.CommentsMdFile != null)
            {
                return true;
            }

            if (count > 0)
            {
                Console.WriteLine();
                Console.WriteLine();
                Value.OutputDir = Program.ReadLine("Data access code output directory: ",
                    $"Found {count} routines in connection {Value.Connection}",
                    "Would you like to generate data access extensions code for those routines?", 
                    " - Type the name of the output directory, relative to the current path, and press enter.",
                    " - For example \"DataAcess\" or \"Functions\".",
                    " - Use dot (\".\" or \"./\") to use the current directory.",
                    " - Leave the name empty and press enter to skip this.");
            }

            Console.WriteLine();
            Value.SchemaDumpFile = Program.ReadLine("Schema dump file: ",
                $"Would you like to create a schema dump file for the connection {Value.Connection}?",
                " - Type the name of the schema dump file, relative to he current path and press enter.",
                " - For example \"Scripts/schema.sql\".",
                " - Leave the name empty and press enter to skip this.");


            Console.WriteLine();
            Value.DataDumpFile = Program.ReadLine("Data dump file: ",
                $"Would you like to create a data dump file for the connection {Value.Connection}?",
                " - Type the name of the data dump file path, relative to he current path, and press enter.",
                " - For example \"Scripts/data.sql\".",
                " - Leave the name empty and press enter to skip this.");

            if (Value.DataDumpFile != null)
            {
                Console.WriteLine();
                var tables = Program.ReadLine("Data dump tables: ",
                $"Please type the table names for your data dump file \"{Value.DataDumpFile}\", separated by comma.",
                " - For example \"table1, table2, table3\".",
                " - Leave this value empty and press enter to use all available tables in your database.");
                if (tables != null)
                {
                    try
                    {
                        Value.DataDumpTables = new List<string>(tables.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()));
                    }
                    catch
                    {
                        Value.DataDumpTables = new List<string>();
                    }
                }
            }

            Console.WriteLine();
            Value.DbObjectsDir = Program.ReadLine("Database objects tree directory: ",
            $"Would you like to create a database objects tree directory for the {Value.Connection}?",
            " - Type the name of the objects tree directory, relative to he current path, and press enter.",
            " - For example \"Database\" or \"Scripts\".",
            " - Use dot (\".\" or \"./\") to use the current directory.",
            " - Leave the name empty and press enter to skip this.");

            if (Value.SchemaDumpFile != null || Value.DataDumpFile != null || Value.DbObjectsDir != null)
            {
                var builder = new PgDumpBuilder(Value, connection);
                if (!CheckPgDumpVersion(connection, builder))
                {
                    Value.SchemaDumpFile = null;
                    Value.DataDumpFile = null;
                    Value.DbObjectsDir = null;
                }
            }

            Console.WriteLine();
            Value.CommentsMdFile = Program.ReadLine("Comments markdown (MD) file: ",
            $"Would you like to markdown documentation file from objects settings for the {Value.Connection}?",
                " - Type the name of the markdown file path, relative to he current path, and press enter.",
                " - For example \"database-dictionary.md\" or \"../db-dictionary.md\".",
                " - Leave the name empty and press enter to skip this.");

            Console.WriteLine();
            if (Value.OutputDir == null &&
                Value.SchemaDumpFile == null &&
                Value.DataDumpFile == null &&
                Value.DbObjectsDir == null &&
                Value.CommentsMdFile == null)
            {
                Value.Run = false;
            }
            else
            {
                Program.Write(ConsoleColor.Yellow, "Settings file ");
                Program.Write(ConsoleColor.Cyan, pgroutinerSettingsFile);
                Program.WriteLine(ConsoleColor.Yellow, " will be created for you based on your selection:");

                Program.Write(ConsoleColor.Yellow, "- data access code output directory: ");
                Program.WriteLine(ConsoleColor.Cyan, Value.OutputDir ?? "<skip>");

                Program.Write(ConsoleColor.Yellow, "- schema dump file: ");
                Program.WriteLine(ConsoleColor.Cyan, Value.SchemaDumpFile ?? "<skip>");

                Program.Write(ConsoleColor.Yellow, "- data dump file: ");
                if (Value.DataDumpTables.Count == 0)
                {
                    Program.WriteLine(ConsoleColor.Cyan, Value.DataDumpFile ?? "<skip>");
                }
                else
                {
                    Program.WriteLine(ConsoleColor.Cyan, $"{(Value.DataDumpFile ?? "<skip>")} for tables: {string.Join(",", Value.DataDumpTables)}");
                }

                Program.Write(ConsoleColor.Yellow, "- database objects tree directory: ");
                Program.WriteLine(ConsoleColor.Cyan, Value.DbObjectsDir ?? "<skip>");

                Program.Write(ConsoleColor.Yellow, "- comments markdown (MD) file: ");
                Program.WriteLine(ConsoleColor.Cyan, Value.CommentsMdFile ?? "<skip>");

                Program.WriteLine("");
                Program.Write(ConsoleColor.Yellow, "You can change this file to update to new settings and run ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner --{RunArgs.Name}");
                Program.WriteLine(ConsoleColor.Yellow, " command to start file generation.");

                var answer = Program.Ask("Run code files generation immediately based on your current settings. Yes or No? [Y/N]", ConsoleKey.Y, ConsoleKey.N);
                Value.Run = answer == ConsoleKey.Y;
            }

            BuildSettingsFile(pgroutinerFile);
            return true;
        }

        private static void UpdateProjectReferences(Project project)
        {
            if (project.NpgsqlIncluded == false)
            {
                if (!Value.SkipUpdateReferences)
                {
                    Program.DumpError($"Npgsql package package is required.");
                    if (Program.Ask("Add Npgsql reference? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        Program.RunProcess("dotnet", "add package Npgsql");
                    }
                }
            }

            if (!Value.SkipAsyncMethods && project.AsyncLinqIncluded == false)
            {
                if (!Value.SkipUpdateReferences)
                {
                    Program.DumpError($"To be able to use async methods, System.Linq.Async package is required.");
                    if (Program.Ask("Add System.Linq.Async package reference? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        Program.RunProcess("dotnet", "add package System.Linq.Async");
                    }
                }
            }

            if (string.IsNullOrEmpty(project.NormVersion))
            {
                if (!Value.SkipUpdateReferences)
                {
                    Program.DumpError($"Norm.net package package is required.");
                    if (Program.Ask("Add Norm.net reference? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        Program.RunProcess("dotnet", "add package Norm.net");
                    }
                }
            }

            var minNormVersion = Convert.ToInt32(Value.MinNormVersion.Replace(".", ""));
            try
            {
                var version = Convert.ToInt32(project.NormVersion.Replace(".", ""));
                if (version < minNormVersion)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                if (!Value.SkipUpdateReferences)
                {
                    Program.DumpError($"Minimum version for Norm.net package is {Settings.Value.MinNormVersion}. Current version in project is {project.NormVersion}.");
                    if (Program.Ask("Update Norm.net package? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        Program.RunProcess("dotnet", "add package Norm.net");
                    }
                }
            }
        }

        private static Project ParseProjectFile()
        {
            string projectFile = null;
            if (!string.IsNullOrEmpty(Value.Project))
            {
                projectFile = Path.Combine(Program.CurrentDir, Path.GetFileName(Value.Project));
                if (!File.Exists(projectFile))
                {
                    Program.DumpError($"Couldn't find a project to run. Ensure that a {Path.GetFullPath(projectFile)} project exists, or pass the path to the project in a first argument (pgroutiner path)");
                    return null;
                }
            }
            else
            {
                foreach (var file in Directory.EnumerateFiles(Program.CurrentDir))
                {
                    if (Path.GetExtension(file)?.ToLower() == ".csproj")
                    {
                        projectFile = file;
                        break;
                    }
                }
                if (projectFile == null)
                {
                    Program.DumpError($"Couldn't find a project to run. Ensure a project exists in {Path.GetFullPath(Program.CurrentDir)}, or pass the path to the project in a first argument (pgroutiner path)");
                    return null;
                }
            }
            Program.WriteLine("", "Using project file: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + Path.GetFileName(projectFile));

            var ns = Path.GetFileNameWithoutExtension(projectFile);

            Project result = new Project();

            using (var fileStream = File.OpenText(projectFile))
            {
                using var reader = XmlReader.Create(fileStream, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "RootNamespace")
                    {
                        if (reader.Read())
                        {
                            ns = reader.Value;
                        }
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "PackageReference")
                    {
                        if (reader.GetAttribute("Include") == "Norm.net")
                        {
                            result.NormVersion = reader.GetAttribute("Version");
                        }
                        if (reader.GetAttribute("Include") == "System.Linq.Async")
                        {
                            result.AsyncLinqIncluded = true;
                        }
                        if (reader.GetAttribute("Include") == "Npgsql")
                        {
                            result.NpgsqlIncluded = true;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(Settings.Value.Namespace))
            {
                Value.Namespace = ns;
            }

            return result;
        }

        private static void BuildSettingsFile(string file)
        {
            File.WriteAllText(file, BuildFormatedSettings());
            Console.WriteLine();
            Program.Write(ConsoleColor.Yellow, $"Settings file ");
            Program.Write(ConsoleColor.Cyan, pgroutinerSettingsFile);
            Program.WriteLine(ConsoleColor.Yellow, $" successfully created!");
            if (!Value.Run)
            {
                Program.Write(ConsoleColor.Yellow, "- Change this file and run ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {RunArgs.Alias}");
                Program.Write(ConsoleColor.Yellow, " or ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {RunArgs.Name}");
                Program.WriteLine(ConsoleColor.Yellow, " command to start file generation.");

                Program.Write(ConsoleColor.Yellow, "- Run ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {SettingsArgs.Alias}");
                Program.Write(ConsoleColor.Yellow, " or ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {SettingsArgs.Name}");
                Program.WriteLine(ConsoleColor.Yellow, " to see current settings.");

                Program.Write(ConsoleColor.Yellow, "- Run ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {HelpArgs.Alias}");
                Program.Write(ConsoleColor.Yellow, " or ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {HelpArgs.Name}");
                Program.WriteLine(ConsoleColor.Yellow, " to see help on available commands.");
            }
        }
    }
}
