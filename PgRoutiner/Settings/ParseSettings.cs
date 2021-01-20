using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner
{
    public partial class Settings
    {
        private const string pgroutinerSettingsFile = "appsettings.PgRoutiner.json";

        public static IConfigurationRoot ParseSettings(string[] args, out string error)
        {
            var pgroutinerFile = Path.Join(Program.CurrentDir, pgroutinerSettingsFile);
            var settingsFile = Path.Join(Program.CurrentDir, "appsettings.json");
            var devSettingsFile = Path.Join(Program.CurrentDir, "appsettings.Development.json");

            var files = new List<string>();
            if (File.Exists(pgroutinerFile))
            {
                files.Add(" " + Path.GetFileName(pgroutinerFile));
            }
            if (File.Exists(devSettingsFile))
            {
                files.Add(" " + Path.GetFileName(devSettingsFile));
            }
            if (File.Exists(settingsFile))
            {
                files.Add(" " + Path.GetFileName(settingsFile));
            }
            if (files.Count > 0)
            {
                Program.WriteLine("", "Using config files: ");
                Program.WriteLine(ConsoleColor.Cyan, files.ToArray());
            }

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(pgroutinerFile, optional: true, reloadOnChange: false)
                .AddJsonFile(settingsFile, optional: true, reloadOnChange: false)
                .AddJsonFile(devSettingsFile, optional: true, reloadOnChange: false);

            var config = configBuilder.Build();
            config.GetSection("PgRoutiner").Bind(Value);

            var cmdLineConfigBuilder = new ConfigurationBuilder().AddCommandLine(args);
            var cmdLine = cmdLineConfigBuilder.Build();
            cmdLine.Bind(Value);

            if (Value.SimilarTo == "")
            {
                Value.SimilarTo = null;
            }
            if (Value.NotSimilarTo == "")
            {
                Value.NotSimilarTo = null;
            }
            if (Value.ModelDir == "")
            {
                Value.ModelDir = null;
            }
            if (Value.Schema == "")
            {
                Value.Schema = null;
            }

            if (Value.Mapping != null && Value.Mapping.Values.Count > 0)
            {
                foreach (var (key, value) in Value.Mapping)
                {
                    DefaultTypeMapping[key] = value;
                }
                Value.Mapping = DefaultTypeMapping;
            }
            else
            {
                Value.Mapping = DefaultTypeMapping;
            }

            if (!string.IsNullOrEmpty(Value.Connection))
            {
                if (!string.IsNullOrEmpty(config.GetConnectionString(Value.Connection)))
                {
                    error = null;
                    return config;
                }
                error = $"Connection name {Value.Connection} could not be found in settings, exiting...";
                return null;
            }

            if (!config.GetSection("ConnectionStrings").GetChildren().Any())
            {
                error = $"Connection setting is not set and ConnectionStrings section doesn't contain any values, exiting...";
                return null;
            }

            Value.Connection = config.GetSection("ConnectionStrings").GetChildren().First().Key;

            if (Program.ArgsInclude(args, "-r", "--run"))
            {
                Value.Run = true;
            }
            Program.WriteLine("", "Using connection: ");
            Program.WriteLine(ConsoleColor.Cyan, " " + Path.GetFileName(Value.Connection));

            error = null;
            return config;
        }
    }
}
