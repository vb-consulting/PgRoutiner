﻿namespace PgRoutiner.Connection;

public partial class ConnectionManager
{
    public static string EnvPgUser => Environment.GetEnvironmentVariable("PGUSER");
    public static string EnvPgHost => Environment.GetEnvironmentVariable("PGHOST") ?? Environment.GetEnvironmentVariable("PGSERVER");
    public static string EnvPgPort => Environment.GetEnvironmentVariable("PGPORT");
    public static string EnvPgDb => Environment.GetEnvironmentVariable("PGDATABASE") ?? Environment.GetEnvironmentVariable("PGDB");
    public static string EnvPgPass => Environment.GetEnvironmentVariable("PGPASSWORD") ?? Environment.GetEnvironmentVariable("PGPASS");

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

    private string GetUser(bool skipPrompt = false)
    {
        var env = EnvPgUser;
        
        if (!string.IsNullOrEmpty(env) && skipPrompt)
        {
            return $"User Id={env};";
        }

        if (env == null)
        {
            env = "postgres";
        }
        Console.Write($"{name2} user [{env}]: ");

        var result = Console.ReadLine();
        if (string.IsNullOrEmpty(result))
        {
            result = env;
        }
        return $"User Id={result};";
    }

    private string GetServer(bool skipPrompt = false)
    {
        var env = EnvPgHost;
        if (!string.IsNullOrEmpty(env) && skipPrompt)
        {
            return $"Server={env};";
        }

        if (env == null)
        {
            env = "localhost";
        }
        Console.Write($"{name2} server [{env}]: ");

        var result = Console.ReadLine();
        if (string.IsNullOrEmpty(result))
        {
            result = env;
        }
        return $"Server={result};";
    }

    private string GetPort(bool skipPrompt = false)
    {
        var env = EnvPgPort;
        if (!string.IsNullOrEmpty(env) && skipPrompt)
        {
            return $"Port={env};";
        }

        if (env == null)
        {
            env = "5432";
        }
        Console.Write($"{name2} port [{env}]: ");

        var result = Console.ReadLine();
        if (string.IsNullOrEmpty(result))
        {
            result = env;
        }
        return $"Port={result};";
    }

    private string GetDatabase(bool skipPrompt = false)
    {
        var env = EnvPgDb;
        if (!string.IsNullOrEmpty(env) && skipPrompt)
        {
            return $"Db={env};";
        }

        if (env == null)
        {
            env = "postgres";
        }
        Console.Write($"{name2} database [{env}]: ");

        var result = Console.ReadLine();
        if (string.IsNullOrEmpty(result))
        {
            result = env;
        }
        return $"Db={result};";
    }

    private string GetPassword(bool skipPrompt = false)
    {
        var env = EnvPgPass;
        if (!string.IsNullOrEmpty(env) && skipPrompt)
        {
            return $"Password={env};";
        }

        if (env != null)
        {
            Console.Write($"{name2} password [environment var.]: ");
        }
        else
        {
            Console.Write($"{name2} password: ");
        }
        var pass = string.Empty;

        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && pass.Length > 0)
            {
                pass = pass[0..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                pass += keyInfo.KeyChar;
                Console.Write("*");
            }
        } while (key != ConsoleKey.Enter);

        Console.WriteLine();
        if (string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(env))
        {
            return $"Password={env};";
        }
        return $"Password={pass};";
    }
}
