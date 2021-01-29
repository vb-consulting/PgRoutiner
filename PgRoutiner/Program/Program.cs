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
            var config = Settings.ParseSettings(args, out var error);
            if (ShowDebug())
            {
                return;
            }
            if (error != null)
            {
                DumpError(error);
                return;
            }
            if (!Settings.ParseInitialSettings(config.GetConnectionString(Settings.Value.Connection)))
            {
                return;
            }
            var connectionStr = config.GetConnectionString(Settings.Value.Connection);
            
            if (Builder.BuilMdDiff(connectionStr))
            {
                return;
            }
            if (Builder.ExecuteFile(connectionStr))
            {
                return;
            }

            if (Settings.Value.Run)
            {
                WriteLine(ConsoleColor.Yellow, "", "Running files generation ... ", "");
                Builder.Run(connectionStr);
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
