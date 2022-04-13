﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner.SettingsManagement
{
    public partial class Settings
    {
        public static IConfigurationRoot ParseSettings(string[] args, string pgroutinerSettingsFile)
        {
            var argsList = args.ToList();

            void ParseSwitches(object instance, bool setValue = false)
            {
                foreach(var prop in instance.GetType().GetProperties())
                {
                    if (prop.PropertyType != typeof(bool))
                    {
                        continue;
                    }
                    Arg argNames;
                    var argField = typeof(Settings).GetField($"{prop.Name}Args");
                    if (argField != null)
                    {
                        argNames = (Arg)argField.GetValue(null);
                    }
                    else
                    {
                        argNames = new Arg($"--{prop.Name.ToLower()}", prop.Name);
                    }
                    if (Program.ArgsInclude(args, argNames))
                    {
                        prop.SetValue(instance, true);
                        argsList.Remove(argNames.Alias);
                        argsList.Remove(argNames.Name);
                        if (setValue)
                        {
                            argsList.Add(argNames.Name);
                            argsList.Add("True");
                        }
                    }
                }
            }
            ParseSwitches(Switches.Value);
            ParseSwitches(Value, true);

            var pgroutinerFile = Path.Join(Program.CurrentDir, pgroutinerSettingsFile);
            var pgroutinerFile2 = Path.Join(Program.CurrentDir, "pgroutiner.json");
            var settingsFile = Path.Join(Program.CurrentDir, "appsettings.json");
            var devSettingsFile = Path.Join(Program.CurrentDir, "appsettings.Development.json");

            var files = new List<string>();
            if (File.Exists(pgroutinerFile))
            {
                files.Add(" " + Path.GetFileName(pgroutinerFile));
            }
            if (File.Exists(pgroutinerFile2))
            {
                files.Add(" " + Path.GetFileName(pgroutinerFile2));
            }
            if (File.Exists(devSettingsFile))
            {
                files.Add(" " + Path.GetFileName(devSettingsFile));
            }
            if (File.Exists(settingsFile))
            {
                files.Add(" " + Path.GetFileName(settingsFile));
            }
            IConfigurationRoot config;
            try
            {
                var configBuilder = new ConfigurationBuilder()
                    .AddJsonFile(pgroutinerFile, optional: true, reloadOnChange: false)
                    .AddJsonFile(pgroutinerFile2, optional: true, reloadOnChange: false)
                    .AddJsonFile(settingsFile, optional: true, reloadOnChange: false)
                    .AddJsonFile(devSettingsFile, optional: true, reloadOnChange: false);
                config = configBuilder.Build();
                config.GetSection("PgRoutiner").Bind(Value);
                config.Bind(Value);

                if (!string.IsNullOrEmpty(Value.ConfigPath))
                {
                    Value.ConfigPath = Path.Join(Program.CurrentDir, Value.ConfigPath);
                    
                    var pgroutinerFileConfig = Path.Join(Value.ConfigPath, pgroutinerSettingsFile);
                    if (File.Exists(pgroutinerFileConfig))
                    {
                        files.Add(" " + Path.GetFileName(pgroutinerFileConfig));
                        configBuilder.AddJsonFile(pgroutinerFileConfig, optional: true, reloadOnChange: false);
                    }
                    var pgroutinerFile2Config = Path.Join(Value.ConfigPath, "pgroutiner.json");
                    if (File.Exists(pgroutinerFile2Config))
                    {
                        files.Add(" " + Path.GetFileName(pgroutinerFile2Config));
                        configBuilder.AddJsonFile(pgroutinerFile2Config, optional: true, reloadOnChange: false);
                    }
                    var settingsFileConfig = Path.Join(Value.ConfigPath, "appsettings.json");
                    if (File.Exists(settingsFileConfig))
                    {
                        files.Add(" " + Path.GetFileName(settingsFileConfig));
                        configBuilder.AddJsonFile(settingsFileConfig, optional: true, reloadOnChange: false);
                    }
                    var devSettingsFileConfig = Path.Join(Value.ConfigPath, "appsettings.Development.json");
                    if (File.Exists(devSettingsFileConfig))
                    {
                        files.Add(" " + Path.GetFileName(devSettingsFileConfig));
                        configBuilder.AddJsonFile(devSettingsFileConfig, optional: true, reloadOnChange: false);
                    }

                    configBuilder = new ConfigurationBuilder()
                        .AddJsonFile(pgroutinerFile, optional: true, reloadOnChange: false)
                        .AddJsonFile(settingsFile, optional: true, reloadOnChange: false)
                        .AddJsonFile(devSettingsFile, optional: true, reloadOnChange: false)
                        .AddJsonFile(pgroutinerFileConfig, optional: true, reloadOnChange: false)
                        .AddJsonFile(settingsFileConfig, optional: true, reloadOnChange: false)
                        .AddJsonFile(devSettingsFileConfig, optional: true, reloadOnChange: false);
                    config = configBuilder.Build();
                    config.GetSection("PgRoutiner").Bind(Value);
                    config.Bind(Value);
                }

                Dictionary<string, string> switchMappings = new();
                foreach (var f in typeof(Settings).GetFields())
                {
                    if (f.FieldType != typeof(Arg))
                    {
                        continue;
                    }
                    var arg = (Arg)f.GetValue(null);
                    switchMappings[arg.Alias] = arg.Original;
                }
                new ConfigurationBuilder()
                    .AddCommandLine(argsList.ToArray(), switchMappings)
                    .Build()
                    .Bind(Value);
            }
            catch (Exception e)
            {
                Program.DumpError($"Failed to bind configuration: {e.Message}");
                return null;
            }
            if (files.Count > 0)
            {
                Program.WriteLine("", "Using configuration files: ");
                Program.WriteLine(ConsoleColor.Cyan, files.ToArray());
            }

            foreach (var prop in typeof(Settings).GetProperties())
            {
                if (prop.PropertyType != typeof(string))
                {
                    continue;
                }
                var value = prop.GetValue(Value) as string;
                if (value == "")
                {
                    prop.SetValue(Value, null);
                }
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
#if DEBUG
            Program.ParseProjectSetting(Value);
#endif
            if (Value.Execute != null || Value.Psql || Value.CommitMd)
            {
                Value.DbObjects = false;
                Value.SchemaDump = false;
                Value.DataDump = false;
                Value.Diff = false;
                Value.Routines = false;
                Value.UnitTests = false;
                Value.Markdown = false;
            }

            if (!new string[] { "Single", "SingleOrDefault", "First", "FirstOrDefault" }.Contains(Value.ReturnMethod))
            {
                Program.DumpError($"ReturnMethod setting must be one of the allowed values: Single, SingleOrDefault, First or FirstOrDefault");
                return null;
            }

            return config;
        }

        public static (string settingsFile, bool customSettings) GetSettingsFile(string[] args)
        {
            string result = null;
            var index = Array.IndexOf(args, SettingsArgs.Alias);
            if (index == -1)
            {
                index = Array.IndexOf(args, $"--{SettingsArgs.Original}");
            }
            if (index > -1 && index < args.Length - 1)
            {
                var value = args[index + 1];
                if (!value.StartsWith("-"))
                {
                    result = value;
                }
            }
            if (result == null)
            {
                return ("appsettings.PgRoutiner.json", false);
            }
            return (result, true);
        }
    }
}
