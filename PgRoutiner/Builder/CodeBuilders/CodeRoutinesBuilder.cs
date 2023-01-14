using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.CodeBuilders;

public class CodeRoutinesBuilder : CodeBuilder
{
    public static Dictionary<string, string> RoutinesCustomDirs = new();
    
    public CodeRoutinesBuilder(NpgsqlConnection connection, Current settings, CodeSettings codeSettings) :
        base(connection, settings, codeSettings)
    {
        //AddEntry(nameof(Settings.RoutinesSchemaSimilarTo), Settings.Value.Routines);
        //AddEntry(nameof(Settings.RoutinesSchemaNotSimilarTo), Settings.Value.Routines);
    }

    protected override IEnumerable<CodeResult> GetCodes()
    {
        foreach (var group in connection.GetRoutineGroups(settings, all: false, schemaSimilarTo: settings.RoutinesSchemaSimilarTo, schemaNotSimilarTo: settings.RoutinesSchemaNotSimilarTo))
        {
            var name = group.Key.Name;
            var schema = group.Key.Schema;

            string extraNamespace = null;
            if (settings.RoutinesCustomDirs != null)
            {
                foreach (var ns in settings.RoutinesCustomDirs)
                {
                    if (this.connection.WithParameters(name, ns.Key).Read<bool>("select $1 similar to $2").Single())
                    {
                        extraNamespace = ns.Value.PathToNamespace().Replace("..", ".");
                        RoutinesCustomDirs.Add(name, ns.Value);
                        break;
                    }
                }
            }

            var module = new RoutineModule(settings, codeSettings, group.Key.Schema, extraNamespace);
            Code code;
            try
            {
                List<PgRoutineGroup> routines = group.ToList();
                foreach (var routine in group)
                {
                    foreach(var parameter in routine.Parameters)
                    {
                        if (parameter.Default != null)
                        {
                            var newRoutine = routine with { Parameters = routine.Parameters.Where(p => p.Name != parameter.Name).ToList() };
                            routines.Add(newRoutine);
                        }
                    }
                }
                code = new RoutineCode(settings, name, schema, module.Namespace, routines, connection);
            }
            catch (ArgumentException e)
            {
                Writer.Error($"Code for routine {schema}.{name} could not be generated. {e.Message}");
                continue;
            }
            yield return new CodeResult
            {
                Code = code,
                Name = name,
                Module = module,
                Schema = schema
            };
        }
    }
}
