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
using Newtonsoft.Json.Linq;
using PgRoutiner.Builder;
using PgRoutiner.Connection;

namespace PgRoutiner;

static partial class Program
{
    public static IConfigurationRoot Config;
    public static string CurrentDir { get; private set; } = Directory.GetCurrentDirectory();

    private static string version = null;
    private static bool? docker = false;

    public static Current ConsoleSettings;

    static void Main(string[] rawArgs)
    {
        //rawArgs = new string[] { "-ls" };
        var args = ParseArgs(rawArgs);

        if (args == null)
        {
            return;
        }
        ConsoleSettings = BindConsole(args);
        if (ConsoleSettings.Help)
        {
            ProgramInfo.ShowInfo();
            return;
        }
        if (ConsoleSettings.Version)
        {
            ProgramInfo.ShowVersion();
            return;
        }
        Current.Value.Silent = ConsoleSettings.Silent;
        Current.Value.DumpConsole = ConsoleSettings.DumpConsole;

        if (!SetCurrentDir(args))
        {
            return;
        }

        var (settingsFile, customSettings) = Current.GetSettingsFile(args);
        Config = Current.ParseSettings(args, settingsFile);
        if (Current.Value != null && Current.Value.Verbose)
        {
            WriteLine("", "Using dir: ");
            WriteLine(ConsoleColor.Cyan, " " + CurrentDir);
        }
            
        if (Config == null)
        {
            return;
        }
        if (ProgramInfo.ShowDebug(customSettings))
        {
            return;
        }
        if (Current.Value.Verbose)
        {
            Current.ShowUpdatedSettings();
        }
        using var connection = new ConnectionManager(Config).ParseConnectionString();
        if (connection == null)
        {
            return;
        }
        if (!Current.ParseInitialSettings(connection, args.Length > 0, settingsFile))
        {
            return;
        }
        if (ConsoleSettings.Info)
        {
            WriteLine("");
            return;
        }

        //WriteLine("");
        Runner.Run(connection);
        //WriteLine("");
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

    public static bool Docker
    {
        get
        {
            if (docker != null)
            {
                return docker.Value;
            }
            
            if (File.Exists("/.dockerenv"))
            {
                docker = true;
                return true;
            }
            docker = false;
            return false;
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
                    { Current.DirArgs.Alias, Current.DirArgs.Name.Replace("--", "") }
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
        return true;
    }
#if DEBUG
    public static void ParseProjectSetting(Current settings)
    {
        if (!string.IsNullOrEmpty(settings.Project))
        {
            CurrentDir = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(CurrentDir, settings.Project))));
        }
    }
#endif
}
