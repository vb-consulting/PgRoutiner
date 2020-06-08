using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;


namespace PgRoutiner
{
    partial class Program
    {
        //const int Top = 24;

        static void Run(IConfiguration config)
        {
            var dir = Path.Combine(CurrentDir, Settings.Value.OutputDir);

            if (!Directory.Exists(dir))
            {
                Dump($"Creating \"{dir}\" dir..");
                Directory.CreateDirectory(dir);
            }

            //var i = 1;
            using var connection = new NpgsqlConnection(config.GetConnectionString(Settings.Value.Connection));
            {
                foreach (var item in connection.GetRoutines(Settings.Value))
                {
                    var name = string.Concat(item.Name.ToUpperCamelCase(), ".cs");
                    var fileName = Path.Join(dir, name);

                    if (Settings.Value.Overwrite == false && File.Exists(fileName))
                    {
                        Dump($"File {Settings.Value.OutputDir}/{name} exists, overwrite is set to false, skipping ...");
                        continue;
                    }

                    Dump($"Creating {Settings.Value.OutputDir}/{name} ...");
                    File.WriteAllText(fileName, new SourceCodeBuilder(Settings.Value, item).Build());

                    //if (++i > Top) break;
                }
            }
        }

        static void Dump(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
