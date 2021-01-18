using System;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowStartupInfo()
        {
            WriteLine(ConsoleColor.Yellow,
                "***************************************",
                "** PostgreSQL Tool For .NET Projects **",
                $"**        Version: {Version}           **",
                "***************************************",
                "",
                "Usage: PgRoutiner [dir] [-h,--help] [-r,--run] [settings]",
                "",
                "- Use \"dir\" parameter to set working directory for your project other than current.",
                "- Use \"-h\" or \"--help\" to show help, available settings, their current  values and exit immediately.",
                "- Use \"-r\" or \"--run\" to start source code generation.",
                "");
        }
    }
}
