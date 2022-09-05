namespace PgRoutiner.Builder.CodeBuilders;

public class RoutineModule : Module
{
    public RoutineModule(Current settings, CodeSettings codeSettings, string schema) : base(settings)
    {
        if (!settings.SkipAsyncMethods)
        {
            AddUsing("System.Threading.Tasks");
        }
        AddUsing("Norm");
        AddUsing("NpgsqlTypes");
        AddUsing("Npgsql");
        if (settings.RoutinesCallerInfo)
        {
            AddUsing("System.Runtime.CompilerServices");
        }
        if (!string.IsNullOrEmpty(codeSettings.OutputDir))
        {
            var dir = string.Format(codeSettings.OutputDir, schema == "public" ? "" : schema.ToUpperCamelCase());
            AddNamespace(dir.PathToNamespace().Replace("..", "."));
        }
    }
}
