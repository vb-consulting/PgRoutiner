﻿namespace PgRoutiner.SettingsManagement
{
    public class CodeSettings
    {
        public bool Enabled { get; set; }
        public string OutputDir { get; set; }
        public bool EmptyOutputDir { get; set; }
        public bool Overwrite { get; set; }
        public bool AskOverwrite { get; set; }

        public static CodeSettings ToRoutineSettings(Current settings)
        {
            return new CodeSettings
            {
                Enabled = settings.Routines,
                OutputDir = settings.OutputDir,
                EmptyOutputDir = settings.RoutinesEmptyOutputDir,
                //Overwrite = settings.RoutinesOverwrite,
                //AskOverwrite = settings.RoutinesAskOverwrite
                Overwrite = settings.Overwrite,
                AskOverwrite = settings.AskOverwrite
            };
        }
    }
}
