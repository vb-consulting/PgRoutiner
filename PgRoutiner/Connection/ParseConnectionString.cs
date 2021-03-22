using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PgRoutiner
{
    public partial class ConnectionManager
    {
        public NpgsqlConnection ParseConnectionString()
        {
            if (!string.IsNullOrEmpty(name))
            {
                var connectionStr = config.GetConnectionString(name);
                if (string.IsNullOrEmpty(connectionStr))
                {
                    Program.DumpError($"Connection name {name} could not be found in any of the setting files, exiting...");
                    return null;
                }
                connectionStr = ParseConnStringInternal(connectionStr);
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
                    Program.DumpError($"Could not open {name}{Environment.NewLine}{e.Message}{Environment.NewLine}Exiting...");
                    return null;
                }
            }
            else
            {
                var configKey = GetConnStringConfigKey();
                if (configKey != null)
                {
                    typeof(Settings).GetProperty(connectionKey).SetValue(Settings.Value, configKey);
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
                        Program.DumpError($"Could not open {name}{Environment.NewLine}{e.Message}{Environment.NewLine}Exiting...");
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
                        Program.DumpError($"Could not open {connectionStr}{Environment.NewLine}{e.Message}{Environment.NewLine}Exiting...");
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
            foreach(var entry in section)
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
                    if (!string.IsNullOrEmpty(port))
                    { 
                        port = $"Port={port};";
                    }
                    if (!string.IsNullOrEmpty(database))
                    {
                        database = $"Db={database};";
                    }

                    if (!string.IsNullOrEmpty(user))
                    {
                        user = $"User Id={user};";
                    }
                    if (!string.IsNullOrEmpty(pass))
                    {
                        pass = $"Password={pass};";
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
                            user = $"User Id={second};";
                            continue;
                        }
                        if (string.Equals(first, "password"))
                        {
                            pass = $"Password={second};";
                            continue;
                        }
                        if (string.Equals(first, "server") || string.Equals(first, "host"))
                        {
                            server = $"Server={second};";
                            continue;
                        }
                        if (string.Equals(first, "port"))
                        {
                            port = $"Port={second};";
                            continue;
                        }
                        if (string.Equals(first, "db") || string.Equals(first, "database"))
                        {
                            database = $"Db={second};";
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
                server = GetServer(false);
            }
            if (string.IsNullOrEmpty(port))
            {
                port = GetPort(false);
            }
            if (string.IsNullOrEmpty(database))
            {
                database = GetDatabase(false);
            }
            if (string.IsNullOrEmpty(user))
            {
                user = GetUser(false);
            }
            if (string.IsNullOrEmpty(pass))
            {
                pass = GetPassword(false);
            }
           
            return $"{server}{database}{port}{user}{pass}";
        }
    }
}
