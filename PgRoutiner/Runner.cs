using System;
using Microsoft.Extensions.Configuration;
using Npgsql;


namespace PgRoutiner
{
    partial class Program
    {
        static void Run(IConfiguration config)
        {
            var i = 0;
            using var connection = new NpgsqlConnection(config.GetConnectionString(Settings.Value.Connection));
            {
                foreach (var item in connection.GetRoutines(Settings.Value))
                {

                    Console.WriteLine(new SourceCodeBuilder(Settings.Value, item).Build());
                    if (i++ > 15) break;
                }
            }
        }
    }
}
