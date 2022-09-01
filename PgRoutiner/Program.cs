global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Text;
global using Npgsql;
global using PgRoutiner.DataAccess;
global using PgRoutiner.SettingsManagement;

using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using PgRoutiner.Builder;
using PgRoutiner.Connection;

namespace PgRoutiner;

static partial class Program
{
    public static IConfigurationRoot Config;
    public static string CurrentDir { get; private set; } = Directory.GetCurrentDirectory();

    private static string version = null;

    static void Main(string[] rawArgs)
    {
        var args = ParseArgs(rawArgs);

        if (args == null)
        {
            return;
        }
        if (ArgsInclude(args, Settings.HelpArgs))
        {
            Info.ShowInfo();
            return;
        }
        if (ArgsInclude(args, Settings.VersionArgs))
        {
            Info.ShowVersion();
            return;
        }
        Settings.Value.Silent = ArgsInclude(args, Settings.SilentArgs);
        Settings.Value.Dump = ArgsInclude(args, Settings.DumpArgs);
        Info.ShowStartupInfo();
        if (!SetCurrentDir(args))
        {
            return;
        }

        var (settingsFile, customSettings) = Settings.GetSettingsFile(args);
        Config = Settings.ParseSettings(args, settingsFile);
        if (Config == null)
        {
            return;
        }
        if (Info.ShowDebug(customSettings))
        {
            return;
        }
        Settings.ShowUpdatedSettings();
        using var connection = new ConnectionManager(Config).ParseConnectionString();
        if (connection == null)
        {
            return;
        }
        if (!Settings.ParseInitialSettings(connection, args.Length > 0, settingsFile))
        {
            return;
        }
        if (ArgsInclude(args, Settings.InfoArgs))
        {
            WriteLine("");
            return;
        }
        WriteLine("");
        Runner.Run(connection);
        WriteLine("");
    }

    public static string Version
    {
        get
        {
            if (version != null)
            {
                return version;
            }
            string location;
#if SELFCONTAINED
                location = $"{System.AppContext.BaseDirectory}pgroutiner.exe";
#else
            Assembly assembly = Assembly.GetExecutingAssembly();
            location = assembly.Location;
#endif
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(location);
            version = fvi.FileVersion;
            return version;
        }
    }

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
#if DEBUG
    public static void ParseProjectSetting(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.Project))
        {
            CurrentDir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(CurrentDir, settings.Project))));
        }
    }
#endif
}
