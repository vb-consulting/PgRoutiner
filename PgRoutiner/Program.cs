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
using System.Text.Json;
using Microsoft.Extensions.Configuration;
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
        //rawArgs = new string[] { "--info" };
        //rawArgs = new string[] { "--mo" };
        //rawArgs = new string[] { "-wcf", "./Test/test.json" };
        //rawArgs = new string[] { "--list", "int" };
        //rawArgs = new string[] { "--ddl", "*" };
        //rawArgs = new string[] { "--s", "comp" };
        //rawArgs = new string[] { "--settings", "--list", "--definition" };
        //rawArgs = new string[] { "--search", "film" };
        //rawArgs = new string[] { "--inserts", "select b.first_name || ' ' || b.last_name as actor, array_agg(c.title) as titles from film_actor a inner join actor b on a.actor_id = b.actor_id inner join film c on a.film_id = c.film_id group by b.first_name || ' ' || b.last_name" };
        //rawArgs = new string[] { "-l" };
        //rawArgs = new string[] { "-l", "film_in" };
        //rawArgs = new string[] { "-r", "-d" };

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
            WriteLine("");
            return;
        }
        if (ConsoleSettings.Info)
        {
            ProgramInfo.ShowVersion();
            Console.WriteLine();
            var path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            WriteLine("Executable dir: ");
            WriteLine(ConsoleColor.Cyan, " " + path);
            Console.WriteLine();
            WriteLine("OS: ");
            WriteLine(ConsoleColor.Cyan, " " + Environment.OSVersion);
        }
        
        Current.Value.Silent = ConsoleSettings.Silent;
        Current.Value.DumpConsole = ConsoleSettings.DumpConsole;

        if (!SetCurrentDir(args))
        {
            return;
        }

        var (settingsFile, customSettings) = Current.GetSettingsFile(args);
        Config = Current.ParseSettings(args, settingsFile);
        if ((Current.Value != null && Current.Value.Verbose) || Current.Value.Info || ConsoleSettings.Info)
        {
            WriteLine("", "Using dir: ");
            WriteLine(ConsoleColor.Cyan, " " + CurrentDir);
        }

        if (Config == null)
        {
            return;
        }
        if (Current.Value.Verbose || Current.Value.Info || ConsoleSettings.Info)
        {
            Current.ShowUpdatedSettings();
        }
        //AppContext.SetSwitch("Npgsql.EnableSqlRewriting", false);
        using var connection = new ConnectionManager(Config).ParseConnectionString();
        if (!Current.ParseInitialSettings(connection, args.Length > 0, settingsFile))
        {
            return;
        }

        if (Current.Value.Info || ConsoleSettings.Info)
        {
            if (connection != null)
            {
                var builder = new Builder.Dump.PgDumpBuilder(Current.Value, connection);
                var hasPgDump = Builder.Dump.PgDumpVersion.Check(builder);
                if (hasPgDump)
                {
                    WriteLine("");
                    WriteLine("pg_dump: ");
                    WriteLine(ConsoleColor.Cyan, " " + builder.Command);
                }
                else
                {
                    WriteLine("");
                    WriteLine("pg_dump: ");
                    WriteLine(ConsoleColor.Red, " Not available.");
                }

                var hasPgRestore = Builder.Dump.PgDumpVersion.Check(builder, restore: true);
                if (hasPgRestore)
                {
                    WriteLine("");
                    WriteLine("pg_restore: ");
                    WriteLine(ConsoleColor.Cyan, " " + builder.Command);
                }
                else
                {
                    WriteLine("");
                    WriteLine("pg_restore: ");
                    WriteLine(ConsoleColor.Red, " Not available.");
                }
            }
            

            WriteLine("");
            return;
        }

        if (Current.Value.Settings || ConsoleSettings.Settings)
        {
            Current.ShowSettings();
            return;
        }
        
        if (connection == null)
        {
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

    public class DirSettings
    {
        public string Dir { get; set; } = null;
    }

    public static bool SetCurrentDir(string[] args)
    {
        var configBuilder = new ConfigurationBuilder().AddCommandLine(args,
            new Dictionary<string, string> {
                    { Current.DirArgs.Alias, Current.DirArgs.Name.Replace("--", "") }
            });

        var ds = new DirSettings();
        configBuilder.Build().Bind(ds);
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
