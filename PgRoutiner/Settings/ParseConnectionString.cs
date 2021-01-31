using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                    var connectionStr = GetConnStringFromConsole();
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
                server = GetServerFromConsole();
            }
            if (string.IsNullOrEmpty(port))
            {
                port = GetPortFromConsole();
            }
            if (string.IsNullOrEmpty(database))
            {
                database = GetDatabaseFromConsole();
            }
            if (string.IsNullOrEmpty(user))
            {
                user = GetUserFromConsole();
            }
            if (string.IsNullOrEmpty(pass))
            {
                pass = GetPasswordFromConsole();
            }
            return $"Server={server};Db={database};Port={port};User Id={user};Password={pass};";
        }

        private static string GetConnStringFromConsole()
        {
            var server = GetServerFromConsole();
            var port = GetPortFromConsole();
            var database = GetDatabaseFromConsole();
            var user = GetUserFromConsole();
            var pass = GetPasswordFromConsole();
            return $"Server={server};Db={database};Port={port};User Id={user};Password={pass};";
        }

        private static string GetUserFromConsole()
        {
            Console.WriteLine();
            Console.WriteLine("User:");
            return Console.ReadLine();
        }

        private static string GetServerFromConsole()
        {
            Console.WriteLine();
            Console.WriteLine("Server:");
            return Console.ReadLine();
        }

        private static string GetPortFromConsole()
        {
            Console.WriteLine();
            Console.WriteLine("Port:");
            return Console.ReadLine();
        }

        private static string GetDatabaseFromConsole()
        {
            Console.WriteLine();
            Console.WriteLine("Database:");
            return Console.ReadLine();
        }

        private static string GetPasswordFromConsole()
        {
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
