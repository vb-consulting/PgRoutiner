using System;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowStartupInfo()
        { 
            WriteLine(ConsoleColor.Yellow,
                $"PgRoutiner dotnet tool, version {Version}",
                "Scaffold your PostgreSQL routines in .NET project!",
                "Usage: PgRoutiner [dir] [-h,--help] [run] [settings]",
                "",
                "- Use \"-h\" or \"--help\" to display help.",
                "- Use \"run\" to start source code generation.");
        }
    }
}
