using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace PgRoutiner
{
    partial class Program
    {
        public static readonly string CurrentDir = Directory.GetCurrentDirectory();
#if DEBUG
        public static bool IsDebug { get; set; } = true;
#else
        public static bool IsDebug { get; set; } = false;
#endif

        static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("PgRoutiner dotnet tool");
            Console.WriteLine("Scaffold your PostgreSQL routines to enable static type checking for your project!");
            Console.WriteLine("Use -h or --help for help with settings and options...");
            Console.ResetColor();
            Console.WriteLine();

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(CurrentDir, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Join(CurrentDir, "appsettings.Development.json"), optional: true, reloadOnChange: false);

            var config = configBuilder.Build();
            config.GetSection("PgRoutiner").Bind(Settings.Value);
            
            var cmdLineConfigBuilder = new ConfigurationBuilder().AddCommandLine(args);
            var cmdLine = cmdLineConfigBuilder.Build();
            cmdLine.Bind(Settings.Value);

            if (Settings.Value.SimilarTo == "")
            {
                Settings.Value.SimilarTo = null;
            }
            if (Settings.Value.NotSimilarTo == "")
            {
                Settings.Value.NotSimilarTo = null;
            }
            if (Settings.Value.ModelDir == "")
            {
                Settings.Value.ModelDir = null;
            }


            var help = false;
            if (ArgsInclude(args, "-h") || ArgsInclude(args, "--help"))
            {
                help = true;
                ShowInfo();
            }


            bool success = false;
            success = CheckConnectionValue(config);
            success = FindProjFile();
            success = success && ParseProjectFile();
            ShowSettings();

            if (!success)
            {
                return;
            }

            Settings.MergeTypes(Settings.Value);
            Console.WriteLine("Type mappings:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(JsonConvert.SerializeObject(Settings.TypeMapping));
            Console.ResetColor();

            if (help)
            {
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Running files generation... ");
            Console.ResetColor();
            Console.WriteLine();

            Run(config);
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

            if (!config.GetSection("ConnectionStrings").GetChildren().Any())
            {
                DumpError($"Connection setting is not set and ConnectionStrings section doesn't contain any values, exiting...");
                return false;
            }

            Settings.Value.Connection = config.GetSection("ConnectionStrings").GetChildren().First().Key;
            return true;

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
            Console.Write("Usage: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("dotnet pgr [settings] [run]");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Settings:");
            WriteSetting("connection", "<connection string name> - default will use first available connection string");
            WriteSetting("project",
                "<csproj project file name> - .csproj relative to current dir, default will search for first occurrence in current dir");
            WriteSetting("schema", "<PostgreSQL schema name> - default is public");
            WriteSetting("overwrite",
                "<true|false> - should existing generated source file be overwritten (true, default) or skipped (false)");
            WriteSetting("namespace", "<name of the namespace to be used> - default is from project file, use this to override");
            WriteSetting("notSimilarTo", "<NOT SIMILAR TO SQL regular expressions> - not similar to routine sql regular expression (default is null, skip matching)");
            WriteSetting("similarTo", "<SIMILAR TO SQL regular expressions> - similar to routine sql regular expression (default is null, skip matching)");
            WriteSetting("sourceHeader", "<string comment to insert at first line> - default is \"// <auto-generated />\")");

            WriteSetting("syncMethod", "<true|false) - generate sync method (default is true)");
            WriteSetting("asyncMethod", "<true|false) - generate async method (default is true)");

            WriteSetting("mapping", "<key (PostgreSQL udt type name) - value (c# type name)> - Use this to override default type mapping od add new.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "INFO: Settings can be set in json settings file (appsettings.json or appsettings.Development.json) under section \"PgRoutiner\", or trough command line.");
            Console.WriteLine(
                "Command line will take precedence over settings json file.");
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
        private static bool ArgsInclude(string[] args, params string[] values)
        {
            var lower = values.Select(v => v.ToLower()).ToList();
            var upper = values.Select(v => v.ToUpper()).ToList();
            foreach (var arg in args)
            {
                if (lower.Contains(arg))
                {
                    return true;
                }
                if (upper.Contains(arg))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
