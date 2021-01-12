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
                DumpError($"Npgsql package needs to be referenced to to use this tool.");
                return false;
            }

            if (Settings.Value.AsyncMethod && asyncLinqIncluded == false)
            {
                DumpError($"To generate async methods System.Linq.Async library is required. Include System.Linq.Async package or set asyncMethod option to false.");
                return false;
            }

            if (string.IsNullOrEmpty(Settings.Value.Namespace))
            {
                Settings.Value.Namespace = ns;
            }

            if (string.IsNullOrEmpty(normVersion))
            {
                DumpError($"Norm.net is not referenced in your project. Reference Norm.net, minimum version 1.7 first to use this tool.");
                return false;
            }

            try
            {
                var version = Convert.ToInt32(normVersion.Replace(".", ""));
                if (version < 310)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                DumpError($"Minimum version for Norm.net is 3.1.2 Please, update your reference.");
                return false;
            }

            return true;
        }
    }
}
