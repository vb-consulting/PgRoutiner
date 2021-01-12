namespace PgRoutiner
{
    public class RoutineModule : Module
    {
        public RoutineModule(Settings settings) : base(settings)
        {
            if (settings.AsyncMethod)
            {
                AddUsing("System.Threading.Tasks");
            }
            AddUsing("Norm");
            AddUsing("NpgsqlTypes");
            AddUsing("Npgsql");
            if (!string.IsNullOrEmpty(settings.OutputDir))
            {
                AddNamespace(settings.OutputDir.PathToNamespace());
            }
        }
    }
}
