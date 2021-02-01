using System;
using System.IO;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        private static void BuildDataAccess(NpgsqlConnection connection)
        {
            if (string.IsNullOrEmpty(Settings.Value.OutputDir))
            {
                return;
            }

            var outputDir = GetOutputDir();
            var modelDir = GetModelDir();

            foreach (var group in connection.GetRoutineGroups(Settings.Value))
            {
                var name = group.Key;
                var shortFilename = string.Concat(name.ToUpperCamelCase(), ".cs");
                var fullFileName = Path.GetFullPath(Path.Join(outputDir, shortFilename));
                var shortName = outputDir.Contains("/") ?
                                $"{outputDir.Split("/").Last()}{"/"}{shortFilename}" :
                                $"{outputDir.Split("\\").Last()}{"\\"}{shortFilename}";
                    //$"{outputDir.Split(Path.DirectorySeparatorChar).Last()}{Path.DirectorySeparatorChar}{shortFilename}";
                var exists = File.Exists(fullFileName);

                if (!Settings.Value.Dump && exists && Settings.Value.Overwrite == false)
                {
                    Dump($"File {shortName} exists, overwrite is set to false, skipping ...");
                    continue;
                }
                if (!Settings.Value.Dump && exists && Settings.Value.SkipIfExists != null && (
                    Settings.Value.SkipIfExists.Contains(name) || Settings.Value.SkipIfExists.Contains(shortFilename))
                    )
                {
                    Dump($"Skipping {shortName}, already exists...");
                    continue;
                }
                if (!Settings.Value.Dump && exists && Settings.Value.AskOverwrite && Program.Ask($"File {shortName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                {
                    Dump($"Skipping {shortName}...");
                    continue;
                }
                Dump($"Building {shortName}...");
                var module = new RoutineModule(Settings.Value);
                RoutineCode code;
                try
                {
                    code = new RoutineCode(Settings.Value, name, module.Namespace, group, connection);
                }
                catch (ArgumentException e)
                {
                    Error($"File {shortName} could not be written. {e.Message}");
                    continue;
                }
                var models = code.Models.Values.ToArray();
                if (modelDir != null && models.Length > 0)
                {
                    foreach (var (modelName, modelContent) in code.Models)
                    {
                        var shortModelFilename = string.Concat(modelName.ToUpperCamelCase(), ".cs");
                        var fullModelFileName = Path.Join(modelDir, shortModelFilename);
                        var shortModelName = modelDir.Contains("/") ?
                                $"{modelDir.Split("/").Last()}{"/"}{shortModelFilename}" :
                                $"{modelDir.Split("\\").Last()}{"\\"}{shortModelFilename}";

                        var modelExists = File.Exists(fullModelFileName);

                        if (!Settings.Value.Dump && modelExists && Settings.Value.Overwrite == false)
                        {
                            Dump($"File {shortModelName} exists, overwrite is set to false, skipping ...");
                            continue;
                        }
                        if (!Settings.Value.Dump && modelExists && Settings.Value.SkipIfExists != null && (
                            Settings.Value.SkipIfExists.Contains(modelName) || Settings.Value.SkipIfExists.Contains(shortModelName))
                            )
                        {
                            Dump($"Skipping {shortModelName}, already exists...");
                            continue;
                        }
                        if (!Settings.Value.Dump && modelExists && Settings.Value.AskOverwrite && Program.Ask($"File {shortModelName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                        {
                            Dump($"Skipping {shortModelName}...");
                            continue;
                        }
                        Dump($"Building {shortModelName}...");
                        var modelModule = new Module(Settings.Value);
                        modelModule.AddNamespace(Settings.Value.ModelDir.PathToNamespace());
                        modelModule.AddItems(modelContent);
                        if (modelModule.Namespace != module.Namespace)
                        {
                            module.AddUsing(modelModule.Namespace);
                        }
                        Content.Add((modelModule.ToString(), fullModelFileName));
                    }
                }
                else
                {
                    module.AddItems(models);
                }
                module.AddItems(code.Class);
                Modules.Add((code.Methods, module.Namespace));
                Content.Add((module.ToString(), fullFileName));
            }
        }
    }
}
