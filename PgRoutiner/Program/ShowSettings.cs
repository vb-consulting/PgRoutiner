using System;
using Newtonsoft.Json;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowSettings()
        {
            WriteLine("", "Current settings:");
            foreach(var l in Settings.BuildFormatedSettings(false).Split(Environment.NewLine))
            {
                var line = l;
                var trim = l.Trim();

                if (trim.StartsWith("/*"))
                {
                    WriteLine(ConsoleColor.Green, $" {line.Replace("    ", "")}");
                }
                else if (trim.StartsWith("\""))
                {
                    var split = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                    var key = split.Length == 2 ? split[0].Replace("\"", "") : null;
                    var value = split.Length == 2 ? split[1].Trim() : split[0].Trim();

                    if (key != null)
                    {
                        Write(ConsoleColor.Yellow, key.Replace("    ", " "));
                        Write(": ");
                        WriteLine(ConsoleColor.Cyan, value);
                    }
                    else
                    {
                        WriteLine(ConsoleColor.Cyan, $"  {value}");
                    }
                }
                else
                {
                    WriteLine(ConsoleColor.Cyan, line.Replace("    ", " "));
                }
            }
            
            ShowSettingsLink();
        }
    }
}
