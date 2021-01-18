using System;
using System.IO;
using System.Xml;

namespace PgRoutiner
{
    static partial class Program
    {
        private static bool ParseProjectFile()
        {
            string projectFile = null;
            if (!string.IsNullOrEmpty(Settings.Value.Project))
            {
                projectFile = Path.Combine(CurrentDir, Settings.Value.Project);
                if (!File.Exists(projectFile))
                {
                    DumpError($"Project file {projectFile} does not exists, exiting...");
                    return false;
                }
                CurrentDir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(projectFile)));
            }
            else
            {
                foreach (var file in Directory.EnumerateFiles(CurrentDir))
                {
                    if (Path.GetExtension(file)?.ToLower() == ".csproj")
                    {
                        projectFile = file;
                        break;
                    }
                }
                if (projectFile == null)
                {
                    DumpError($"No .csproj or file found in current dir. You can use setting to pass path to .csproj file.");
                    return false;
                }
            }
            WriteLine("", "Using project file: ");
            WriteLine(ConsoleColor.Cyan, " " + Path.GetFileName(projectFile));

            var ns = Path.GetFileNameWithoutExtension(projectFile);
            string normVersion = null;
            bool asyncLinqIncluded = false;
            bool npgsqlIncluded = false;
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
                            normVersion = reader.GetAttribute("Version");
                        }
                        if (reader.GetAttribute("Include") == "System.Linq.Async")
                        {
                            asyncLinqIncluded = true;
                        }
                        if (reader.GetAttribute("Include") == "Npgsql")
                        {
                            npgsqlIncluded = true;
                        }
                    }
                }
            }

            if (npgsqlIncluded == false)
            {
                if (Settings.Value.UpdateReferences)
                {
                    DumpError($"Npgsql package package is required.");
                    if (Ask("Add Npgsql reference? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        RunProcess("dotnet", "add package Npgsql");
                    }
                }
            }

            if (Settings.Value.AsyncMethod && asyncLinqIncluded == false)
            {
                if (Settings.Value.UpdateReferences)
                {
                    DumpError($"To be able to use async methods, System.Linq.Async package is required.");
                    if (Ask("Add System.Linq.Async package reference? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        RunProcess("dotnet", "add package System.Linq.Async");
                    }
                }
            }

            if (string.IsNullOrEmpty(Settings.Value.Namespace))
            {
                Settings.Value.Namespace = ns;
            }

            if (string.IsNullOrEmpty(normVersion))
            {
                if (Settings.Value.UpdateReferences)
                {
                    DumpError($"Norm.net package package is required.");
                    if (Ask("Add Norm.net reference? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        RunProcess("dotnet", "add package Norm.net");
                    }
                }
            }

            var minNormVersion = Convert.ToInt32(Settings.Value.MinNormVersion.Replace(".", ""));
            try
            {
                var version = Convert.ToInt32(normVersion.Replace(".", ""));
                if (version < minNormVersion)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                if (Settings.Value.UpdateReferences)
                {
                    DumpError($"Minimum version for Norm.net package is 3.1.2. Current version in project is {normVersion}.");
                    if (Ask("Update Norm.net package? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.Y)
                    {
                        RunProcess("dotnet", "add package Norm.net");
                    }
                }
            }

            return true;
        }
    }
}
