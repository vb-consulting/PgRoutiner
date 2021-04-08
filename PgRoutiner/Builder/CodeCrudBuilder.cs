using System;
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
                /*
                var shortFilename = string.Concat(name.ToUpperCamelCase(), ".cs");
                var fullFileName = Path.GetFullPath(Path.Join(outputDir, shortFilename));
                var relative = fullFileName.GetRelativePath();

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
                */
            }
            yield return (null, null, null, null, null, null);
        }
    }
}
