using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public static bool ParseInitialSettings(string connectionStr)
        {
            using var connection = new NpgsqlConnection(connectionStr);
            var count = connection.GetRoutineCount(Value);
            Project project = null;
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
                Value.DbObjectsDir != null)
            {
                return true;
            }

            if (count > 0)
            {
                Console.WriteLine();
                Console.WriteLine();
                Value.OutputDir = Program.ReadLine("Output directory: ",
                    $"Found {count} routines in connection {Value.Connection}",
                    "Would you like to generate data access extensions code for those routines?", 
                    " - Type the name of the output directory (relative to he current) and press enter.",
                    " - Use dot for current directory",
                    " - Leave the name empty and press enter to skip this.");
            }

            Console.WriteLine();
            Value.SchemaDumpFile = Program.ReadLine("Schema dump file: ",
                $"Would you like to create a schema dump file for the connection {Value.Connection}?",
                " - Type the name of the schema dump file path (relative to he current path) and press enter.",
                " - Leave the name empty and press enter to skip this.");


            Console.WriteLine();
            Value.DataDumpFile = Program.ReadLine("Data dump file: ",
                $"Would you like to create a data dump file for the connection {Value.Connection}?",
                " - Type the name of the data dump file path (relative to he current path) and press enter.",
                " - Leave the name empty and press enter to skip this.");
            
            Console.WriteLine();
            Value.DataDumpFile = Program.ReadLine("Database objects tree directory: ",
            $"Would you like to create an database objects tree directory for the {Value.Connection}?",
            " - Type the name of the objects tree directory (relative to he current) and press enter.",
            " - Use dot for current directory",
            " - Leave the name empty and press enter to skip this.");

            Console.WriteLine();
            var answer = Program.Ask($"Settings file {pgroutinerSettingsFile} will be created for you!{Environment.NewLine}{Environment.NewLine}" +
                $"Run code files generation based on your selection immediately?{Environment.NewLine}" +
                $"You can always use \"--run\" switch later to run  files generation manually. {Environment.NewLine}{Environment.NewLine}" + 
                "Yes or No? [Y/N]",
                ConsoleKey.Y, ConsoleKey.N);

            if (answer == ConsoleKey.Y)
            {
                Value.Run = true;
            }

            BuildSettingsFile(pgroutinerFile);
            return true;
        }

        private static void UpdateProjectReferences(Project project)
        {
            if (project.NpgsqlIncluded == false)
            {
                if (Value.UpdateReferences)
                {
                    Program.DumpError($"Npgsql package package is required.");
                    if (Program.Ask("Add Npgsql reference? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        Program.RunProcess("dotnet", "add package Npgsql");
                    }
                }
            }

            if (Value.AsyncMethod && project.AsyncLinqIncluded == false)
            {
                if (Value.UpdateReferences)
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
                if (Value.UpdateReferences)
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
                if (Value.UpdateReferences)
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
            StringBuilder sb = new();
            void AddComment(string comment)
            {
                sb.AppendLine($"    /* {comment} */");
            }
            void AddEntry(string field, object fieldValue, string last = ",")
            {
                string v = null;
                if (fieldValue == null)
                {
                    v = "null";
                }
                else if (fieldValue is string)
                {
                    v = $"\"{fieldValue}\"";
                }
                else if (fieldValue is bool)
                {
                    v = (bool)fieldValue == true ? "true" : "false";
                }
                else if (fieldValue is List<string>)
                {
                    if (((List<string>)fieldValue).Count == 0)
                    {
                        v = "[ ]";
                    }
                    else
                    {
                        v = $"[{Environment.NewLine}      {string.Join($",{Environment.NewLine}      ", ((List<string>)fieldValue).Select(i => $"\"{i}\""))}{Environment.NewLine}    ]";
                    }
                }
                else if (fieldValue is int)
                {
                    v = $"{fieldValue}";
                }
                else if (fieldValue is Dictionary<string, string>)
                {
                    if (((Dictionary<string, string>)fieldValue).Values.Count == 0)
                    {
                        v = "{ }";
                    }
                    else
                    {
                        v = $"{{{Environment.NewLine}      {string.Join($",{Environment.NewLine}      ", ((Dictionary<string, string>)fieldValue).Select(d => $"\"{d.Key}\": \"{d.Value}\""))}{Environment.NewLine}    }}";
                    }
                }
                sb.AppendLine($"    \"{field}\": {v}{last}");
                
            }

            sb.AppendLine("{");
            sb.AppendLine("  /* PgRoutiner settings */");
            sb.AppendLine("  /* see https://github.com/vb-consulting/PgRoutiner/SETTINGS.MD for more info */");
            sb.AppendLine("  \"PgRoutiner\": {");

            sb.AppendLine();
            AddComment("general settings");
            AddEntry("Connection", Value.Connection);
            AddEntry("Schema", Value.Schema);
            AddEntry("Overwrite", Value.Overwrite);
            AddEntry("AskOverwrite", Value.AskOverwrite);
            AddEntry("SkipIfExists", Value.SkipIfExists);
            AddEntry("MinNormVersion", Value.MinNormVersion);
            AddEntry("UpdateReferences", Value.UpdateReferences);
            AddEntry("Ident", Value.Ident);
            AddEntry("PgDumpCommand", Value.PgDumpCommand);

            sb.AppendLine();
            AddComment("routines data-access extensions settings");
            AddEntry("OutputDir", Value.OutputDir);
            AddEntry("Namespace", Value.Namespace);
            AddEntry("NotSimilarTo", Value.NotSimilarTo);
            AddEntry("SimilarTo", Value.SimilarTo);
            AddEntry("SourceHeader", Value.SourceHeader);
            AddEntry("SyncMethod", Value.SyncMethod);
            AddEntry("AsyncMethod", Value.AsyncMethod);
            AddEntry("ModelDir", Value.ModelDir);
            AddEntry("Mapping", Value.Mapping);
            AddEntry("CustomModels", Value.CustomModels);
            AddEntry("UseRecords", Value.UseRecords);

            sb.AppendLine();
            AddComment("schema dump settings");
            AddEntry("SchemaDumpFile", Value.SchemaDumpFile);
            AddEntry("SchemaDumpNoOwner", Value.SchemaDumpNoOwner);
            AddEntry("SchemaDumpNoPrivileges", Value.SchemaDumpNoPrivileges);
            AddEntry("SchemaDumpDropIfExists", Value.SchemaDumpDropIfExists);
            AddEntry("SchemaDumpAdditionalOptions", Value.SchemaDumpAdditionalOptions);
            AddEntry("SchemaDumpUnderTransaction", Value.SchemaDumpUnderTransaction);

            sb.AppendLine();
            AddComment("data dump settings");
            AddEntry("DataDumpFile", Value.DataDumpFile);
            AddEntry("DataDumpTables", Value.DataDumpTables);
            AddEntry("DataDumpAdditionalOptions", Value.DataDumpAdditionalOptions);
            AddEntry("DataDumpUnderTransaction", Value.DataDumpUnderTransaction);

            sb.AppendLine();
            AddComment("database object tree settings");
            AddEntry("DbObjectsDir", Value.DbObjectsDir);
            AddEntry("DbObjectsDirClearIfNotEmpty", Value.DbObjectsDirClearIfNotEmpty);
            AddEntry("DbObjectsNoOwner", Value.DbObjectsNoOwner);
            AddEntry("DbObjectsNoPrivileges", Value.DbObjectsNoPrivileges);
            AddEntry("DbObjectsDropIfExists", Value.DbObjectsDropIfExists);
            AddEntry("DbObjectsRaw", Value.DbObjectsRaw, "");

            sb.AppendLine("  }");
            sb.AppendLine("}");

            File.WriteAllText(file, sb.ToString());
            Console.WriteLine();
            Program.WriteLine(ConsoleColor.Yellow, $"Settings file {pgroutinerSettingsFile} created.");
            if (!Value.Run)
            {
                Program.WriteLine(ConsoleColor.Yellow, $"Use -r or --run switch to start code generation.");
            }
        }
    }
}
