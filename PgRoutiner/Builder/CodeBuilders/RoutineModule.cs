namespace PgRoutiner.Builder.CodeBuilders;

public class RoutineModule : Module
{
    public RoutineModule(Current settings, CodeSettings codeSettings, string schema, string extraNamespace = null) : base(settings)
    {
        if (!settings.SkipAsyncMethods)
        {
            if (settings.RoutinesCancellationToken)
            {
                AddUsing("System.Threading");
            }
            AddUsing("System.Threading.Tasks");
        }
        AddUsing("NpgsqlTypes");
        AddUsing("Npgsql");
        if (settings.RoutinesCallerInfo || (settings.RoutinesCancellationToken && !settings.SkipAsyncMethods))
        {
            AddUsing("System.Runtime.CompilerServices");
        }
        if (!string.IsNullOrEmpty(codeSettings.OutputDir))
        {
            var dir = string.Format(codeSettings.OutputDir, schema == "public" ? "" : schema.ToUpperCamelCase());
            if (extraNamespace != null)
            {
                AddNamespace(dir.PathToNamespace().Replace("..", "."), extraNamespace);
            }
            else
            {
                AddNamespace(dir.PathToNamespace().Replace("..", "."));
            }
        }
    }
}
