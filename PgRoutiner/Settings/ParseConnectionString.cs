using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PgRoutiner
{
    partial class Settings
    {
        public static NpgsqlConnection ParseConnectionString(IConfigurationRoot config)
        {
            if (!string.IsNullOrEmpty(Value.Connection))
            {
                var connectionStr = config.GetConnectionString(Value.Connection);
                if (string.IsNullOrEmpty(connectionStr))
                {
                    Program.DumpError($"Connection name {Value.Connection} could not be found in any of the setting files, exiting...");
                    return null;
                }
                connectionStr = ParseConnString(connectionStr);
                if (connectionStr == null)
                {
                    return null;
                }
                try
                {
                    return CreateAndOpen(connectionStr);
                }
                catch (Exception e)
                {
                    Program.DumpError($"Could not open {Value.Connection}.{Environment.NewLine}{e.Message}{Environment.NewLine}Exiting...");
                    return null;
                }
            }
            else
            {
                if (config.GetSection("ConnectionStrings").GetChildren().Any())
                {
                    Value.Connection = config.GetSection("ConnectionStrings").GetChildren().First().Key;
                    var connectionStr = ParseConnString(config.GetConnectionString(Value.Connection));
                    if (connectionStr == null)
                    {
                        return null;
                    }
                    try
                    {
                        return CreateAndOpen(connectionStr);
                    }
                    catch (Exception e)
                    {
                        Program.DumpError($"Could not open {Value.Connection}.{Environment.NewLine}{e.Message}{Environment.NewLine}Exiting...");
                        return null;
                    }
                }
                else
                {
                    var connectionStr = GetConnectionString();
                    try
                    {
                        return CreateAndOpen(connectionStr);
                    }
                    catch (Exception e)
                    {
                        Program.DumpError($"Could not open {connectionStr}.{Environment.NewLine}{e.Message}{Environment.NewLine}Exiting...");
                        return null;
                    }
                }
            }
        }

        private static NpgsqlConnection CreateAndOpen(string connectionStr)
        {
            var conn = new NpgsqlConnection(connectionStr);
            conn.Open();
            if (!string.IsNullOrEmpty(connectionStr))
            {
                Program.WriteLine("");
                Program.Write("Using connection ");
                Program.Write(ConsoleColor.Yellow, Value.Connection);
                Program.Write(":");
                Program.WriteLine("");
                Program.WriteLine(ConsoleColor.Cyan, " " + conn.ConnectionString);
            }
            else
            {
                Program.WriteLine("");
                Program.WriteLine("Using connection ");
                Program.WriteLine(ConsoleColor.Cyan, " " + conn.ConnectionString);
            }
            return conn;
        }
 
        private static string ParseConnString(string connectionStr)
        {
            string user = null;
            string pass = null;
            string server = null;
            string port = null;
            string database = null;
            try
            {
                if (connectionStr.StartsWith("postgresql://"))
                {
                    connectionStr = connectionStr.Remove(0, "postgresql://".Length);
                    var parts = connectionStr.Split('@');
                    var first = parts.First();
                    var second = parts.Last();
                    var firstParts = first.Split(':');
                    user = firstParts.First();
                    pass = firstParts.Last();
                    var secondParts = second.Split('/');
                    var host = secondParts.First();
                    database = secondParts.Last();
                    var hostParts = host.Split(':');
                    server = hostParts.First();
                    port = hostParts.Last();
                }
                else
                {
                    foreach (var part in connectionStr.Split(';'))
                    {
                        var parts = part.Split('=', 2);
                        var first = parts.First().ToLower();
                        var second = parts.Last();
                        if (string.IsNullOrEmpty(first))
                        {
                            continue;
                        }
                        if (string.Equals(first, "user id") || string.Equals(first, "user") || string.Equals(first, "username"))
                        {
                            user = second;
                            continue;
                        }
                        if (string.Equals(first, "password"))
                        {
                            pass = second;
                            continue;
                        }
                        if (string.Equals(first, "server") || string.Equals(first, "host"))
                        {
                            server = second;
                            continue;
                        }
                        if (string.Equals(first, "port"))
                        {
                            port = second;
                            continue;
                        }
                        if (string.Equals(first, "db") || string.Equals(first, "database"))
                        {
                            database = second;
                            continue;
                        }
                    }
                }
            }
            catch
            {
                Program.DumpError($"Connection string \"{connectionStr}\" is malformed.");
                return null;
            }
            if (string.IsNullOrEmpty(server))
            {
                server = GetServer();
            }
            if (string.IsNullOrEmpty(port))
            {
                port = GetPort();
            }
            if (string.IsNullOrEmpty(database))
            {
                database = GetDatabase();
            }
            if (string.IsNullOrEmpty(user))
            {
                user = GetUser();
            }
            if (string.IsNullOrEmpty(pass))
            {
                pass = GetPassword();
            }
            return $"Server={server};Db={database};Port={port};User Id={user};Password={pass};";
        }

        private static string GetConnectionString()
        {
            var server = GetServer();
            var port = GetPort();
            var database = GetDatabase();
            var user = GetUser();
            var pass = GetPassword();
            return $"Server={server};Db={database};Port={port};User Id={user};Password={pass};";
        }

        private static string GetUser()
        {
            var env = Environment.GetEnvironmentVariable("PGUSER");
            if (env != null)
            {
                return env;
            }
            Console.WriteLine();
            Console.WriteLine("User:");
            return Console.ReadLine();
        }

        private static string GetServer()
        {
            var env = Environment.GetEnvironmentVariable("PGHOST");
            if (env != null)
            {
                return env;
            }
            env = Environment.GetEnvironmentVariable("PGSERVER");
            if (env != null)
            {
                return env;
            }
            Console.WriteLine();
            Console.WriteLine("Server:");
            return Console.ReadLine();
        }

        private static string GetPort()
        {
            var env = Environment.GetEnvironmentVariable("PGPORT");
            if (env != null)
            {
                return env;
            }
            Console.WriteLine();
            Console.WriteLine("Port:");
            return Console.ReadLine();
        }

        private static string GetDatabase()
        {
            var env = Environment.GetEnvironmentVariable("PGDATABASE");
            if (env != null)
            {
                return env;
            }
            env = Environment.GetEnvironmentVariable("PGDB");
            if (env != null)
            {
                return env;
            }
            Console.WriteLine();
            Console.WriteLine("Database:");
            return Console.ReadLine();
        }

        private static string GetPassword()
        {
            var env = Environment.GetEnvironmentVariable("PGPASSWORD");
            if (env != null)
            {
                return env;
            }
            Console.WriteLine();
            Console.WriteLine("Password:");
            var pass = string.Empty;
            Console.CursorVisible = false;
            try
            {
                ConsoleKey key;
                do
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass[0..^1];
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        pass += keyInfo.KeyChar;
                    }
                } while (key != ConsoleKey.Enter);
            }
            finally
            {
                Console.CursorVisible = true;
            }
            return pass;
        }
    }
}
