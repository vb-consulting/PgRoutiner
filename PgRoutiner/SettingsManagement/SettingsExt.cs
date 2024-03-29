﻿namespace PgRoutiner.SettingsManagement
{
    public static class SettingsExt
    {
        public static string GetPgDumpFallback(this Current settings)
        {
            if (settings.PgDumpFallback != null)
            {
                return settings.PgDumpFallback;
            }
            return OperatingSystem.IsWindows() ?
                "C:\\Program Files\\PostgreSQL\\{0}\\bin\\pg_dump.exe" :
                "/usr/lib/postgresql/{0}/bin/pg_dump";
        }

        public static string GetPgRestoreFallback(this Current settings)
        {
            if (settings.PgRestoreFallback != null)
            {
                return settings.PgRestoreFallback;
            }
            return OperatingSystem.IsWindows() ?
                "C:\\Program Files\\PostgreSQL\\{0}\\bin\\pg_restore.exe" :
                "/usr/lib/postgresql/{0}/bin/pg_restore";
        }

        public static string GetPsqlFallback(this Current settings)
        {
            if (settings.PsqlFallback != null)
            {
                return settings.PsqlFallback;
            }
            return OperatingSystem.IsWindows() ?
                "C:\\Program Files\\PostgreSQL\\{0}\\bin\\psql.exe" :
                "/usr/lib/postgresql/{0}/bin/psql";
        }
    }
}
