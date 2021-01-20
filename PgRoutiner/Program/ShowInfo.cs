using System;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowInfo()
        {
            WriteLine("");
            Console.Write("Usage: ");

            WriteLine(ConsoleColor.Yellow, "PgRoutiner [dir] [-h,--help] [-r,--run] [settings]", "");
            WriteSetting("[dir]", "Set working directory for your project other than current.");
            WriteSetting("[-h,--help]", "Show help, your current settings and exit immediately.");
            WriteSetting("[-r,--run]", "Start the source code generation.");
            WriteSetting("[settings]", "Override one of the available settings by using `setting=value` argument format");

            WriteLine(ConsoleColor.Yellow,"",
                "To learn how to work with settings, visit https://github.com/vb-consulting/PgRoutiner/SETTINGS.MD");
        }
    }
}
