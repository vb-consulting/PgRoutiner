using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    public class CodeRoutinesBuilder : CodeBuilder
    {
        public CodeRoutinesBuilder(NpgsqlConnection connection, Settings settings, CodeSettings codeSettings) : 
            base(connection, settings, codeSettings)
        {
        }

        protected override Module GetModule(Settings settings, CodeSettings codeSettings) => new RoutineModule(settings, codeSettings);

        protected override IEnumerable<(Code code, string name, string shortFilename, string fullFileName, string relative, Module module)> 
            GetCodes(string outputDir)
        {
            foreach (var group in connection.GetRoutineGroups(settings, all: false))
            {
                var name = group.Key;
                var (shortFilename, fullFileName, relative) = GetFileNames(name, outputDir);

                var module = GetModule(settings, codeSettings);
                Code code;
                try
                {
                    code = new RoutineCode(settings, name, module.Namespace, group, connection);
                }
                catch (ArgumentException e)
                {
                    Builder.Error($"File {relative} could not be generated. {e.Message}");
                    continue;
                }
                yield return (code, name, shortFilename, fullFileName, relative, module);
            }
        }
    }
}
