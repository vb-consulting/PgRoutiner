using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner
{
    static partial class Program
    {
        public static bool Mute = false;
        public static IConfigurationRoot Config;

        static void Main(string[] rawArgs)
        {
            //var args = ParseArgs(new string[] {  "--crud-delete:0", "bla", "--data-dump-tables:0", "t1" });
            var args = ParseArgs(rawArgs);
            if (args == null)
            {
                return;
            }
            if (ArgsInclude(args, Settings.HelpArgs))
            {
                ShowInfo();
                return;
            }
            if (ArgsInclude(args, Settings.VersionArgs))
            {
                ShowVersion();
                return;
            }
            Mute = ArgsInclude(args, Settings.DumpArgs);
            ShowStartupInfo();
            if (!SetCurrentDir(args))
            {
                return;
            }
            Config = Settings.ParseSettings(args);
            if (Config == null)
            {
                return;
            }
            if (ShowDebug())
            {
                return;
            }
            Settings.ShowUpdatedSettings();
            using var connection = new ConnectionManager(Config).ParseConnectionString();
            if (connection == null)
            {
                return;
            }
            if (!Settings.ParseInitialSettings(connection, args.Length > 0))
            {
                return;
            }
            if (ArgsInclude(args, Settings.InfoArgs))
            {
                WriteLine("");
                return;
            }
            WriteLine("");
            Builder.Run(connection);
            WriteLine("");
        }

        private static bool ShowDebug()
        {
            if (Switches.Value.Settings)
            {
                Settings.ShowSettings();
                return true;
            }

            if (Switches.Value.Debug)
            {
                WriteLine("", "Debug: ");
                WriteLine("Version: ");
                WriteLine(ConsoleColor.Cyan, " " + Version);
                var path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                WriteLine("Executable dir: ");
                WriteLine(ConsoleColor.Cyan, " " + path);
                WriteLine("OS: ");
                WriteLine(ConsoleColor.Cyan, " " + Environment.OSVersion);
                WriteLine("Run: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Routines}");
                WriteLine("CommitComments: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.CommitMd}");
                WriteLine("Dump: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Dump}");
                WriteLine("Execute: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Execute ?? "<null>"}");
                WriteLine("Diff: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Diff}");
                return true;
            }

            return false;
        }
    }
}
