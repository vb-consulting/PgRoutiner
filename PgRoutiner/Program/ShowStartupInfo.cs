using System;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowStartupInfo()
        {
            WriteLine(ConsoleColor.Yellow,
                "***************************************",
                $"**     PgRoutiner: {Version}           **",
                "***************************************",
                "");
            Write(ConsoleColor.Yellow, "- Type ");
            Write(ConsoleColor.Cyan, $"pgroutiner {Settings.HelpArgs.Alias}");
            Write(ConsoleColor.Yellow, " or ");
            Write(ConsoleColor.Cyan, $"pgroutiner {Settings.HelpArgs.Name}");
            Write(ConsoleColor.Yellow, " to see help on available commands and settings.");
            WriteLine("");
            Write(ConsoleColor.Yellow, "- Type ");
            Write(ConsoleColor.Cyan, $"pgroutiner {Settings.SettingsArgs.Alias}");
            Write(ConsoleColor.Yellow, " or ");
            Write(ConsoleColor.Cyan, $"pgroutiner {Settings.SettingsArgs.Name}");
            Write(ConsoleColor.Yellow, " to see the currently selected settings.");
            WriteLine("");

            WriteLine(ConsoleColor.Yellow, "", "Issues");
            WriteLine(ConsoleColor.Cyan, "   https://github.com/vb-consulting/PgRoutiner/issues");
            WriteLine(ConsoleColor.Yellow, "Donate");
            WriteLine(ConsoleColor.Cyan, "   bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv");
            WriteLine(ConsoleColor.Cyan, "   https://www.paypal.com/paypalme/vbsoftware/", "");
            WriteLine(ConsoleColor.Yellow, $"Copyright (c) VB Consulting and VB Software {DateTime.Now.Year}. This program and source code is licensed under the MIT license.");
            WriteLine(ConsoleColor.Cyan, "   https://github.com/vb-consulting/PgRoutiner/blob/master/LICENSE", "");
        }
    }
}
