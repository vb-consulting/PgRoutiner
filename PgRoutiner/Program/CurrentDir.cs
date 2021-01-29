using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner
{
    static partial class Program
    {
        public static string CurrentDir { get; private set; } = Directory.GetCurrentDirectory();

        private class DirSettings
        {
            public string Dir { get; set; } = null;
        }

        public static bool SetCurrentDir(string[] args)
        {
            var configBuilder = new ConfigurationBuilder().AddCommandLine(args, 
                new Dictionary<string, string> {
                    { Settings.DirArgs.Alias, Settings.DirArgs.Name.Replace("--", "") } 
                });
            var config = configBuilder.Build();
            var ds = new DirSettings();
            config.Bind(ds);
            if (!string.IsNullOrEmpty(ds.Dir))
            {
                CurrentDir = Path.Join(CurrentDir, ds.Dir);
            }
            var dir = Path.GetFullPath(CurrentDir);
            if (!Directory.Exists(dir))
            {
                DumpError($"Directory {dir} does not exists!");
                return false;
            }
            WriteLine("", "Using dir: ");
            WriteLine(ConsoleColor.Cyan, " " + dir);
            return true;
        }

        public static void ParseProjectSetting(Settings settings)
        {
            if (!string.IsNullOrEmpty(settings.Project))
            {
                CurrentDir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(CurrentDir, settings.Project))));
            }
        }
    }
}
