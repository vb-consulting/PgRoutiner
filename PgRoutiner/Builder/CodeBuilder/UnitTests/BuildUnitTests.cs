using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Npgsql;

namespace PgRoutiner
{
    partial class Builder
    {
        private static void BuildUnitTests(NpgsqlConnection connection)
        {
            if (!Settings.Value.UnitTests || Settings.Value.UnitTestsDir == null)
            {
                return;
            }
            string sufix;
            if (Settings.Value.Namespace != null)
            {
                sufix = Settings.Value.Namespace;
            }
            else
            {
                var projFile = Directory.EnumerateFiles(Program.CurrentDir, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (projFile != null)
                {
                    sufix = Path.GetFileNameWithoutExtension(projFile);
                }
                else
                {
                    sufix = Path.GetFileName(Path.GetFullPath(Program.CurrentDir));
                }
            }
            var shortDir = string.Format(Settings.Value.UnitTestsDir, sufix);
            var dir = Path.GetFullPath(Path.Join(Program.CurrentDir, shortDir));
            var name = Path.GetFileName(dir);
            var relativeDir = dir.GetRelativePath();

            var exists = Directory.Exists(dir);
            if (exists && Settings.Value.UnitTestsAskRecreate)
            {
                var answer = Program.Ask($"Directory {relativeDir} already exists. Do you want to recreate this dir? This will delete all existing files. [Y/N]", ConsoleKey.Y, ConsoleKey.N);
                if (answer == ConsoleKey.Y)
                {
                    if (Directory.GetFiles(dir).Length > 0)
                    {
                        DumpFormat("Deleting all existing files in dir: {0} ...", relativeDir);
                        foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                        {
                            fi.Delete();
                        }
                    }
                    DumpFormat("Removing dir: {0} ...", relativeDir);
                    Directory.Delete(dir, true);
                    exists = false;
                }
            }
            if (!exists)
            {
                DumpFormat("Creating dir: {0} ...", relativeDir);
                Directory.CreateDirectory(dir);
            }
           
            var settingsFile = Path.GetFullPath(Path.Join(dir, "testsettings.json"));
            if (!File.Exists(settingsFile))
            {
                DumpRelativePath("Creating file: {0} ...", settingsFile);
                if (!WriteFile(settingsFile, GetTestSettingsContent(connection, dir)))
                {
                    return;
                }
            }
            else
            {
                DumpRelativePath("Skipping {0}, already exists ...", settingsFile);
            }

            var projectFile = Path.GetFullPath(Path.Join(dir, $"{name}.csproj"));
            List<ExtensionMethods> extensions = new();
            extensions.AddRange(new CodeRoutinesBuilder(connection, Settings.Value, CodeSettings.ToRoutineSettings(Settings.Value)).GetMethods());
            extensions.AddRange(new CodeCrudBuilder(connection, Settings.Value, CodeSettings.ToCrudSettings(Settings.Value)).GetMethods());

            if (!File.Exists(projectFile))
            {
                DumpRelativePath("Creating file: {0} ...", projectFile);
                if (!WriteFile(projectFile, GetTestCsprojContent(dir)))
                {
                    return;
                }
                Program.RunProcess("dotnet", "add package Microsoft.NET.Test.Sdk", dir);
                Program.RunProcess("dotnet", "add package Microsoft.Extensions.Configuration", dir);
                Program.RunProcess("dotnet", "add package Microsoft.Extensions.Configuration.Json", dir);
                Program.RunProcess("dotnet", "add package Microsoft.Extensions.Configuration.Binder", dir);
                Program.RunProcess("dotnet", "add package Norm.net", dir);
                if (extensions.Any(e => e.Methods.Any(m => m.Sync == false)))
                {
                    Program.RunProcess("dotnet", "add package System.Linq.Async", dir);
                }
                Program.RunProcess("dotnet", "add package Npgsql", dir);
                Program.RunProcess("dotnet", "add package xunit", dir);
                Program.RunProcess("dotnet", "add package xunit.runner.visualstudio", dir);
                Program.RunProcess("dotnet", "add package coverlet.collector", dir);
                Program.RunProcess("dotnet", "add package FluentAssertions", dir);
            }
            else
            {
                DumpRelativePath("Skipping {0}, already exists ...", projectFile);
            }

            var fixtureFile = Path.GetFullPath(Path.Join(dir, "TestFixtures.cs"));
            if (!File.Exists(fixtureFile))
            {
                DumpRelativePath("Creating file: {0} ...", fixtureFile);
                if (!WriteFile(fixtureFile, GetTestFixturesFile(name)))
                {
                    return;
                }
            }
            else
            {
                DumpRelativePath("Skipping {0}, already exists ...", fixtureFile);
            }

            if (extensions.Any())
            {
                foreach (var ext in extensions)
                {
                    var className = $"{ext.Name}UnitTests";
                    var moduleFile = Path.GetFullPath(Path.Join(dir, $"{className}.cs"));
                    if (File.Exists(moduleFile))
                    {
                        DumpRelativePath("Skipping {0}, already exists ...", moduleFile);
                        continue;
                    }
                    var module = new Module(new Settings { Namespace = name });
                    if (ext.Methods.Any(m => m.Sync == false))
                    {
                        module.AddUsing("System.Threading.Tasks");
                    }
                    module.AddUsing("Xunit");
                    module.AddUsing("Norm");
                    module.AddUsing("FluentAssertions");
                    module.AddUsing(ext.Namespace);
                    if (!string.IsNullOrEmpty(ext.ModelNamespace))
                    {
                        module.AddUsing(ext.ModelNamespace);
                    }
                    var code = new UnitTestCode(Settings.Value, className, ext);
                    module.AddItems(code.Class);
                    DumpRelativePath("Creating file: {0} ...", moduleFile);
                    WriteFile(moduleFile, module.ToString());
                }
            }
            else
            {
                var className = $"UnitTest1";
                var moduleFile = Path.GetFullPath(Path.Join(dir, $"{className}.cs"));
                if (File.Exists(moduleFile))
                {
                    DumpRelativePath("Skipping {0}, already exists ...", moduleFile);
                    return;
                }
                var module = new Module(new Settings { Namespace = name });
                module.AddUsing("Xunit");
                module.AddUsing("Norm");

                var code = new UnitTestCode(Settings.Value, className, null);
                module.AddItems(code.Class);
                DumpRelativePath("Creating file: {0} ...", moduleFile);
                WriteFile(moduleFile, module.ToString());
            }
        }

        private static string GetTestCsprojContent(string dir)
        {
            StringBuilder sb = new(@"<Project Sdk=""Microsoft.NET.Sdk"">");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(@"  <PropertyGroup>");
            sb.AppendLine(@"    <TargetFramework>net5.0</TargetFramework>");
            sb.AppendLine(@"    <IsPackable>false</IsPackable>");
            sb.AppendLine(@"  </PropertyGroup>");
            sb.AppendLine();
            sb.AppendLine(@"  <ItemGroup>");
            sb.AppendLine(@"    <None Update=""testsettings.json"">");
            sb.AppendLine(@"      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>");
            sb.AppendLine(@"    </None>");
            sb.AppendLine(@"  </ItemGroup>");
            sb.AppendLine();
            if (Settings.ProjectInfo?.ProjectFile != null)
            {
                sb.AppendLine(@"  <ItemGroup>");
                sb.AppendLine(@$"    <ProjectReference Include=""{Path.GetRelativePath(dir, Settings.ProjectInfo.ProjectFile)}"" />");
                sb.AppendLine(@"  </ItemGroup>");
            }
            sb.AppendLine();
            sb.AppendLine(@"</Project>");
            return sb.ToString();
        }

        private static string GetTestSettingsContent(NpgsqlConnection connection, string dir)
        {
            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(Settings.Value.SourceHeader))
            {
                sb.AppendLine(string.Format(Settings.Value.SourceHeader, DateTime.Now));
            }
            sb.AppendLine(@"{");
            sb.AppendLine(@"  ""ConnectionStrings"": {");
            sb.AppendLine(@$"    ""DefaultConnection"": ""{connection.ConnectionString}""");
            sb.AppendLine(@"  },");
            
            sb.AppendLine(@"  ""TestSettings"": {");

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // Name of the connection string used for testing.");
            sb.AppendLine(@"    // This connection string should point to an ctual development or test database. ");
            sb.AppendLine(@"    // The real test database is re-created based on this connection string.");
            sb.AppendLine(@"    // Connection string can be defined in this config file or in the config file defined by the ConfigPath value. ");
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    ""TestConnection"": ""DefaultConnection"",");

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // Path to the external json configuration file.");
            sb.AppendLine(@"    // External configuration is only used to parse the ConnectionStrings section.");
            sb.AppendLine(@"    // Use this setting to set TestConnection in a different configuration file, so that connection string doesn't have to be duplicated.");
            sb.AppendLine(@"    //");

            sb.AppendLine(@"    ""ConfigPath"": null,");

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // Name of the database recreated on each testing session.");
            sb.AppendLine(@"    // Database on server defined by the TestConnection with this name will be created before first test starts and dropped after last test ends.");
            sb.AppendLine(@"    // Make sure that database with name doesn't already exists on server.");
            sb.AppendLine(@"    //");
            sb.AppendLine(@$"    ""TestDatabaseName"": ""{connection.Database}_test_{Guid.NewGuid().ToString().Substring(0, 8)}"",");

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // If set to true, the test database (defined by TestDatabaseName) - will not be created created - but replicated by using database template from a TestConnection.");
            sb.AppendLine(@"    // Replicated database (using database template) has exactly the same schema and as well as the data as original database.");
            sb.AppendLine(@"    // If set to false, the test database is created as empty database and, if migrations are applied (if any).");
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    ""TestDatabaseFromTemplate"": false,");

            dir = Path.Join(dir, "bin/Debug/net5.0");
            List<string> scripts = new();
            if (SchemaFile != null && File.Exists(SchemaFile))
            {
                scripts.Add($"\"{Path.GetRelativePath(dir, SchemaFile).Replace("\\", "/")}\"");
            }
            if (DataFile != null && File.Exists(DataFile))
            {
                scripts.Add($"\"{Path.GetRelativePath(dir, DataFile).Replace("\\", "/")}\"");
            }

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // List of the SQL scripts to be executed in order after the test database has been created and just before the first test starts.");
            sb.AppendLine(@"    // This can be any SQL script file like migrations, schema or data dumps.");
            sb.AppendLine(@"    //");
            sb.AppendLine(@$"    ""UpScripts"": [ {string.Join(", ", scripts)} ],");

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // List of the SQL scripts to be executed in order before the test database is dropped and after the last is finished.");
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    ""DownScripts"": [ ],");

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // Set this to true to run each test in isolated transaction.");
            sb.AppendLine(@"    // Transaction is created before each test starts and rolled back after each test finishes.");
            sb.AppendLine(@"    //");

            sb.AppendLine(@"    ""UnitTestsUnderTransaction"": true,");

            sb.AppendLine();
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    // Set his to true to run each unit test connection in new and uniqly created database that is created by using template from the test database. ");
            sb.AppendLine(@"    // New database is created as a template database from a test database before each test starts and droped after test finishes.");
            sb.AppendLine(@"    // That new database will be named same as test database plus new guid.");
            sb.AppendLine(@"    // This settings cannot be combined with TestDatabaseFromTemplate, UnitTestsUnderTransaction, UpScripts and DownScripts");
            sb.AppendLine(@"    //");
            sb.AppendLine(@"    ""UnitTestsNewDatabaseFromTemplate"": false");
            sb.AppendLine(@"  }");
            sb.AppendLine(@"}");
            return sb.ToString();
        }

        private static string GetTestFixturesFile(string ns)
        {
            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(Settings.Value.SourceHeader))
            {
                sb.AppendLine(string.Format(Settings.Value.SourceHeader, DateTime.Now));
            }
            sb.AppendLine(@"using System;");
            sb.AppendLine(@"using System.Collections.Generic;");
            sb.AppendLine(@"using System.IO;");
            sb.AppendLine(@"using System.Linq;");
            sb.AppendLine(@"using Microsoft.Extensions.Configuration;");
            sb.AppendLine(@"using Norm;");
            sb.AppendLine(@"using Npgsql;");
            sb.AppendLine(@"using Xunit;");
            sb.AppendLine(@"");
            sb.AppendLine(@"namespace PgRoutinerTests");
            sb.AppendLine(@"{");
            sb.AppendLine(@"    public class Config");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        public string TestConnection { get; set; }");
            sb.AppendLine(@"        public string ConfigPath { get; set; }");
            sb.AppendLine(@"        public string TestDatabaseName { get; set; }");
            sb.AppendLine(@"        public bool TestDatabaseFromTemplate { get; set; }");
            sb.AppendLine(@"        public List<string> UpScripts { get; set; } = new();");
            sb.AppendLine(@"        public List<string> DownScripts { get; set; } = new();");
            sb.AppendLine(@"        public bool UnitTestsUnderTransaction { get; set; }");
            sb.AppendLine(@"        public bool UnitTestsNewDatabaseFromTemplate { get; set; }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        public static Config Value { get; }");
            sb.AppendLine(@"        public static string ConnectionString { get; }");
            sb.AppendLine(@"        ");
            sb.AppendLine(@"        static Config()");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            Value = new Config();");
            sb.AppendLine(@"            var config = new ConfigurationBuilder().AddJsonFile(""testsettings.json"", false, false).Build();");
            sb.AppendLine(@"            config.GetSection(""TestSettings"").Bind(Value);");
            sb.AppendLine(@"            if (Value.TestDatabaseFromTemplate && Value.UnitTestsNewDatabaseFromTemplate)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                throw new ArgumentException(@""Configuration settings TestDatabaseFromTemplate=true and UnitTestsNewDatabaseFromTemplate=true doesn't make any sense.");
            sb.AppendLine(@"There is no point of creating a test database from a template and to do that again for each unit test.");
            sb.AppendLine(@"Set one of TestDatabaseFromTemplate or UnitTestsNewDatabaseFromTemplate to false."");");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            if (Value.UnitTestsNewDatabaseFromTemplate && Value.UnitTestsUnderTransaction)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                throw new ArgumentException(@""Configuration settings UnitTestsNewDatabaseFromTemplate=true and UnitTestsUnderTransaction=true doesn't make any sense.");
            sb.AppendLine(@"There is no point of creating a new test database from a template for each test and then use transaction on a database where only one test runs.");
            sb.AppendLine(@"Set one of UnitTestsNewDatabaseFromTemplate or UnitTestsUnderTransaction to false."");");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            if (Value.UnitTestsNewDatabaseFromTemplate && (Value.UpScripts.Any() || Value.DownScripts.Any()))");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                throw new ArgumentException(@""Configuration settings UnitTestsNewDatabaseFromTemplate=true and up or down scripts (UpScripts, DownScripts) doesn't make any sense.");
            sb.AppendLine(@"Up or down scripts are only applied on a test database created for all tests.");
            sb.AppendLine(@"Set one of UnitTestsNewDatabaseFromTemplate or clear UpScripts and DownScripts."");");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            string externalConnectionString = null;");
            sb.AppendLine(@"            if (Value.ConfigPath != null && File.Exists(Value.ConfigPath))");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                var external = new ConfigurationBuilder().AddJsonFile(Path.Join(Directory.GetCurrentDirectory(), Value.ConfigPath), false, false).Build();");
            sb.AppendLine(@"                externalConnectionString = external.GetConnectionString(Value.TestConnection);");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            ConnectionString = config.GetConnectionString(Value.TestConnection) ?? externalConnectionString;");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"");
            sb.AppendLine(@"    public sealed class PostgreSqlFixture : IDisposable");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        public NpgsqlConnection Connection { get; }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        public PostgreSqlFixture()");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            Connection = new NpgsqlConnection(Config.ConnectionString);");
            sb.AppendLine(@"            CreateTestDatabase(Connection);");
            sb.AppendLine(@"            Connection.ChangeDatabase(Config.Value.TestDatabaseName);");
            sb.AppendLine(@"            ApplyMigrations(Connection, Config.Value.UpScripts);");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        public void Dispose()");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            ApplyMigrations(Connection, Config.Value.DownScripts);");
            sb.AppendLine(@"            Connection.Close();");
            sb.AppendLine(@"            Connection.Dispose();");
            sb.AppendLine(@"            using var connection = new NpgsqlConnection(Config.ConnectionString);");
            sb.AppendLine(@"            DropTestDatabase(connection);");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        public static void CreateDatabase(NpgsqlConnection connection, string database, string template = null)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            void DoCreate() => connection.Execute($""create database {database}{(template == null ? """" : $"" template {template}"")}"");");
            sb.AppendLine(@"            if (template != null)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                connection.Execute(RevokeUsersCmd(template));");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            try");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                DoCreate();");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            catch (PostgresException e)");
            sb.AppendLine(@"            when (e.SqlState == ""42P04"") // 42P04=duplicate_database, see https://www.postgresql.org/docs/9.3/errcodes-appendix.html");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                DropDatabase(connection, database);");
            sb.AppendLine(@"                DoCreate();");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        public static void DropDatabase(NpgsqlConnection connection, string database)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            connection.Execute($@""");
            sb.AppendLine(@"            {RevokeUsersCmd(database)}");
            sb.AppendLine(@"            drop database {database};"");");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        private static void CreateTestDatabase(NpgsqlConnection connection)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            if (Config.Value.TestDatabaseFromTemplate)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                CreateDatabase(connection, Config.Value.TestDatabaseName, connection.Database);");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            else");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                CreateDatabase(connection, Config.Value.TestDatabaseName);");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        private static void DropTestDatabase(NpgsqlConnection connection)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            DropDatabase(connection, Config.Value.TestDatabaseName);");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        private static void ApplyMigrations(NpgsqlConnection connection, List<string> scriptPaths)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            foreach (var path in scriptPaths)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                connection.Execute(File.ReadAllText(path));");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        private static string RevokeUsersCmd(string database)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            return $@""");
            sb.AppendLine(@"            revoke connect on database {database} from public;");
            sb.AppendLine(@"            select pg_terminate_backend(pid) from pg_stat_activity where datname = '{database}' and pid <> pg_backend_pid();");
            sb.AppendLine(@"            "";");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"");
            sb.AppendLine(@"    [CollectionDefinition(""PostgreSqlDatabase"")]");
            sb.AppendLine(@"    public class DatabaseFixtureCollection : ICollectionFixture<PostgreSqlFixture> { }");
            sb.AppendLine(@"");
            sb.AppendLine(@"    [Collection(""PostgreSqlDatabase"")]");
            sb.AppendLine(@"    public abstract class PostgreSqlUnitTestFixture : IDisposable");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        protected NpgsqlConnection Connection { get; }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        protected PostgreSqlUnitTestFixture(PostgreSqlFixture fixture)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            if (Config.Value.UnitTestsNewDatabaseFromTemplate)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                var dbName = string.Concat(Config.Value.TestDatabaseName, ""_"", Guid.NewGuid().ToString().Replace(""-"", ""_""));");
            sb.AppendLine(@"                using var connection = new NpgsqlConnection(Config.ConnectionString);");
            sb.AppendLine(@"                PostgreSqlFixture.CreateDatabase(connection, dbName, connection.Database);");
            sb.AppendLine(@"                Connection = fixture.Connection.CloneWith(fixture.Connection.ConnectionString);");
            sb.AppendLine(@"                Connection.Open();");
            sb.AppendLine(@"                Connection.ChangeDatabase(dbName);");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            else");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                Connection = fixture.Connection.CloneWith(fixture.Connection.ConnectionString);");
            sb.AppendLine(@"                Connection.Open();");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            ");
            sb.AppendLine(@"            if (Config.Value.UnitTestsUnderTransaction)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                Connection.Execute(""begin"");");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        [System.Diagnostics.CodeAnalysis.SuppressMessage(""Usage"", ""CA1816:Dispose methods should call SuppressFinalize"", Justification = ""XUnit"")]");
            sb.AppendLine(@"        public void Dispose()");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            if (Config.Value.UnitTestsUnderTransaction)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                Connection.Execute(""rollback"");");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            Connection.Close();");
            sb.AppendLine(@"            Connection.Dispose();");
            sb.AppendLine(@"            if (Config.Value.UnitTestsNewDatabaseFromTemplate)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                using var connection = new NpgsqlConnection(Config.ConnectionString);");
            sb.AppendLine(@"                PostgreSqlFixture.DropDatabase(connection, Connection.Database);");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"}");
            return sb.ToString();
        }
    }
}
