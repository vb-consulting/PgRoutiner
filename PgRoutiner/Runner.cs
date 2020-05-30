using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;


namespace PgRoutiner
{
    partial class Program
    {
        static void Run(IConfiguration config)
        {
            using var connection = new NpgsqlConnection(config.GetConnectionString(Settings.Value.Connection));
            {
                foreach (var (name, parameters, returns, description) in connection.GetRoutines(Settings.Value))
                {

                    Console.WriteLine(name);
                }
            }
        }
    }
}
