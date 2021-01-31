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

            void Remove(Arg value)
            {
                argsList.Remove(value.Alias);
                argsList.Remove(value.Name);
            }
            void ParseSwitches(object instance)
            {
                foreach(var prop in instance.GetType().GetProperties())
                {
                    if (prop.PropertyType != typeof(bool))
                    {
                        continue;
                    }
                    var argField = typeof(Settings).GetField($"{prop.Name}Args");
                    if (argField == null)
                    {
                        continue;
                    }
                    var argNames = (Arg) argField.GetValue(null);

                    if (Program.ArgsInclude(args, argNames))
                    {
                        prop.SetValue(instance, true);
                        Remove(argNames);
                    }
                }
            }

            ParseSwitches(Switches.Value);
            ParseSwitches(Value);
          
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

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(pgroutinerFile, optional: true, reloadOnChange: false)
                .AddJsonFile(settingsFile, optional: true, reloadOnChange: false)
                .AddJsonFile(devSettingsFile, optional: true, reloadOnChange: false);
            var config = configBuilder.Build();
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
                    {DirArgs.Alias, DirArgs.Original}
                })
                .Build()
                .Bind(Value);

            static void NullIfEmpty(params string[] names)
            {
                foreach(var n in names)
                {
                    var prop = typeof(Settings).GetProperty(n);
                    var value = prop.GetValue(Value) as string;
                    if (value == "")
                    {
                        prop.SetValue(Value, null);
                    }
                }
            }
            NullIfEmpty(
                nameof(SimilarTo), 
                nameof(NotSimilarTo), 
                nameof(ModelDir), 
                nameof(Schema), 
                nameof(SchemaDumpFile),
                nameof(DataDumpFile), 
                nameof(DbObjectsDir), 
                nameof(CommentsMdFile), 
                nameof(CommentsMdSimilarTo), 
                nameof(CommentsMdNotSimilarTo), 
                nameof(Execute));

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
