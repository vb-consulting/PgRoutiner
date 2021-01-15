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
                return true;
            }
            return false;
        }
    }
}
