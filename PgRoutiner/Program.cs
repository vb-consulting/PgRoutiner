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
        public static readonly string Version = "1.1.0";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("PgRoutiner dotnet tool");
            Console.WriteLine("Scaffold your PostgreSQL routines to enable static type checking for your project!");
            Console.WriteLine("Use -h or --help for help with settings and options...");
            Console.ResetColor();
            Console.WriteLine();

            var currentDir = CurrentDir;
            foreach (var arg in args)
            {
                if (arg.ToLower().StartsWith("project") && arg.Contains("="))
                {
                    currentDir = Path.Join(currentDir, arg.Split('=').Last());
                }
            }

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(currentDir, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Join(currentDir, "appsettings.Development.json"), optional: true, reloadOnChange: false);

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
            success = success && FindProjFile();
            success = success && ParseProjectFile();

            Settings.MergeTypes(Settings.Value);
            Settings.Value.Mapping = Settings.TypeMapping;

            ShowSettings();

            if (!success)
            {
                return;
            }

            if (help)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Running files generation... ");
            Console.ResetColor();

            Run(config);
        }

        private static bool ParseProjectFile()
        {
            var ns = Path.GetFileNameWithoutExtension(Settings.Value.Project);
            string normVersion = null;
            bool asyncLinqIncluded = false;
            bool npgsqlIncluded = false;
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

            DumpError($"No .csproj or file found in current dir. You can use setting to pass path to .csproj file.");
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
            Console.WriteLine("Version {0}", Version);
            Console.ResetColor();
            Console.WriteLine();

            Console.Write("Usage: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("PgRoutiner [settings]");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Settings:");

            WriteSetting("connection", "<name> - Connection string name from your configuration connection string to be used. Default first available connection string.");
            WriteSetting("project", "<name> - Relative path to project `.csproj` file. Default is first available `.csproj` file from the current dir.");
            WriteSetting("outputDir", "<path> - Relative path where generated source files will be saved. Default is current dir.");
            WriteSetting("modelDir", "<path> -  Relative path where model classes source files will be saved. Default value saves model classes in the same file as a related data-access code.");
            WriteSetting("schema", "<name> - PostgreSQL schema name used to search for routines. Default is public");
            WriteSetting("overwrite", "<true|false> - should existing generated source file be overwritten (true, default) or skipped if they exist (false)");
            WriteSetting("namespace", "<name> - Root namespace for generated source files. Default is project root namespace. ");
            WriteSetting("notSimilarTo", "<expressions> - `NOT SIMILAR TO` SQL regular expression used to search routine names. Default skips this matching.");
            WriteSetting("similarTo", "<expressions> - `SIMILAR TO` SQL regular expression used to search routine names. Default skips this matching.");
            WriteSetting("sourceHeader", "<string> - Insert the following content to the start of each generated source code file. Default is \"// <auto-generated at timestamp />\")");
            WriteSetting("syncMethod", "<true|false> - Generate a `sync` method, true or false. Default is true.");
            WriteSetting("asyncMethod", "<true|false> - Generate a `async` method, true or false. Default is true.");
            WriteSetting("mapping:key=value", "Key-values to override default type mapping. Key is PostgreSQL UDT type name and value is the corresponding C# type name.");
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
                Formatting = Formatting.None
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
