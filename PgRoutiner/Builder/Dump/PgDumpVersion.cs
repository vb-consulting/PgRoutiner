﻿namespace PgRoutiner.Builder.Dump;

public static class PgDumpVersion
{
    public static bool Check(PgDumpBuilder builder, bool restore = false)
    {
        var option = (typeof(Current).GetField(name: $"{(restore ? nameof(Current.PgRestore) : nameof(Current.PgDump))}Args").GetValue(null) as Arg).Original;
        var connVersion = builder.Connection.ServerVersion.Split(".").First();
        string fullDumpVersion;
        string dumpVersion;
        try
        {
            fullDumpVersion = builder.GetDumpVersion(restore);
            dumpVersion = fullDumpVersion.Split(".").First();
        }
        catch (Exception e)
        {
            PgDumpError(connVersion, e, option, restore: restore);
            return false;
        }

        if (!string.Equals(connVersion, dumpVersion))
        {
            builder.SetPgDumpName(string.Format(restore ? Current.Value.GetPgRestoreFallback() : Current.Value.GetPgDumpFallback(), connVersion));
            try
            {
                fullDumpVersion = builder.GetDumpVersion(restore);
                dumpVersion = fullDumpVersion.Split(".").First();
            }
            catch (Exception e)
            {
                PgDumpError(connVersion, e, option, restore: restore);
                return false;
            }

            var value = typeof(Current).GetProperty(builder.PgDumpName).GetValue(Current.Value);
            if (!string.Equals(connVersion, dumpVersion))
            {
                PgDumpMistmatch(builder.Connection, connVersion, fullDumpVersion, dumpVersion, value, option, restore: restore);
                return false;
            }
            if (Current.Value.Verbose) Program.WriteLine(ConsoleColor.Yellow, "",
                $"WARNING: Using fall-back path for {(restore ? "pg_restore" : "pg_dump")}: {value}. To remove this warning set the {option} setting to point to this path.",
                "");
        }
        return true;
    }

    private static void PgDumpMistmatch(NpgsqlConnection connection, string connVersion, string fullDumpVersion, string dumpVersion,
        object value, string option, bool restore)
    {
        Program.WriteLine("");
        Program.WriteLine(ConsoleColor.Red, $"ERROR: It looks like {(restore ? "pg_restore" : "pg_dump")} version mismatch: ");

        Program.Write(ConsoleColor.Red, "- Connection ");
        Program.Write(ConsoleColor.Red, Current.Value.Connection);
        Program.Write(ConsoleColor.Red, " (server ");
        Program.Write(ConsoleColor.Red, connection.Host);
        Program.Write(ConsoleColor.Red, ", port ");
        Program.Write(ConsoleColor.Red, connection.Port.ToString());
        Program.Write(ConsoleColor.Red, ", database ");
        Program.Write(ConsoleColor.Red, connection.Database);
        Program.Write(ConsoleColor.Red, ") is version ");
        Program.Write(ConsoleColor.Red, connVersion);
        Program.Write(ConsoleColor.Red, " (");
        Program.Write(ConsoleColor.Red, connection.ServerVersion);
        Program.WriteLine(ConsoleColor.Red, ")");

        Program.Write(ConsoleColor.Red, "- Default ");
        Program.Write(ConsoleColor.Red, $"{(restore ? "pg_restore" : "pg_dump")}");
        Program.Write(ConsoleColor.Red, " path (");
        Program.Write(ConsoleColor.Red, $"{value}");
        Program.Write(ConsoleColor.Red, ") is version ");
        Program.Write(ConsoleColor.Red, dumpVersion);
        Program.Write(ConsoleColor.Red, " (");
        Program.Write(ConsoleColor.Red, fullDumpVersion);
        Program.WriteLine(ConsoleColor.Red, ")");

        Program.WriteLine(ConsoleColor.Red,
            "You will not be able to create database dump files (schema file, data dump files, object tree dir or schema diff), unless versions match!");
        Program.Write(ConsoleColor.Red, "Make sure you have access to ");
        Program.Write(ConsoleColor.Red, $"{(restore ? "pg_restore" : "pg_dump")}");
        Program.Write(ConsoleColor.Red, " version ");
        Program.Write(ConsoleColor.Red, connVersion);
        Program.Write(ConsoleColor.Red, " and, the settings key ");
        Program.Write(ConsoleColor.Red, option);
        Program.WriteLine(ConsoleColor.Red, " points to the correct path.");
    }

    private static void PgDumpError(string connVersion, Exception e, string option, bool restore)
    {
        Program.WriteLine("");
        Program.Write(ConsoleColor.Red, $"ERROR - {(restore ? "pg_restore" : "pg_dump")} returned an error: ");
        Program.WriteLine(ConsoleColor.Red, e.Message);

        Program.WriteLine(ConsoleColor.Red, "PostgreSQL might not be installed on your computer.");
        Program.WriteLine(ConsoleColor.Red,
            "You will not be able to create database dump files (schema file, data dump file or object tree dir), unless versions match!");
        Program.Write(ConsoleColor.Red, "Make sure you have access to ");
        Program.Write(ConsoleColor.Red, $"{(restore ? "pg_restore" : "pg_dump")}");
        Program.Write(ConsoleColor.Red, " version ");
        Program.Write(ConsoleColor.Red, connVersion);
        Program.Write(ConsoleColor.Red, " and, the settings key ");
        Program.Write(ConsoleColor.Red, option);
        Program.WriteLine(ConsoleColor.Red, " points to the correct path.");
    }
}
