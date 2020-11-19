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
                Dump($"Creating dir: {dir}");
                Directory.CreateDirectory(dir);
            }

            var modelDir = Settings.Value.ModelDir;
            if (modelDir != null)
            {
                modelDir = Path.Combine(CurrentDir, modelDir);
                if (!Directory.Exists(modelDir))
                {
                    Dump($"Creating dir: {modelDir}");
                    Directory.CreateDirectory(modelDir);
                }
            }

            //var i = 1;
            using var connection = new NpgsqlConnection(config.GetConnectionString(Settings.Value.Connection));
            {
                foreach (var item in connection.GetRoutines(Settings.Value))
                {
                    var builder = new SourceCodeBuilder(Settings.Value, item);
                    var name = string.Concat(item.Name.ToUpperCamelCase(), ".cs");
                    var fileName = Path.Join(dir, name);
                    var exists = File.Exists(fileName);

                    if (exists && Settings.Value.Overwrite == false)
                    {
                        Dump($"File {Settings.Value.OutputDir}/{name} exists, overwrite is set to false, skipping ...");
                        continue;
                    }
                    if (exists && Settings.Value.SkipIfExists.Contains(name))
                    {
                        Dump($"Skipping {Settings.Value.OutputDir}/{name}, already exists...");
                        continue;
                    }

                    Dump($"Creating {Settings.Value.OutputDir}/{name} ...");
                    File.WriteAllText(fileName, builder.Content);

                    if (modelDir != null && builder.ModelContent != null)
                    {
                        var modelName = string.Concat(builder.ModelName, ".cs");
                        var modelFileName = Path.Join(modelDir, modelName);
                        
                        if (Settings.Value.Overwrite || (Settings.Value.Overwrite == false && !File.Exists(modelFileName)))
                        {
                            Dump($"Creating {Settings.Value.ModelDir}/{modelName} ...");
                            File.WriteAllText(modelFileName, builder.ModelContent);
                        }
                        else if (File.Exists(modelFileName) && Settings.Value.Overwrite == false)
                        {
                            Dump($"File {Settings.Value.ModelDir}/{modelName} exists, overwrite is set to false, skipping ...");
                        }
                    }

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
