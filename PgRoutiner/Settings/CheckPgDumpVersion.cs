using System;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{
    public partial class Settings
    {
        public static bool CheckPgDumpVersion(PgDumpBuilder builder)
        {
            var option = (typeof(Settings).GetField($"{builder.PgDumpName}Args").GetValue(null) as Arg).Original;
            var connVersion = builder.Connection.ServerVersion.Split(".").First();
            string fullDumpVersion;
            string dumpVersion;
            try
            {
                fullDumpVersion = builder.GetDumpVersion();
                dumpVersion = fullDumpVersion.Split(".").First();
            }
            catch(Exception e)
            {
                PgDumpError(connVersion, e, option);
                return false;
            }

            if (!string.Equals(connVersion, dumpVersion))
            {
                builder.SetPgDumpName(string.Format(Value.PgDumpFallback, connVersion));
                try
                {
                    fullDumpVersion = builder.GetDumpVersion();
                    dumpVersion = fullDumpVersion.Split(".").First();
                }
                catch (Exception e)
                {
                    PgDumpError(connVersion, e, option);
                    return false;
                }

                var value = typeof(Settings).GetProperty(builder.PgDumpName).GetValue(Value);
                if (!string.Equals(connVersion, dumpVersion))
                {
                    PgDumpMistmatch(builder.Connection, connVersion, fullDumpVersion, dumpVersion, value, option);
                    return false;
                }
                Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Using fall-back path for pg_dump: {value}. To remove this warning set the {option} setting to point to this path.");
            }
            return true;
        }

        private static void PgDumpMistmatch(NpgsqlConnection connection, string connVersion, string fullDumpVersion, string dumpVersion, 
            object value, string option)
        {
            Program.WriteLine("");
            Program.WriteLine(ConsoleColor.Red, "ERROR: It looks like pg_dump version mismatch: ");

            Program.Write(ConsoleColor.Red, "- Connection ");
            Program.Write(ConsoleColor.Red, Value.Connection);
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
            Program.Write(ConsoleColor.Red, "pg_dump");
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
            Program.Write(ConsoleColor.Red, "pg_dump");
            Program.Write(ConsoleColor.Red, " version ");
            Program.Write(ConsoleColor.Red, connVersion);
            Program.Write(ConsoleColor.Red, " and, the settings key ");
            Program.Write(ConsoleColor.Red, option);
            Program.WriteLine(ConsoleColor.Red, " points to the correct path.");
        }

        private static void PgDumpError(string connVersion, Exception e, string option)
        {
            Program.WriteLine("");
            Program.Write(ConsoleColor.Red, "ERROR - pg_dump returned an error: ");
            Program.WriteLine(ConsoleColor.Red, e.Message);

            Program.WriteLine(ConsoleColor.Red, "PostgreSQL might not be installed on your computer.");
            Program.WriteLine(ConsoleColor.Red,
                "You will not be able to create database dump files (schema file, data dump file or object tree dir), unless versions match!");
            Program.Write(ConsoleColor.Red, "Make sure you have access to ");
            Program.Write(ConsoleColor.Red, "pg_dump");
            Program.Write(ConsoleColor.Red, " version ");
            Program.Write(ConsoleColor.Red, connVersion);
            Program.Write(ConsoleColor.Red, " and, the settings key ");
            Program.Write(ConsoleColor.Red, option);
            Program.WriteLine(ConsoleColor.Red, " points to the correct path.");
        }
    }
}
