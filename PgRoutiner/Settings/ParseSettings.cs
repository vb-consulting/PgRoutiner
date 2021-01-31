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

        public static IConfigurationRoot ParseSettings(string[] args)
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
                Program.WriteLine("", "Using configuration files: ");
                Program.WriteLine(ConsoleColor.Cyan, files.ToArray());
            }

            IConfigurationRoot config = null;
            try
            {
                var configBuilder = new ConfigurationBuilder()
                    .AddJsonFile(pgroutinerFile, optional: true, reloadOnChange: false)
                    .AddJsonFile(settingsFile, optional: true, reloadOnChange: false)
                    .AddJsonFile(devSettingsFile, optional: true, reloadOnChange: false);
                config = configBuilder.Build();
                config.GetSection("PgRoutiner").Bind(Value);
                config.Bind(Value);

                new ConfigurationBuilder()
                    .AddCommandLine(argsList.ToArray(), new Dictionary<string, string> {
                        {ExecuteArgs.Alias, ExecuteArgs.Original},
                        {ConnectionArgs.Alias, ConnectionArgs.Original},
                        {SchemaArgs.Alias, SchemaArgs.Original},
                        {PgDumpArgs.Alias, PgDumpArgs.Original},
                        {OutputDirArgs.Alias, OutputDirArgs.Original},
                        {NotSimilarToArgs.Alias, NotSimilarToArgs.Original},
                        {SimilarToArgs.Alias, SimilarToArgs.Original},
                        {ModelDirArgs.Alias, ModelDirArgs.Original},
                        {UseRecordsArgs.Alias, UseRecordsArgs.Original},
                        {SchemaDumpFileArgs.Alias, SchemaDumpFileArgs.Original},
                        {DataDumpFileArgs.Alias, DataDumpFileArgs.Original},
                        {DbObjectsDirArgs.Alias, DbObjectsDirArgs.Original},
                        {CommentsMdFileArgs.Alias, CommentsMdFileArgs.Original},
                        {DirArgs.Alias, DirArgs.Original},
                        {PsqlArgs.Alias, PsqlArgs.Original}
                    })
                    .Build()
                    .Bind(Value);
            }
            catch (Exception e)
            {
                Program.DumpError($"Failed to bind configuration: {e.Message}");
                return null;
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
            return config;
        }
    }
}
