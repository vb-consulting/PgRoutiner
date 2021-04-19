﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    public abstract class CodeBuilder
    {
        protected readonly NpgsqlConnection connection;
        protected readonly Settings settings;
        protected readonly CodeSettings codeSettings;

        private static HashSet<string> UserDefinedModels { get; } = new();
        private static HashSet<string> DirsEmptied { get; } = new();

        public CodeBuilder(NpgsqlConnection connection, Settings settings, CodeSettings codeSettings)
        {
            this.connection = connection;
            this.settings = settings;
            this.codeSettings = codeSettings;
        }

        public void Build()
        {
            if (codeSettings.OutputDir == null)
            {
                return;
            }

            var outputDir = GetOutputDir();
            var modelDir = GetModelDir();

            if (codeSettings.EmptyOutputDir)
            {
                EmptyDir(outputDir);
            }

            if (settings.EmptyModelDir)
            {
                EmptyDir(modelDir);
            }

            foreach ((Code code, string name, string shortFilename, string fullFileName, string relative, Module module) in GetCodes(outputDir))
            {
                if (code == null)
                {
                    continue;
                }
                var exists = File.Exists(fullFileName);

                if (!settings.Dump && exists && codeSettings.Overwrite == false)
                {
                    Builder.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", fullFileName);
                    continue;
                }
                if (!settings.Dump && exists && settings.SkipIfExists != null && (
                    settings.SkipIfExists.Contains(name) ||
                    settings.SkipIfExists.Contains(shortFilename) ||
                    settings.SkipIfExists.Contains(relative))
                    )
                {
                    Builder.DumpFormat("Skipping {0}, already exists ... ", relative);
                    continue;
                }
                if (!settings.Dump && exists && codeSettings.AskOverwrite &&
                    Program.Ask($"File {relative} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                {
                    Builder.DumpFormat("Skipping {0} ... ", relative);
                    continue;
                }

                var models = code.Models.Values.ToArray();
                foreach (var (modelName, modelContent) in code.Models)
                {
                    if (modelName == null)
                    {
                        continue;
                    }

                    if (modelDir == null && !code.UserDefinedModels.Contains(modelName))
                    {
                        module.AddItems(models);
                    }
                    else
                    {
                        var modelModule = new Module(settings);
                        modelModule.AddNamespace((settings.ModelDir ?? codeSettings.OutputDir).PathToNamespace());
                        modelModule.AddItems(modelContent);
                        if (settings.ModelCustomNamespace != null)
                        {
                            modelModule.Namespace = settings.ModelCustomNamespace;
                        }
                        if (modelModule.Namespace != module.Namespace)
                        {
                            module.AddUsing(modelModule.Namespace);
                            code.ModuleNamespace = modelModule.Namespace;
                        }

                        if (UserDefinedModels.Contains(modelName))
                        {
                            continue;
                        }

                        var (shortModelFilename, fullModelFileName, relativeModelName) = GetFileNames(modelName, modelDir ?? outputDir);
                        var modelExists = File.Exists(fullModelFileName);

                        if (!settings.Dump && modelExists && codeSettings.Overwrite == false)
                        {
                            Builder.DumpFormat("File {0} exists, overwrite is set to false, skipping ...", relativeModelName);
                            continue;
                        }
                        if (!settings.Dump && modelExists && settings.SkipIfExists != null && (
                            settings.SkipIfExists.Contains(modelName) ||
                            settings.SkipIfExists.Contains(shortModelFilename) ||
                            settings.SkipIfExists.Contains(relativeModelName))
                            )
                        {
                            Builder.DumpFormat("Skipping {0}, already exists ...", relativeModelName);
                            continue;
                        }
                        if (!settings.Dump && modelExists && settings.RoutinesAskOverwrite &&
                            Program.Ask($"File {relativeModelName} already exists, overwrite? [Y/N]", ConsoleKey.Y, ConsoleKey.N) == ConsoleKey.N)
                        {
                            Builder.DumpFormat("Skipping {0} ...", relativeModelName);
                            continue;
                        }
                        Builder.DumpRelativePath("Creating file: {0} ...", fullModelFileName);
                        Builder.WriteFile(fullModelFileName, modelModule.ToString());
                        modelModule.Flush();

                        if (code.UserDefinedModels.Contains(modelName))
                        {
                            UserDefinedModels.Add(modelName);
                        }
                    }
                }

                module.AddItems(code.Class);
                Builder.DumpRelativePath("Creating file: {0} ...", fullFileName);
                Builder.WriteFile(fullFileName, module.ToString());
                module.Flush();
            }
        }

        public List<ExtensionMethods> GetMethods()
        {
            var outputDir = GetOutputDir();
            var extensions = new List<ExtensionMethods>();

            foreach ((Code code, string _, string _, string fullFileName, string _, Module module) in GetCodes(outputDir))
            {
                if (code == null)
                {
                    continue;
                }
                if (code.Methods.Count > 0 && File.Exists(fullFileName))
                {
                    string modelNamespace = null;
                    if (code.Models.Any())
                    {
                        var modelModule = new Module(settings);
                        modelModule.AddNamespace((settings.ModelDir ?? codeSettings.OutputDir).PathToNamespace());
                        if (modelModule.Namespace != module.Namespace)
                        {
                            modelNamespace = modelModule.Namespace;
                        }
                    }
                    extensions.Add(new ExtensionMethods
                    { 
                        Methods = code.Methods,
                        Namespace = module.Namespace,
                        Name = code.Methods.First().Name,
                        ModelNamespace = modelNamespace
                    });
                }
            }
            return extensions;
        }

        protected abstract Module GetModule(Settings settings, CodeSettings codeSettings);

        protected abstract IEnumerable<(Code code, string name, string shortFilename, string fullFileName, string relative, Module module)>
            GetCodes(string outputDir);

        protected (string shortFilename, string fullFileName, string relative) GetFileNames(string name, string outputDir)
        {
            var shortFilename = string.Concat(name.ToUpperCamelCase(), ".cs");
            var fullFileName = Path.GetFullPath(Path.Join(outputDir, shortFilename));
            var relative = fullFileName.GetRelativePath();

            return (shortFilename, fullFileName, relative);
        }

        private string GetOutputDir()
        {
            var dir = Path.Combine(Program.CurrentDir, codeSettings.OutputDir);
            if (!Directory.Exists(dir))
            {
                Builder.DumpRelativePath("Creating dir: {0} ...", dir);
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private string GetModelDir()
        {
            var dir = settings.ModelDir;
            if (dir != null)
            {
                dir = Path.Combine(Program.CurrentDir, dir);
                if (!Directory.Exists(dir))
                {
                    Builder.DumpRelativePath("Creating dir: {0} ...", dir);
                    Directory.CreateDirectory(dir);
                }
            }
            return dir;
        }

        private void EmptyDir(string dir)
        {
            if (dir.PathEquals(Program.CurrentDir))
            {
                Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Cannot delete output dir that is project root. Combining RoutinesDeleteOutputDir or CrudOutputDir with CrudDeleteOutputDir or RoutinesDeleteOutputDir is not allowed. Don't ever try to delete your project root. Not a good idea. Ignoring these settings for now...");
            }
            else
            {
                if (!DirsEmptied.Contains(dir))
                {
                    DeleteDirFiles(dir);
                    DirsEmptied.Add(dir);
                }
            }
        }

        private static void DeleteDirFiles(string dir)
        {
            Builder.DumpRelativePath("Emptying dir of cs files: {0} ...", dir);
            if (Directory.GetFiles(dir, "*.cs").Length > 0)
            {
                foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                {
                    fi.Delete();
                }
            }
        }
    }
}
