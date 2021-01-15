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
            var config = Settings.ParseSettings(args);
            var success = ParseProjectFile();

            if (ParseHelp(args))
            {
                return;
            }
#if DEBUG
            if (config != null && success)
            {
                WriteLine(ConsoleColor.Yellow, "", "Running files generation ... ", "");
                Builder.Run(config.GetConnectionString(Settings.Value.Connection));
            }
#else
            if (config != null && success && ArgsInclude(args, "run"))
            {
                WriteLine(ConsoleColor.Yellow, "", "Running files generation ... ", "");
                Builder.Run(config.GetConnectionString(Settings.Value.Connection));
            }
#endif
        }
    }
}
