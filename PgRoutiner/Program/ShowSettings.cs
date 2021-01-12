using System;
using Newtonsoft.Json;

namespace PgRoutiner
{
    static partial class Program
    {
        private static void ShowSettings()
        {
            WriteLine("", "Current settings:");
            var settings = JsonConvert.SerializeObject(Settings.Value, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
                //.Replace(string.Concat("{", Environment.NewLine, "  "), " ")
                //.Replace("}", " ")
                //.Replace(string.Concat(Environment.NewLine, "  "), Environment.NewLine + " ");
            WriteLine(ConsoleColor.Cyan, settings);
        }
    }
}
