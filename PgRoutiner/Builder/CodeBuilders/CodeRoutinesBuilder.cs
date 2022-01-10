namespace PgRoutiner.Builder.CodeBuilders;

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
            var schema = group.Key.Schema;
            var module = new RoutineModule(settings, codeSettings, group.Key.Schema);
            Code code;
            try
            {
                code = new RoutineCode(settings, name, schema, module.Namespace, group, connection);
            }
            catch (ArgumentException e)
            {
                Writer.Error($"Code for routine {name} could not be generated. {e.Message}");
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
