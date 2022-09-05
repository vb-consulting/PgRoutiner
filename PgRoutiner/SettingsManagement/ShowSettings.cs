using System;
using System.Collections.Generic;
using System.Linq;

namespace PgRoutiner.SettingsManagement
{
    public partial class Current
    {
        public static void ShowSettings()
        {
            Program.WriteLine("", "Current settings:");
            bool comment = false;
            foreach (var l in FormatedSettings.Build(wrap: false).Split(Environment.NewLine))
            {
                var line = l;
                var trim = l.Trim();

                if (trim.StartsWith("/*") || comment)
                {
                    Program.WriteLine(ConsoleColor.Green, $" {line.Replace("    ", "")}");
                    comment = true;
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
                if (trim.Contains("*/") && comment)
                {
                    comment = false;
                }
            }

            Program.WriteLine("", "To get help please navigate to: ");
            Program.WriteLine(ConsoleColor.Cyan, " https://github.com/vb-consulting/postgresql-driven-development-demo/blob/master/PDD.Database/README.md", "");
        }


        public static void ShowUpdatedSettings()
        { 
            bool settingsWritten = false;
            void WriteSetting(string name, object value, System.Type type)
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
                else if (type == typeof(bool) && (bool)value)
                {
                    Program.WriteLine(ConsoleColor.Yellow, $" {name.ToKebabCase()}");
                }
                else
                {
                    Program.Write(ConsoleColor.Yellow, $" {name.ToKebabCase()} = ");
                    if (type == typeof(IList<string>))
                    {
                        Program.WriteLine(ConsoleColor.Cyan, $"{string.Join(", ", (value as IList<string>).ToArray())}");
                    } 
                    else
                    if (type == typeof(HashSet<string>))
                    {
                        Program.WriteLine(ConsoleColor.Cyan, $"{string.Join(", ", (value as HashSet<string>).ToArray())}");
                    }
                    else
                    {
                        Program.WriteLine(ConsoleColor.Cyan, $"{value}");
                    }
                }
                settingsWritten = true;
            }

            var defaultValue = new Current();
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
                    WriteSetting(prop.Name, v1, prop.PropertyType);
                }
                if (prop.PropertyType == typeof(string))
                {
                    var s1 = (string)prop.GetValue(Value);
                    var s2 = (string)prop.GetValue(defaultValue);
                    if (string.Equals(s1, s2))
                    {
                        continue;
                    }
                    WriteSetting(prop.Name, s1, prop.PropertyType);
                }
                if (prop.PropertyType == typeof(IList<string>))
                {
                    var l1 = (IList<string>)prop.GetValue(Value);
                    var l2 = (IList<string>)prop.GetValue(defaultValue);
                    if (l1.SequenceEqual(l2))
                    {
                        continue;
                    }
                    WriteSetting(prop.Name, l1, prop.PropertyType);
                }
                if (prop.PropertyType == typeof(HashSet<string>))
                {
                    var l1 = (HashSet<string>)prop.GetValue(Value);
                    var l2 = (HashSet<string>)prop.GetValue(defaultValue);
                    if (l1.SequenceEqual(l2))
                    {
                        continue;
                    }
                    WriteSetting(prop.Name, l1, prop.PropertyType);
                }
            }
        }
    }
}
