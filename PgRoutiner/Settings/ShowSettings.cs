using System;

namespace PgRoutiner
{
    public partial class Settings
    {
        public static void ShowSettings()
        {
            Program.WriteLine("", "Current settings:");
            foreach (var l in Settings.BuildFormatedSettings(wrap: false).Split(Environment.NewLine))
            {
                var line = l;
                var trim = l.Trim();

                if (trim.StartsWith("/*"))
                {
                    Program.WriteLine(ConsoleColor.Green, $" {line.Replace("    ", "")}");
                }
                else if (trim.StartsWith("\""))
                {
                    var split = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                    var key = split.Length == 2 ? split[0].Replace("\"", "") : null;
                    var value = split.Length == 2 ? split[1].Trim() : split[0].Trim();

                    if (key != null)
                    {
                        Program.Write(ConsoleColor.Yellow, key.Replace("    ", " "));
                        Program.Write(": ");
                        Program.WriteLine(ConsoleColor.Cyan, value);
                    }
                    else
                    {
                        Program.WriteLine(ConsoleColor.Cyan, $"  {value}");
                    }
                }
                else
                {
                    Program.WriteLine(ConsoleColor.Cyan, line.Replace("    ", " "));
                }
            }

            ShowSettingsLink();
        }

        public static void ShowSettingsLink()
        {
            Program.WriteLine("", "To learn how to work with settings, visit: ");
            Program.WriteLine(ConsoleColor.Cyan, " https://github.com/vb-consulting/PgRoutiner/blob/master/SETTINGS.MD", "");
        }

        public static void ShowUpdatedSettings()
        { 
            bool settingsWritten = false;
            void WriteSetting(string name, object value)
            {
                if (!settingsWritten)
                {
                    Program.WriteLine("");
                    Program.WriteLine("Using settings: ");
                }
                if (value == null)
                {
                    Program.Write(ConsoleColor.Yellow, $" {name.ToKebabCase()} = ");
                    Program.WriteLine(ConsoleColor.Cyan, "null");
                }
                else if (value.GetType() == typeof(bool) && (bool)value)
                {
                    Program.WriteLine(ConsoleColor.Yellow, $" {name.ToKebabCase()}");
                }
                else
                {
                    Program.Write(ConsoleColor.Yellow, $" {name.ToKebabCase()} = ");
                    Program.WriteLine(ConsoleColor.Cyan, $"{value}");
                }
                settingsWritten = true;
            }

            var defaultValue = new Settings();
            foreach (var prop in Value.GetType().GetProperties())
            {
                var name = prop.Name;
                if (prop.PropertyType == typeof(bool))
                {
                    var v1 = (bool)prop.GetValue(Value);
                    var v2 = (bool)prop.GetValue(defaultValue);
                    if (v1 == v2)
                    {
                        continue;
                    }
                    WriteSetting(prop.Name, v1);
                }
                if (prop.PropertyType == typeof(string))
                {
                    var s1 = (string)prop.GetValue(Value);
                    var s2 = (string)prop.GetValue(defaultValue);
                    if (string.Equals(s1, s2))
                    {
                        continue;
                    }
                    WriteSetting(prop.Name, s1);
                }
            }
        }
    }
}
