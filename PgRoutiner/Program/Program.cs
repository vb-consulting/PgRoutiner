using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner
{
    static partial class Program
    {
        public static bool Mute = false;

        static void Main(string[] rawArgs)
        {
            var args = ParseArgs(rawArgs);
            if (args == null)
            {
                return;
            }
            var help = ArgsInclude(args, Settings.HelpArgs);
            if (help)
            {
                ShowInfo();
                return;
            }

            var dump = ArgsInclude(args, Settings.DumpArgs);
            if ((dump && ArgsInclude(args, Settings.CommitCommentsArgs)) ||
                (dump && ArgsInclude(args, Settings.ExecuteArgs)))
            {
                Mute = true;
            }
            if (!help)
            {
                ShowStartupInfo();
            }
            if (!SetCurrentDir(args))
            {
                return;
            }
            var config = Settings.ParseSettings(args);
            if (ShowDebug())
            {
                return;
            }
            using var connection = Settings.ParseConnectionString(config);
            if (connection == null)
            {
                return;
            }
            if (!Settings.ParseInitialSettings(connection))
            {
                return;
            }
            if (Builder.BuilMdDiff(connection))
            {
                return;
            }
            if (Builder.ExecuteFile(connection))
            {
                return;
            }

            if (Settings.Value.Run)
            {
                WriteLine(ConsoleColor.Yellow, "", "Running files generation ... ", "");
                Builder.Run(connection);
            }
            WriteLine("");
        }

        private static bool ShowDebug()
        {
            if (Switches.Value.Settings)
            {
                ShowSettings();
                return true;
            }

            if (Switches.Value.Debug)
            {
                WriteLine("", "Debug: ");
                var path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                WriteLine("Executable dir: ");
                WriteLine(ConsoleColor.Cyan, " " + path);
                WriteLine("OS: ");
                WriteLine(ConsoleColor.Cyan, " " + Environment.OSVersion);
                WriteLine("Run: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Run}");
                WriteLine("CommitComments: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.CommitComments}");
                WriteLine("Dump: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Dump}");
                WriteLine("Execute: ");
                WriteLine(ConsoleColor.Cyan, $" {Settings.Value.Execute ?? "<null>"}");
                return true;
            }

            return false;
        }
    }
}
