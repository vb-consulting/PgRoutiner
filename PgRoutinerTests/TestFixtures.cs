using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Norm;
using Npgsql;
using Xunit;

namespace PgRoutinerTests
{
    public class Config
    {
        public string TestConnection { get; set; }
        public string TestDatabaseName { get; set; }
        public List<string> UpScripts { get; set; } = new();
        public List<string> DownScripts { get; set; } = new();

        public static Config Value { get; }
        public static string ConnectionString { get; }
        
        static Config()
        {
            Value = new Config();
            var config = new ConfigurationBuilder().AddJsonFile("testsettings.json", false, false).Build();
            config.GetSection("TestSettings").Bind(Value);
            ConnectionString = config.GetConnectionString(Value.TestConnection);
        }
    }

    public sealed class PostgreSqlFixture : IDisposable
    {
        public NpgsqlConnection Connection { get; }

        public PostgreSqlFixture()
        {
            Connection = new NpgsqlConnection(Config.ConnectionString);
            CreateTestDatabase(Connection);
            Connection.ChangeDatabase(Config.Value.TestDatabaseName);
            ApplyMigrations(Connection, Config.Value.UpScripts);
        }

        public void Dispose()
        {
            ApplyMigrations(Connection, Config.Value.DownScripts);
            Connection.Close();
            Connection.Dispose();
            using var connection = new NpgsqlConnection(Config.ConnectionString);
            DropTestDatabase(connection);
        }

        private static void CreateTestDatabase(NpgsqlConnection connection)
        {
            void DoCreate() => connection.Execute($"create database {Config.Value.TestDatabaseName}");
            try
            {
                DoCreate();
            }
            catch (PostgresException e) 
            when (e.SqlState == "42P04") // 42P04=duplicate_database, see https://www.postgresql.org/docs/9.3/errcodes-appendix.html
            {
                DropTestDatabase(connection);
                DoCreate();
            }
        }

        private static void DropTestDatabase(NpgsqlConnection connection) => connection.Execute($@"
            revoke connect on database {Config.Value.TestDatabaseName} from public;
            select pg_terminate_backend(pid) from pg_stat_activity where datname = '{Config.Value.TestDatabaseName}' and pid <> pg_backend_pid();
            drop database {Config.Value.TestDatabaseName};");

        private static void ApplyMigrations(NpgsqlConnection connection, List<string> scriptPaths)
        {
            foreach (var path in scriptPaths)
            {
                connection.Execute(File.ReadAllText(path));
            }
        }
    }

    [CollectionDefinition("PostgreSqlDatabase")]
    public class DatabaseFixtureCollection : ICollectionFixture<PostgreSqlFixture> { }

    [Collection("PostgreSqlDatabase")]
    public abstract class PostgreSqlUnitTestFixture : IDisposable
    {
        protected NpgsqlConnection Connection { get; }

        protected PostgreSqlUnitTestFixture(PostgreSqlFixture fixture)
        {
            Connection = fixture.Connection.CloneWith(fixture.Connection.ConnectionString);
            Connection.Execute("begin");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "XUnit")]
        public void Dispose()
        {
            Connection.Execute("rollback");
            Connection.Close();
            Connection.Dispose();
        }
    }
}
