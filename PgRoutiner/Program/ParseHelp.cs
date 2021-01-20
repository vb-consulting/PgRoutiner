using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
        private static bool ParseHelp(string[] args)
        {
            if (ArgsInclude(args, "-h") || ArgsInclude(args, "--help"))
            {
                ShowInfo();
                ShowSettings();
                WriteLine(ConsoleColor.Yellow,
                "",
                "Issues",
                "   https://github.com/vb-consulting/PgRoutiner/issues",
                "Donate",
                "   bitcoincash:qp93skpzyxtvw3l3lqqy7egwv8zrszn3wcfygeg0mv",
                "   https://www.paypal.com/paypalme/vbsoftware/",
                "",
                $"Copyright (c) VB Consulting and VB Software {DateTime.Now.Year}. This source code is licensed under the MIT license.",
                "   https://github.com/vb-consulting/Norm.net/blob/master/LICENSE",
                "");
                return true;
            }
            return false;
        }
    }
}
