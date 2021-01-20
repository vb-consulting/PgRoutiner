using System;
using System.IO;
using System.Xml;
using Npgsql;

namespace PgRoutiner
{
    public class Project
    {
        public string NormVersion = null;
        public bool AsyncLinqIncluded = false;
        public bool NpgsqlIncluded = false;
    }

    static partial class Program
    {
        private static Project ParseProjectFile()
        {
            string projectFile = null;
            if (!string.IsNullOrEmpty(Settings.Value.Project))
            {
                projectFile = Path.Combine(CurrentDir, Settings.Value.Project);
                if (!File.Exists(projectFile))
                {
                    DumpError($"Couldn't find a project to run. Ensure that a {Path.GetFullPath(projectFile)} project exists, or pass the path to the project in a first argument (pgroutiner path)");
                    return null;
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
                    DumpError($"Couldn't find a project to run. Ensure a project exists in {Path.GetFullPath(CurrentDir)}, or pass the path to the project in a first argument (pgroutiner path)");
                    return null;
                }
            }
            WriteLine("", "Using project file: ");
            WriteLine(ConsoleColor.Cyan, " " + Path.GetFileName(projectFile));

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
                Settings.Value.Namespace = ns;
            }

            return result;
        }
    }
}
