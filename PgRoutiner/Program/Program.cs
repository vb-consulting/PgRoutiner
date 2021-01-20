using System;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner
{
    static partial class Program
    {
        static void Main(string[] args)
        {
            ShowStartupInfo();
            SetCurrentDir(args);
            var config = Settings.ParseSettings(args, out var error);
            var project = ParseProjectFile();

            if (ParseHelp(args))
            {
                return;
            }

            if (error != null)
            {
                DumpError(error);
                return;
            }
            if (project == null)
            {
                return;
            }

            Settings.ParseInitialSettings(config.GetConnectionString(Settings.Value.Connection), project);

            if (Settings.Value.Run)
            {
                WriteLine(ConsoleColor.Yellow, "", "Running files generation ... ", "");
                Builder.Run(config.GetConnectionString(Settings.Value.Connection));
            }
            WriteLine("");
        }
    }
}
