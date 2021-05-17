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

        protected override IEnumerable<CodeResult> GetCodes()
        {
            foreach (var group in connection.GetRoutineGroups(settings, all: false))
            {
                var name = group.Key.Name;
                var module = new RoutineModule(settings, codeSettings, group.Key.Schema);
                Code code;
                try
                {
                    code = new RoutineCode(settings, name, module.Namespace, group, connection);
                }
                catch (ArgumentException e)
                {
                    Builder.Error($"Code for routine {name} could not be generated. {e.Message}");
                    continue;
                }
                yield return new CodeResult
                { 
                    Code = code, 
                    Name = name,  
                    Module = module,
                    Schema = group.Key.Schema
                };
            }
        }
    }
}
