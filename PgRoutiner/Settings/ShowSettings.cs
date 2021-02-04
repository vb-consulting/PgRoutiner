using System;

namespace PgRoutiner
{
    public partial class Settings
    {
        public static void ShowSettings()
        { 
            bool settingsWritten = false;
            void WriteSetting(string name, object value)
            {
                if (!settingsWritten)
                {
                    Program.WriteLine("");
                    Program.WriteLine("Using settings: ");
                }
                if (value.GetType() == typeof(bool) && (bool)value)
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
