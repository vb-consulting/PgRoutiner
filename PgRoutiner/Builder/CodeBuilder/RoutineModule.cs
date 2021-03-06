﻿namespace PgRoutiner
{
    public class RoutineModule : Module
    {
        public RoutineModule(Settings settings, CodeSettings codeSettings, string schema) : base(settings)
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
                var dir = string.Format(codeSettings.OutputDir, schema == "public" ? "" : schema.ToUpperCamelCase());
                AddNamespace(dir.PathToNamespace().Replace("..", "."));
            }
        }
    }
}
