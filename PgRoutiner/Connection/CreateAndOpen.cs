using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PgRoutiner
{
    public partial class ConnectionManager
    {
        private NpgsqlConnection CreateAndOpen(string connectionStr)
        {
            var conn = new NpgsqlConnection(connectionStr);
            conn.Open();
            
            var keyValue = typeof(Settings).GetProperty(connectionKey).GetValue(Settings.Value) as string;
            if (!string.IsNullOrEmpty(keyValue))
            {
                Program.WriteLine("");
                Program.Write($"Using {name1} ");
                Program.Write(ConsoleColor.Yellow, keyValue);
                Program.Write(":");
                Program.WriteLine("");
                Program.WriteLine(ConsoleColor.Cyan, " " + conn.ConnectionString);
            }
            else
            {
                Program.WriteLine("");
                Program.Write($"Using {name1}:");
                Program.WriteLine(ConsoleColor.Cyan, " " + conn.ConnectionString);
            }
            return conn;
        }
    }
}
