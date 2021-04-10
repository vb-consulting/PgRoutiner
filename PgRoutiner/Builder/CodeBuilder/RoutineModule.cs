namespace PgRoutiner
{
    public class RoutineModule : Module
    {
        public RoutineModule(Settings settings, CodeSettings codeSettings) : base(settings)
        {
            if (!settings.SkipAsyncMethods)
            {
                AddUsing("System.Threading.Tasks");
            }
            AddUsing("Norm");
            AddUsing("NpgsqlTypes");
            AddUsing("Npgsql");
            if (!string.IsNullOrEmpty(codeSettings.OutputDir))
            {
                AddNamespace(codeSettings.OutputDir.PathToNamespace());
            }
        }
    }
}
