﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    public class CodeCrudBuilder : CodeBuilder
    {
        public CodeCrudBuilder(NpgsqlConnection connection, Settings settings, CodeSettings codeSettings) :
            base(connection, settings, codeSettings)
        {
        }

        protected override Module GetModule(Settings settings, CodeSettings codeSettings) => new RoutineModule(settings, codeSettings);

        protected override IEnumerable<(Code code, string name, string shortFilename, string fullFileName, string relative, Module module)>
            GetCodes(string outputDir)
        {
            foreach (var group in connection.GetTableDefintions(settings))
            {
                var (schema, name) = group.Key;
                var module = GetModule(settings, codeSettings);
                string shortFilename = null, fullFileName = null, relative = null;
                Code code = null;
                if (OptionContains(settings.CrudRead, schema, name))
                {
                    try
                    {
                        if (OptionContains(settings.CrudRead, schema, name))
                        {
                            (shortFilename, fullFileName, relative) = GetFileNames($"{name}_read", outputDir);
                            code = new CrudReadCode(settings, group.Key, module.Namespace, group);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Builder.Error($"File {relative} could not be generated. {e.Message}");
                        continue;
                    }
                    yield return (code, name, shortFilename, fullFileName, relative, module);
                }
            }
            //Program.WriteLine(ConsoleColor.Yellow, "", $"WARNING: Table {schema}.{name} not found, skipping...");
        }

        public static bool OptionContains(HashSet<string> option, string schema, string name)
        {
            return option.Contains(name) || 
                option.Contains($"{schema}.{name}") || 
                option.Contains($"\"{schema}\".\"{name}\"") ||
                option.Contains($"{schema}.\"{name}\"") ||
                option.Contains($"\"{schema}\".{name}");
        }
    }
}
