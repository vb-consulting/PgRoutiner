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
            if (Settings.Value.OutputDir == null)
            {
                return;
            }

            var outputDir = GetOutputDir();
            var modelDir = GetModelDir();

            foreach (var group in connection.GetRoutineGroups(Settings.Value, all: false))
            {
                var name = group.Key;
                var shortFilename = string.Concat(name.ToUpperCamelCase(), ".cs");
                var fullFileName = Path.GetFullPath(Path.Join(outputDir, shortFilename));
                var relative = fullFileName.GetRelativePath();
                var exists = File.Exists(fullFileName);

                if (!Settings.Value.Dump && exists && Settings.Value.Overwrite == false)
                {
                    DumpFormat("File {0} exists, overwrite is set to false, skipping ...", fullFileName);
                    continue;
                }
                if (!Settings.Value.Dump && exists && Settings.Value.SkipIfExists != null && (
                    Settings.Value.SkipIfExists.Contains(name) || 
                    Settings.Value.SkipIfExists.Contains(shortFilename) || 
                    Settings.Value.SkipIfExists.Contains(relative))
                    )
                {
                    DumpFormat("Skipping {0}, already exists ... ", relative);
                    continue;
                }
                if (!Settings.Value.Dump && exists && Settings.Value.AskOverwrite && 
                    Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                {
                    DumpFormat("Skipping {0} ... ", relative);
                    continue;
                }

                var module = new RoutineModule(Settings.Value);
                RoutineCode code;
                try
                {
                    code = new RoutineCode(Settings.Value, name, module.Namespace, group, connection);
                }
                catch (ArgumentException e)
                {
                    Error($"File {relative} could not be generated. {e.Message}");
                    continue;
                }
                var models = code.Models.Values.ToArray();
                if (modelDir != null && models.Length > 0)
                {
                    foreach (var (modelName, modelContent) in code.Models)
                    {
                        var shortModelFilename = string.Concat(modelName.ToUpperCamelCase(), ".cs");
                        var fullModelFileName = Path.Join(modelDir, shortModelFilename);
                        var relativeModelName = fullModelFileName.GetRelativePath();
                        var modelExists = File.Exists(fullModelFileName);

                        if (!Settings.Value.Dump && modelExists && Settings.Value.Overwrite == false)
                        {
                            DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relativeModelName);
                            continue;
                        }
                        if (!Settings.Value.Dump && modelExists && Settings.Value.SkipIfExists != null && (
                            Settings.Value.SkipIfExists.Contains(modelName) || 
                            Settings.Value.SkipIfExists.Contains(shortModelFilename) ||
                            Settings.Value.SkipIfExists.Contains(relativeModelName))
                            )
                        {
                            DumpFormat("Skipping {0}, already exists ...", relativeModelName);
                            continue;
                        }
                        if (!Settings.Value.Dump && modelExists && Settings.Value.AskOverwrite && 
                            Program.Ask($"File {relativeModelName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                        {
                            DumpFormat("Skipping {0} ...", relativeModelName);
                            continue;
                        }
                        DumpFormat("Building {0}...", relativeModelName);
                        var modelModule = new Module(Settings.Value);
                        modelModule.AddNamespace(Settings.Value.ModelDir.PathToNamespace());
                        modelModule.AddItems(modelContent);
                        if (modelModule.Namespace != module.Namespace)
                        {
                            module.AddUsing(modelModule.Namespace);
                        }
                        DumpRelativePath("Creating file: {0} ...", fullModelFileName);
                        WriteFile(fullModelFileName, modelModule.ToString());
                    }
                }
                else
                {
                    module.AddItems(models);
                }
                module.AddItems(code.Class);
                if (code.Methods.Count > 0)
                {
                    Extensions.Add(new Extension { Methods = code.Methods, Namespace = module.Namespace, Name = code.Methods.First().Name });
                }
                DumpRelativePath("Creating file: {0} ...", fullFileName);
                WriteFile(fullFileName, module.ToString());
            }
        }

        private static void BuildDataAccessExtensions(NpgsqlConnection connection)
        {
            foreach (var group in connection.GetRoutineGroups(Settings.Value, all: false))
            {
                var name = group.Key;
                var module = new RoutineModule(Settings.Value);
                RoutineCode code;
                try
                {
                    code = new RoutineCode(Settings.Value, name, module.Namespace, group, connection);
                }
                catch
                {
                    continue;
                }
                if (code.Methods.Count > 0)
                {
                    Extensions.Add(new Extension { Methods = code.Methods, Namespace = module.Namespace, Name = code.Methods.First().Name });
                }
            }
        }

        private static string GetOutputDir()
        {
            var dir = Path.Combine(Program.CurrentDir, Settings.Value.OutputDir);
            if (!Directory.Exists(dir))
            {
                DumpRelativePath("Creating dir: {0} ...", dir);
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private static string GetModelDir()
        {
            var dir = Settings.Value.ModelDir;
            if (dir != null)
            {
                dir = Path.Combine(Program.CurrentDir, dir);
                if (!Directory.Exists(dir))
                {
                    DumpRelativePath("Creating dir: {0} ...", dir);
                    Directory.CreateDirectory(dir);
                }
            }
            return dir;
        }
    }
}
