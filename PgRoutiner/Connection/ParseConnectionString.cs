﻿using Microsoft.Extensions.Configuration;

namespace PgRoutiner.Connection;

public partial class ConnectionManager
{
    public NpgsqlConnection ParseConnectionString()
    {
        if (!string.IsNullOrEmpty(name))
        {
            var connectionStr = config.GetConnectionString(name);
            if (string.IsNullOrEmpty(connectionStr))
            {
                connectionStr = ParseConnStringInternal(name);
            }
            else
            {
                connectionStr = ParseConnStringInternal(connectionStr);
            }

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
                Program.DumpError($"Could not open {name}{Environment.NewLine}{e.Message}{Environment.NewLine}{e.InnerException?.Message}{Environment.NewLine}{e.InnerException?.InnerException?.Message}");
                return null;
            }
        }
        else
        {
            var configKey = GetConnStringConfigKey();
            if (configKey != null)
            {
                typeof(Current).GetProperty(connectionKey).SetValue(Current.Value, configKey);
                var connectionStr = ParseConnStringInternal(config.GetConnectionString(configKey));
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
                    Program.DumpError($"Could not open {name}{Environment.NewLine}{e.Message}{Environment.NewLine}{e.InnerException?.Message}{Environment.NewLine}{e.InnerException?.InnerException?.Message}");
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
                    Program.DumpError($"Could not open {connectionStr}{Environment.NewLine}{e.Message}{Environment.NewLine}{e.InnerException?.Message}{Environment.NewLine}{e.InnerException?.InnerException?.Message}");
                    return null;
                }
            }
        }
    }

    private string GetConnStringConfigKey()
    {
        var section = config.GetSection("ConnectionStrings").GetChildren();
        if (!section.Any())
        {
            return null;
        }
        if (skipKey == null)
        {
            return section.First().Key;
        }
        foreach (var entry in section)
        {
            if (string.Equals(entry.Key, skipKey))
            {
                continue;
            }
            return entry.Key;
        }
        return null;
    }

    private string ParseConnStringInternal(string connectionStr)
    {
        string user = null;
        string pass = null;
        string server = null;
        string port = null;
        string database = null;
        string addition = null;
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

                if (!string.IsNullOrEmpty(server))
                {
                    server = $"Server={server};";
                }
                else
                {
                    if (EnvPgHost != null)
                    {
                        server = $"Server={EnvPgHost};";
                    }
                }
                
                if (!string.IsNullOrEmpty(port))
                {
                    port = $"Port={port};";
                }
                else
                {
                    if (EnvPgPort != null)
                    {
                        port = $"Port={EnvPgPort};";
                    }
                }

                
                if (!string.IsNullOrEmpty(database))
                {
                    database = $"Db={database};";
                }
                else
                {
                    if (EnvPgDb != null)
                    {
                        database = $"Db={EnvPgDb};";
                    }
                }

                if (!string.IsNullOrEmpty(user))
                {
                    user = $"User Id={user};";
                }
                else
                {
                    if (EnvPgUser != null)
                    {
                        user = $"User Id={EnvPgUser};";
                    }
                }
                if (!string.IsNullOrEmpty(pass))
                {
                    pass = $"Password={pass};";
                }
                else
                {
                    if (EnvPgPass != null)
                    {
                        pass = $"Password={EnvPgPass};";
                    }
                }
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
                        if (string.IsNullOrEmpty(second))
                        {
                            user = $"User Id={EnvPgUser};";
                        } 
                        else
                        {
                            user = $"User Id={second};";
                        }
                        continue;
                    }
                    if (string.Equals(first, "password"))
                    {
                        if (string.IsNullOrEmpty(second))
                        {
                            pass = $"Password={EnvPgPass};";
                        }
                        else
                        {
                            pass = $"Password={second};";
                        }
                        continue;
                    }
                    if (string.Equals(first, "server") || string.Equals(first, "host"))
                    {
                        if (string.IsNullOrEmpty(second))
                        {
                            server = $"Server={EnvPgHost};";
                        }
                        else
                        {
                            server = $"Server={second};";
                        }
                        continue;
                    }
                    if (string.Equals(first, "port"))
                    {
                        if (string.IsNullOrEmpty(second))
                        {
                            port = $"Port={EnvPgPort};";
                        }
                        else
                        {
                            port = $"Port={second};";
                        }
                        continue;
                    }
                    if (string.Equals(first, "db") || string.Equals(first, "database"))
                    {
                        if (string.IsNullOrEmpty(second))
                        {
                            database = $"Db={EnvPgDb};";
                        }
                        else
                        {
                            database = $"Db={second};";
                        }
                        continue;
                    }
                    if (!string.IsNullOrEmpty(part))
                    {
                        addition = string.Concat(addition == null ? "" : $"{addition};", part);
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
            server = GetServer(Current.Value.SkipConnectionPrompt);
        }
        if (string.IsNullOrEmpty(port))
        {
            port = GetPort(Current.Value.SkipConnectionPrompt);
        }
        if (string.IsNullOrEmpty(database))
        {
            database = GetDatabase(Current.Value.SkipConnectionPrompt);
        }
        if (string.IsNullOrEmpty(user))
        {
            user = GetUser(Current.Value.SkipConnectionPrompt);
        }
        if (string.IsNullOrEmpty(pass))
        {
            pass = GetPassword(Current.Value.SkipConnectionPrompt);
        }
        if (string.Equals(addition, connectionStr))
        {
            return $"{server}{database}{port}{user}{pass}";
        }
        return $"{server}{database}{port}{user}{pass}{addition}";
    }
}
