﻿using System;

namespace PgRoutiner
{
    public partial class ConnectionManager
    {
        private string GetConnectionString()
        {
            Console.WriteLine();
            var server = GetServer();
            var port = GetPort();
            var database = GetDatabase();
            var user = GetUser();
            var pass = GetPassword();
            return $"{server}{database}{port}{user}{pass}";
        }

        private string GetUser()
        {
            var env = Environment.GetEnvironmentVariable("PGUSER");
            var msg = "";
            if (env != null)
            {
                msg = " (empty for PG env. var.)";
            }
            Console.Write($"{name2} user{msg}: ");
            var result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                result = env;
            }
            return $"User Id={result};";
        }

        private string GetServer()
        {;
            var env = Environment.GetEnvironmentVariable("PGHOST");
            if (env == null)
            {
                env = Environment.GetEnvironmentVariable("PGSERVER");
            }
            string msg;
            if (env != null)
            {
                msg = " (empty for PG env. var.)";;
            }
            else
            {
                msg = " (empty for localhost)";
                env = "localhost";
            }
            Console.Write($"{name2} server{msg}: ");
            var result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                result = env;
            }
            return $"Server={result};";
        }

        private string GetPort()
        {
            var env = Environment.GetEnvironmentVariable("PGPORT");
            string msg;
            if (env != null)
            {
                msg = " (empty for PG env. var.)";
            }
            else
            {
                msg = " (empty for 5432)";
                env = "5432";
            }
            Console.Write($"{name2} port{msg}: ");
            var result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                result = env;
            }
            return $"Port={result};";
        }

        private string GetDatabase()
        {
            var env = Environment.GetEnvironmentVariable("PGDATABASE");
            if (env == null)
            {
                env = Environment.GetEnvironmentVariable("PGDB");
            }
            var msg = "";
            if (env != null)
            {
                msg = " (empty for PG env. var.)"; ;
            }
            Console.Write($"{name2} database{msg}: ");
            var result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                result = env;
            }
            return $"Db={result};";
        }

        private string GetPassword()
        {
            var env = Environment.GetEnvironmentVariable("PGPASSWORD");
            var msg = "";
            if (env != null)
            {
                msg = " (empty for PG env. var.)"; ;
            }
            Console.Write($"{name2} password{msg}: ");
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
            Console.WriteLine();
            return $"Password={pass};";
        }
    }
}
