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
        public static IConfigurationRoot ParseSettings(string[] args)
        {
            var settingsFile = Path.Join(Program.CurrentDir, "appsettings.json");
            var devSettingsFile = Path.Join(Program.CurrentDir, "appsettings.Development.json");

            var files = new List<string>();
            if (File.Exists(devSettingsFile))
            {
                files.Add(" " + Path.GetFileName(devSettingsFile));
            }
            if (File.Exists(settingsFile))
            {
                files.Add(" " + Path.GetFileName(settingsFile));
            }
            if (files.Count() > 0)
            {
                Program.WriteLine("", "Using config files: ");
                Program.WriteLine(ConsoleColor.Cyan, files.ToArray());
            }

            var configBuilder = new ConfigurationBuilder()
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

            if (Value.Mapping != null)
            {
                foreach (var (key, value) in Value.Mapping)
                {
                    DefaultTypeMapping[key] = value;
                }
                Value.Mapping = DefaultTypeMapping;
            }
            else
            {
                Value.Mapping = DefaultTypeMapping; // new Dictionary<string, string>();
            }

            if (!string.IsNullOrEmpty(Value.Connection))
            {
                if (!string.IsNullOrEmpty(config.GetConnectionString(Value.Connection)))
                {
                    return config;
                }
                Program.DumpError($"Connection name {Value.Connection} could not be found in settings, exiting...");
                return null;
            }

            if (!config.GetSection("ConnectionStrings").GetChildren().Any())
            {
                Program.DumpError($"Connection setting is not set and ConnectionStrings section doesn't contain any values, exiting...");
                return null;
            }

            Value.Connection = config.GetSection("ConnectionStrings").GetChildren().First().Key;
            return config;
        }
    }
}
