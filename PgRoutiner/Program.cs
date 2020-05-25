using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var currentDir = Directory.GetCurrentDirectory();
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(currentDir, "settings.json"), optional: true,
                    reloadOnChange: false)
                .AddJsonFile(Path.Join(currentDir, "appsettings.json"), optional: true,
                    reloadOnChange: false)
                .AddJsonFile(Path.Join(currentDir, "appsettings.Development.json"), optional: true,
                    reloadOnChange: false)
                .AddCommandLine(args);

            var config = configBuilder.Build();
            Settings.Value = new Settings();
            config.GetSection("pgr").Bind(Settings.Value);
        }
    }
}
