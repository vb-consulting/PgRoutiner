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
                "- Use \"-h\" or \"--help\" to show help, your current settings and exit immediately.",
                "- Use \"-r\" or \"--run\" to start source code generation.",
                "",
                "Submit issues to https://github.com/vb-consulting/PgRoutiner/issues",
                "If you would like to support this work consider small donation bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv");
        }
    }
}
