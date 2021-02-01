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
            var pgroutinerFile = Path.Join(Program.CurrentDir, pgroutinerSettingsFile);
            var exists = File.Exists(Path.Join(pgroutinerFile));

            if (!exists &&
                Value.Execute == null &&
                !Value.Psql &&
                !Value.Routines && 
                !Value.UnitTests && 
                !Value.SchemaDump && 
                !Value.DataDump && 
                !Value.DbObjects && 
                !Value.Markdown)
            {
                Program.WriteLine(ConsoleColor.Yellow, "",
                    "It seems that you are not using any commands or switches and you don't have a custom settings file created.",
                    $"You can use custom settings file to set switches, commands and settings instead of command line.", "");
                var answer = Program.Ask($"Create a custom \"{pgroutinerSettingsFile}\" settings file with all available default values? [Y/N]", ConsoleKey.Y, ConsoleKey.N);
                if (answer == ConsoleKey.Y)
                {
                    BuildSettingsFile(pgroutinerFile, connection);
                }
            }

            var count = connection.GetRoutineCount(Value);
            Project project;
            if ((Value.Routines && Value.OutputDir != null && count > 0) || (Value.UnitTests && Value.UnitTestsDir != null))
            {
                project = ParseProjectFile();
                if (project == null)
                {
                    return false;
                }
                UpdateProjectReferences(project);
            }
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

            if (string.IsNullOrEmpty(Value.Namespace))
            {
                Value.Namespace = ns;
            }

            return result;
        }

        private static void BuildSettingsFile(string file, NpgsqlConnection connection)
        {
            try 
            {
                File.WriteAllText(file, BuildFormatedSettings(connection: connection));
                Program.WriteLine("");
                Program.Write(ConsoleColor.Yellow, $"Settings file ");
                Program.Write(ConsoleColor.Cyan, pgroutinerSettingsFile);
                Program.WriteLine(ConsoleColor.Yellow, $" successfully created!", "");

                Program.Write(ConsoleColor.Yellow, "- Run ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {SettingsArgs.Alias}");
                Program.Write(ConsoleColor.Yellow, " or ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {SettingsArgs.Name}");
                Program.WriteLine(ConsoleColor.Yellow, " to see current settings and switches.");

                Program.Write(ConsoleColor.Yellow, "- Run ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {HelpArgs.Alias}");
                Program.Write(ConsoleColor.Yellow, " or ");
                Program.Write(ConsoleColor.Cyan, $"pgroutiner {HelpArgs.Name}");
                Program.WriteLine(ConsoleColor.Yellow, " to see help on available commands.", "");
            }
            catch (Exception e)
            {
                Program.DumpError($"File {pgroutinerSettingsFile} could not be written: {e.Message}");
            }
        }
    }
}
