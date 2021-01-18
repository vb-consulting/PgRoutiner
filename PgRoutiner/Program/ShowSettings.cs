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
            WriteLine(ConsoleColor.Cyan, settings);
        }
    }
}
