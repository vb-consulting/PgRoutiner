namespace PgRoutiner.Builder.CodeBuilders.UnitTests;

public class TestFixtures : Code
{
    private readonly HashSet<string> globalUsings;

    public TestFixtures(Settings settings, HashSet<string> globalUsings = null) : base(settings, null)
    {
        this.globalUsings = globalUsings;

        Class = Build();
    }

    private StringBuilder Build()
    {
        StringBuilder sb = new();
        if (settings.SourceHeaderLines != null && settings.SourceHeaderLines.Count > 0)
        {
            foreach (var line in settings.SourceHeaderLines)
            {
                sb.AppendLine(string.Format(line, DateTime.Now));
            }
        }
        if (globalUsings == null)
        {
            sb.AppendLine(@"using System;");
            sb.AppendLine(@"using System.Collections.Generic;");
            sb.AppendLine(@"using System.IO;");
            sb.AppendLine(@"using System.Linq;");
            sb.AppendLine(@"using Microsoft.Extensions.Configuration;");
            sb.AppendLine(@"using Norm;");
            sb.AppendLine(@"using Npgsql;");
            sb.AppendLine(@"using Xunit;");
            sb.AppendLine(@"");
        }
        else
        {
            foreach(var u in globalUsings)
            {
                sb.AppendLine($"global using {u};");
            }
            void AddUsing(string u)
            {
                if (!globalUsings.Contains(u))
                {
                    sb.AppendLine($"using {u};");
                }
            }
            AddUsing(@"System");
            AddUsing(@"System.Collections.Generic");
            AddUsing(@"System.IO");
            AddUsing(@"System.Linq");
            AddUsing(@"Microsoft.Extensions.Configuration");
            AddUsing(@"Norm");
            AddUsing(@"Npgsql");
            AddUsing(@"Xunit");
            sb.AppendLine(@"");
        }

        if (!settings.UseFileScopedNamespaces)
        {
            sb.AppendLine(@$"namespace {settings.Namespace}");
            sb.AppendLine(@"{");
        }
        else
        {
            sb.AppendLine(@$"namespace {settings.Namespace};");
            sb.AppendLine(@"");
        }

        sb.AppendLine(@$"{I1}public class Config");
        sb.AppendLine(@$"{I1}{{");
        sb.AppendLine(@$"{I2}public string TestConnection {{ get; set; }}");
        sb.AppendLine(@$"{I2}public string ConfigPath {{ get; set; }}");
        sb.AppendLine(@$"{I2}public string TestDatabaseName {{ get; set; }}");
        sb.AppendLine(@$"{I2}public bool TestDatabaseFromTemplate {{ get; set; }}");
        sb.AppendLine(@$"{I2}public List<string> UpScripts {{ get; set; }} = new();");
        sb.AppendLine(@$"{I2}public List<string> DownScripts {{ get; set; }} = new();");
        sb.AppendLine(@$"{I2}public bool UnitTestsUnderTransaction {{ get; set; }}");
        sb.AppendLine(@$"{I2}public bool UnitTestsNewDatabaseFromTemplate {{ get; set; }}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}public static Config Value {{ get; }}");
        sb.AppendLine(@$"{I2}public static string ConnectionString {{ get; }}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}static Config()");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}Value = new Config();");
        sb.AppendLine(@$"{I3}var config = new ConfigurationBuilder().AddJsonFile(""testsettings.json"", false, false).Build();");
        sb.AppendLine(@$"{I3}config.GetSection(""TestSettings"").Bind(Value);");
        sb.AppendLine(@$"{I3}if (Value.TestDatabaseFromTemplate && Value.UnitTestsNewDatabaseFromTemplate)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}throw new ArgumentException(@""Configuration settings TestDatabaseFromTemplate=true and UnitTestsNewDatabaseFromTemplate=true doesn't make any sense.");
        sb.AppendLine(@"There is no point of creating a test database from a template and to do that again for each unit test.");
        sb.AppendLine(@"Set one of TestDatabaseFromTemplate or UnitTestsNewDatabaseFromTemplate to false."");");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}if (Value.UnitTestsNewDatabaseFromTemplate && Value.UnitTestsUnderTransaction)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}throw new ArgumentException(@""Configuration settings UnitTestsNewDatabaseFromTemplate=true and UnitTestsUnderTransaction=true doesn't make any sense.");
        sb.AppendLine(@"There is no point of creating a new test database from a template for each test and then use transaction on a database where only one test runs.");
        sb.AppendLine(@"Set one of UnitTestsNewDatabaseFromTemplate or UnitTestsUnderTransaction to false."");");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}if (Value.UnitTestsNewDatabaseFromTemplate && (Value.UpScripts.Any() || Value.DownScripts.Any()))");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}throw new ArgumentException(@""Configuration settings UnitTestsNewDatabaseFromTemplate=true and up or down scripts (UpScripts, DownScripts) doesn't make any sense.");
        sb.AppendLine(@"Up or down scripts are only applied on a test database created for all tests.");
        sb.AppendLine(@"Set one of UnitTestsNewDatabaseFromTemplate or clear UpScripts and DownScripts."");");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}string externalConnectionString = null;");
        sb.AppendLine(@$"{I3}if (Value.ConfigPath != null && File.Exists(Value.ConfigPath))");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}var external = new ConfigurationBuilder().AddJsonFile(Path.Join(Directory.GetCurrentDirectory(), Value.ConfigPath), false, false).Build();");
        sb.AppendLine(@$"{I4}externalConnectionString = external.GetConnectionString(Value.TestConnection);");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}ConnectionString = config.GetConnectionString(Value.TestConnection) ?? externalConnectionString;");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@$"{I1}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I1}public sealed class PostgreSqlFixture : IDisposable");
        sb.AppendLine(@$"{I1}{{");
        sb.AppendLine(@$"{I2}public NpgsqlConnection Connection {{ get; }}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}public PostgreSqlFixture()");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}Connection = new NpgsqlConnection(Config.ConnectionString);");
        sb.AppendLine(@$"{I3}CreateTestDatabase(Connection);");
        sb.AppendLine(@$"{I3}Connection.ChangeDatabase(Config.Value.TestDatabaseName);");
        sb.AppendLine(@$"{I3}ApplyMigrations(Connection, Config.Value.UpScripts);");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}public void Dispose()");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}ApplyMigrations(Connection, Config.Value.DownScripts);");
        sb.AppendLine(@$"{I3}Connection.Close();");
        sb.AppendLine(@$"{I3}Connection.Dispose();");
        sb.AppendLine(@$"{I3}using var connection = new NpgsqlConnection(Config.ConnectionString);");
        sb.AppendLine(@$"{I3}DropTestDatabase(connection);");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}public static void CreateDatabase(NpgsqlConnection connection, string database, string template = null)");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}void DoCreate() => connection.Execute($""create database {{database}}{{(template == null ? """" : $"" template {{template}}"")}}"");");
        sb.AppendLine(@$"{I3}if (template != null)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}connection.Execute(RevokeUsersCmd(template));");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}try");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}DoCreate();");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}catch (PostgresException e)");
        sb.AppendLine(@$"{I3}when (e.SqlState == ""42P04"") // 42P04=duplicate_database, see https://www.postgresql.org/docs/9.3/errcodes-appendix.html");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}DropDatabase(connection, database);");
        sb.AppendLine(@$"{I4}DoCreate();");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}public static void DropDatabase(NpgsqlConnection connection, string database)");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}connection.Execute($@""");
        sb.AppendLine(@$"{I3}{{RevokeUsersCmd(database)}}");
        sb.AppendLine(@$"{I3}drop database {{database}};"");");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}private static void CreateTestDatabase(NpgsqlConnection connection)");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}if (Config.Value.TestDatabaseFromTemplate)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}CreateDatabase(connection, Config.Value.TestDatabaseName, connection.Database);");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}else");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}CreateDatabase(connection, Config.Value.TestDatabaseName);");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}private static void DropTestDatabase(NpgsqlConnection connection)");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}DropDatabase(connection, Config.Value.TestDatabaseName);");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}private static void ApplyMigrations(NpgsqlConnection connection, List<string> scriptPaths)");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}foreach (var path in scriptPaths)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}connection.Execute(File.ReadAllText(path));");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}private static string RevokeUsersCmd(string database)");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}return $@""");
        sb.AppendLine(@$"{I3}revoke connect on database {{database}} from public;");
        sb.AppendLine(@$"{I3}select pg_terminate_backend(pid) from pg_stat_activity where datname = '{{database}}' and pid <> pg_backend_pid();");
        sb.AppendLine(@$"{I3}"";");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@$"{I1}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I1}[CollectionDefinition(""PostgreSqlDatabase"")]");
        sb.AppendLine(@$"{I1}public class DatabaseFixtureCollection : ICollectionFixture<PostgreSqlFixture> {{ }}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I1}[Collection(""PostgreSqlDatabase"")]");
        sb.AppendLine(@$"{I1}public abstract class PostgreSqlUnitTestFixture : IDisposable");
        sb.AppendLine(@$"{I1}{{");
        sb.AppendLine(@$"{I2}protected NpgsqlConnection Connection {{ get; }}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}protected PostgreSqlUnitTestFixture(PostgreSqlFixture fixture)");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}if (Config.Value.UnitTestsNewDatabaseFromTemplate)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}var dbName = string.Concat(Config.Value.TestDatabaseName, ""_"", Guid.NewGuid().ToString().Replace(""-"", ""_""));");
        sb.AppendLine(@$"{I4}using var connection = new NpgsqlConnection(Config.ConnectionString);");
        sb.AppendLine(@$"{I4}PostgreSqlFixture.CreateDatabase(connection, dbName, connection.Database);");
        sb.AppendLine(@$"{I4}Connection = fixture.Connection.CloneWith(fixture.Connection.ConnectionString);");
        sb.AppendLine(@$"{I4}Connection.Open();");
        sb.AppendLine(@$"{I4}Connection.ChangeDatabase(dbName);");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}else");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}Connection = fixture.Connection.CloneWith(fixture.Connection.ConnectionString);");
        sb.AppendLine(@$"{I4}Connection.Open();");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I3}if (Config.Value.UnitTestsUnderTransaction)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}Connection.Execute(""begin"");");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@"");
        sb.AppendLine(@$"{I2}[System.Diagnostics.CodeAnalysis.SuppressMessage(""Usage"", ""CA1816:Dispose methods should call SuppressFinalize"", Justification = ""XUnit"")]");
        sb.AppendLine(@$"{I2}public void Dispose()");
        sb.AppendLine(@$"{I2}{{");
        sb.AppendLine(@$"{I3}if (Config.Value.UnitTestsUnderTransaction)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}Connection.Execute(""rollback"");");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I3}Connection.Close();");
        sb.AppendLine(@$"{I3}Connection.Dispose();");
        sb.AppendLine(@$"{I3}if (Config.Value.UnitTestsNewDatabaseFromTemplate)");
        sb.AppendLine(@$"{I3}{{");
        sb.AppendLine(@$"{I4}using var connection = new NpgsqlConnection(Config.ConnectionString);");
        sb.AppendLine(@$"{I4}PostgreSqlFixture.DropDatabase(connection, Connection.Database);");
        sb.AppendLine(@$"{I3}}}");
        sb.AppendLine(@$"{I2}}}");
        sb.AppendLine(@$"{I1}}}");
        if (!settings.UseFileScopedNamespaces)
        {
            sb.AppendLine(@"}");
        }
        return sb;
    }
}
