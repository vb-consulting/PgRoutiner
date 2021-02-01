using System;
using System.Linq;
using Npgsql;

namespace PgRoutiner
{

    public partial class Settings
    {
        public static bool CheckPgDumpVersion(NpgsqlConnection connection, PgDumpBuilder builder, bool ask = true)
        {
            var connVersion = connection.ServerVersion.Split(".").First();
            string fullDumpVersion;
            string dumpVersion;
            try
            {
                fullDumpVersion = builder.GetDumpVersion();
                dumpVersion = fullDumpVersion.Split(".").First();
            }
            catch(Exception e)
            {
                Program.WriteLine("");
                Program.Write(ConsoleColor.Red, "ERROR - It seems that pg_dump command: ");
                Program.WriteLine(ConsoleColor.Red, e.Message, "");

                Program.WriteLine(ConsoleColor.Yellow, "PostgreSQL might not be installed on your computer.", "");
                Program.Write(ConsoleColor.Yellow, "Make sure you have access to pg_dump version ");
                Program.Write(ConsoleColor.Cyan, connVersion);
                Program.Write(ConsoleColor.Yellow, " and, the settings key ");
                Program.Write(ConsoleColor.Cyan, PgDumpArgs.Original);
                Program.Write(ConsoleColor.Yellow, " (command line ");
                Program.Write(ConsoleColor.Cyan, PgDumpArgs.Alias);
                Program.Write(ConsoleColor.Yellow, ", ");
                Program.Write(ConsoleColor.Cyan, PgDumpArgs.Name);
                Program.WriteLine(ConsoleColor.Yellow, ") point to correct path.");
                Program.Write(ConsoleColor.Yellow, "Current path is ");
                Program.WriteLine(ConsoleColor.Cyan, Value.PgDump, "");

                return false;
            }
            
            if (!string.Equals(connVersion, dumpVersion))
            {
                Program.WriteLine("");
                Program.WriteLine(ConsoleColor.Red, "ERROR: It looks like version mismatch: ", "");
                
                Program.Write(ConsoleColor.Yellow, "- Connection ");
                Program.Write(ConsoleColor.Cyan, Value.Connection);
                Program.Write(ConsoleColor.Yellow, " (server ");
                Program.Write(ConsoleColor.Cyan, connection.Host);
                Program.Write(ConsoleColor.Yellow, ", port ");
                Program.Write(ConsoleColor.Cyan, connection.Port.ToString());
                Program.Write(ConsoleColor.Yellow, ", database ");
                Program.Write(ConsoleColor.Cyan, connection.Database);
                Program.Write(ConsoleColor.Yellow, ") is version ");
                Program.Write(ConsoleColor.Cyan, connVersion);
                Program.Write(ConsoleColor.Yellow, " (");
                Program.Write(ConsoleColor.Cyan, connection.ServerVersion);
                Program.WriteLine(ConsoleColor.Yellow, ")");

                Program.Write(ConsoleColor.Yellow, "- Default pg_dump path (");
                Program.Write(ConsoleColor.Cyan, Value.PgDump);
                Program.Write(ConsoleColor.Yellow, ") is version ");
                Program.Write(ConsoleColor.Cyan, dumpVersion);
                Program.Write(ConsoleColor.Yellow, " (");
                Program.Write(ConsoleColor.Cyan, fullDumpVersion);
                Program.WriteLine(ConsoleColor.Yellow, ")");

                Program.WriteLine(ConsoleColor.Red, "",
                    "You will not be able to create database dump files (schema file, data dump file or object tree dir), unless versions match!",
                    "");

                if (ask)
                {
                    var pgDump = Program.ReadLine($"Path to pg_dump version {connVersion} executable file: ",

                        $"Please provide a correct path to the pg_dump for version {connVersion}.",
                        $" - On windows systems that might be C:\\Program Files\\PostgreSQL\\{connVersion}\\bin\\pg_dump.exe",
                        $" - On linux systems, that might be /usr/lib/postgresql/{connVersion}/bin/pg_dump",
                        $" - You can always change this value in configuration file under \"{nameof(PgDump)}\" settings key.",
                        " - Leave this path empty and press enter to continue using the default setting.");

                    if (pgDump != null)
                    {
                        Value.PgDump = pgDump;
                        return true;
                    }
                }
                else
                {
                    Program.Write(ConsoleColor.Yellow, "Please provide a correct path to the pg_dump for version ");
                    Program.Write(ConsoleColor.Cyan, connVersion);
                    Program.Write(ConsoleColor.Yellow, " by setting configuration value key ");
                    Program.Write(ConsoleColor.Cyan, PgDumpArgs.Original);
                    Program.Write(ConsoleColor.Yellow, " (command line ");
                    Program.Write(ConsoleColor.Cyan, PgDumpArgs.Alias);
                    Program.Write(ConsoleColor.Yellow, ", ");
                    Program.Write(ConsoleColor.Cyan, PgDumpArgs.Name);
                    Program.WriteLine(ConsoleColor.Yellow, ") point to correct path.");
                    Program.Write(ConsoleColor.Yellow, "Current path is ");
                    Program.WriteLine(ConsoleColor.Cyan, Value.PgDump, "");

                    Program.WriteLine(ConsoleColor.Yellow, "",
                        $" - On windows systems that might be C:\\Program Files\\PostgreSQL\\{connVersion}\\bin\\pg_dump.exe",
                        $" - On linux systems, that might be /usr/lib/postgresql/{connVersion}/bin/pg_dump", "");
                }
                return false;
            }
            return true;
        }
    }
}
