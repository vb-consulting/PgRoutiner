using System;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace PgRoutiner
{
    class Program
    {
        public static readonly string CurrentDir = Directory.GetCurrentDirectory();

        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(CurrentDir, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Join(CurrentDir, "appsettings.Development.json"), optional: true, reloadOnChange: false);

            var config = configBuilder.Build();
            config.GetSection("pgr").Bind(Settings.Value);
            
            var cmdLineConfigBuilder = new ConfigurationBuilder().AddCommandLine(args);
            var cmdLine = cmdLineConfigBuilder.Build();
            cmdLine.Bind(Settings.Value);

            ShowInfo();

            bool success = false;
            success = CheckConnectionValue(config);
            success = FindProjFile();
            success = ParseProjectFile();
            ShowSettings();

            if (!success)
            {
                return;
            }

            //
        }

        private static bool ParseProjectFile()
        {
            var ns = Path.GetFileNameWithoutExtension(Settings.Value.Project);
            string normVersion = null;

            using (var fileStream = File.OpenText(Settings.Value.Project))
            {
                using var reader = XmlReader.Create(fileStream, new XmlReaderSettings{IgnoreComments = true, IgnoreWhitespace = true});
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "RootNamespace")
                    {
                        if (reader.Read())
                        {
                            ns = reader.Value;
                        }
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "PackageReference" && reader.GetAttribute("Include") == "Norm.net")
                    {
                        normVersion = reader.GetAttribute("Version");
                    }
                }
            }

            Settings.Value.Namespace = ns;
            if (string.IsNullOrEmpty(normVersion))
            {
                DumpError($"Norm.net is not referenced in your project. Reference Norm.net, minimum version 1.5.1. first to use this tool.");
                return false;
            }

            try
            {
                var parts = normVersion.Split('.');
                if (Convert.ToInt32(parts[0]) < 1)
                {
                    throw new Exception();
                }
                if (Convert.ToInt32(parts[1]) < 5)
                {
                    throw new Exception();
                }
                if (Convert.ToInt32(parts[1]) == 5 && Convert.ToInt32(parts[2]) < 1)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                DumpError($"Minimum version for Norm.net is 1.5.1. Please, update your reference.");
                return false; 
            }

            return true;
        }

        private static bool CheckConnectionValue(IConfigurationRoot config)
        {
            if (!string.IsNullOrEmpty(Settings.Value.Connection))
            {
                if (!string.IsNullOrEmpty(config.GetConnectionString(Settings.Value.Connection)))
                {
                    return true;
                }
                DumpError($"Connection name {Settings.Value.Connection} could not be found in settings, exiting...");
                return false;
            }
            
            DumpError($"Connection setting is not set, exiting...");
            return false;

        }

        private static bool FindProjFile()
        {
            if (!string.IsNullOrEmpty(Settings.Value.Project))
            {
                Settings.Value.Project = Path.Combine(CurrentDir, Settings.Value.Project);
                if (File.Exists(Settings.Value.Project))
                {
                    return true;
                }
                DumpError($"Project file {Settings.Value.Project} does not exists, exiting...");
                return false;
            }
  
            foreach (var file in Directory.EnumerateFiles(CurrentDir))
            {
                if (Path.GetExtension(file)?.ToLower() != ".csproj")
                {
                    continue;
                }

                Settings.Value.Project = file;
                return true;
            }

            DumpError($"Couldn't find a project to use. Ensure a project exists in {CurrentDir}, or pass the path to the project using project setting.");
            return false;
        }

        private static void DumpError(string msg)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {msg}");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ShowInfo()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("PgRoutiner dotnet tool");
            Console.WriteLine("Scaffold your PostgreSQL routines to enable static type checking for your project!");

            Console.ResetColor();
            Console.WriteLine();
            Console.Write("Usage: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("dotnet pgr [settings]");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Settings:");
            WriteSetting("connection", "<connection string name>");
            WriteSetting("project",
                "<csproj project file name relative to execution dir> - default will search in current dir");
            WriteSetting("schema", "<PostgreSQL schema name> - default is public");
            WriteSetting("skipExisting",
                "<true|false> - should existing generated source file be skipped (true) or overwritten (false, default)");
            WriteSetting("namespace", "<name of the namespace to be used> - default is from project file, use this to override");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "INFO: Settings can be set in json settings file (under section \"pgr\") or trough command line. Command line will override json settings file.");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ShowSettings()
        {
            Console.WriteLine();
            Console.WriteLine("Current settings:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(JsonConvert.SerializeObject(Settings.Value, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            }));
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void WriteSetting(string key, string value)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {key}");
            Console.ResetColor();
            Console.WriteLine($"={value}");
        }
    }
}
