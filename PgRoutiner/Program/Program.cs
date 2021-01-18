﻿using System;
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
            var success = ParseProjectFile();

            if (ParseHelp(args))
            {
                return;
            }
            if (config != null && success && Settings.Value.Run)
            {
                WriteLine(ConsoleColor.Yellow, "", "Running files generation ... ", "");
                Builder.Run(config.GetConnectionString(Settings.Value.Connection));
            }
            else if (success && error != null)
            {
                DumpError(error);
            }
            WriteLine("");
        }
    }
}