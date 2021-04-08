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
            var extensions = new CodeRoutinesBuilder(connection, Settings.Value, CodeSettings.ToRoutineSettings(Settings.Value)).GetMethods();

            if (!File.Exists(projectFile))
            {
                DumpRelativePath("Creating file: {0} ...", projectFile);
                if (!WriteFile(projectFile, GetTestCsprojContent(dir)))
                {
                    return;
                }
                Program.RunProcess("dotnet", "add package Microsoft.NET.Test.Sdk", dir);
                Program.RunProcess("dotnet", "add package Norm.net", dir);
                if (extensions.Any(e => e.Methods.Any(m => m.Sync == false)))
                {
                    Program.RunProcess("dotnet", "add package System.Linq.Async", dir);
                }
                Program.RunProcess("dotnet", "add package Npgsql", dir);
                Program.RunProcess("dotnet", "add package xunit", dir);
                Program.RunProcess("dotnet", "add package xunit.runner.visualstudio", dir);
                Program.RunProcess("dotnet", "add package coverlet.collector", dir);
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
                    module.AddUsing(ext.Namespace);
                    module.AddUsing("Xunit");
                    module.AddUsing("Norm");

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
            StringBuilder sb = new(@"{");
            sb.AppendLine();
            sb.AppendLine(@"  ""ConnectionStrings"": {");
            sb.AppendLine(@$"    ""DefaultConnection"": ""{connection.ConnectionString}""");
            sb.AppendLine(@"  },");
            sb.AppendLine(@"  ""TestSettings"": {");
            sb.AppendLine(@"    ""TestConnection"": ""DefaultConnection"",");
            sb.AppendLine(@$"    ""TestDatabaseName"": ""{ConnectionName.ToKebabCase().Replace("-", "_")}_test_{Guid.NewGuid().ToString().Substring(0, 8)}"",");
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
            sb.AppendLine(@$"    ""UpScripts"": [ {string.Join(", ", scripts)} ],");
            sb.AppendLine(@"    ""DownScripts"": []");
            sb.AppendLine(@"  }");
            sb.AppendLine(@"}");
            return sb.ToString();
        }

        private static string GetTestFixturesFile(string ns)
        {
            StringBuilder sb = new();
            sb.AppendLine(@"using System;");
            sb.AppendLine(@"using System.Collections.Generic;");
            sb.AppendLine(@"using System.IO;");
            sb.AppendLine(@"using Microsoft.Extensions.Configuration;");
            sb.AppendLine(@"using Norm;");
            sb.AppendLine(@"using Npgsql;");
            sb.AppendLine(@"using Xunit;");
            sb.AppendLine(@"");
            sb.AppendLine(@$"namespace {ns}");
            sb.AppendLine(@"{");
            sb.AppendLine(@"    public class Config");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        public string TestConnection { get; set; }");
            sb.AppendLine(@"        public string TestDatabaseName { get; set; }");
            sb.AppendLine(@"        public List<string> UpScripts { get; set; } = new();");
            sb.AppendLine(@"        public List<string> DownScripts { get; set; } = new();");
            sb.AppendLine(@"");
            sb.AppendLine(@"        public static Config Value { get; }");
            sb.AppendLine(@"        public static string ConnectionString { get; }");
            sb.AppendLine(@"        ");
            sb.AppendLine(@"        static Config()");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            Value = new Config();");
            sb.AppendLine(@"            var config = new ConfigurationBuilder().AddJsonFile(""testsettings.json"", false, false).Build();");
            sb.AppendLine(@"            config.GetSection(""TestSettings"").Bind(Value);");
            sb.AppendLine(@"            ConnectionString = config.GetConnectionString(Value.TestConnection);");
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
            sb.AppendLine(@"        private static void CreateTestDatabase(NpgsqlConnection connection)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            void DoCreate() => connection.Execute($""create database {Config.Value.TestDatabaseName}"");");
            sb.AppendLine(@"            try");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                DoCreate();");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"            catch (PostgresException e) ");
            sb.AppendLine(@"            when (e.SqlState == ""42P04"") // 42P04=duplicate_database, see https://www.postgresql.org/docs/9.3/errcodes-appendix.html");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                DropTestDatabase(connection);");
            sb.AppendLine(@"                DoCreate();");
            sb.AppendLine(@"            }");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        private static void DropTestDatabase(NpgsqlConnection connection) => connection.Execute($@""");
            sb.AppendLine(@"            revoke connect on database {Config.Value.TestDatabaseName} from public;");
            sb.AppendLine(@"            select pg_terminate_backend(pid) from pg_stat_activity where datname = '{Config.Value.TestDatabaseName}' and pid <> pg_backend_pid();");
            sb.AppendLine(@"            drop database {Config.Value.TestDatabaseName};"");");
            sb.AppendLine(@"");
            sb.AppendLine(@"        private static void ApplyMigrations(NpgsqlConnection connection, List<string> scriptPaths)");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            foreach (var path in scriptPaths)");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                connection.Execute(File.ReadAllText(path));");
            sb.AppendLine(@"            }");
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
            sb.AppendLine(@"            Connection = fixture.Connection.CloneWith(fixture.Connection.ConnectionString);");
            sb.AppendLine(@"            Connection.Execute(""begin"");");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"");
            sb.AppendLine(@"        [System.Diagnostics.CodeAnalysis.SuppressMessage(""Usage"", ""CA1816:Dispose methods should call SuppressFinalize"", Justification = ""XUnit"")]");
            sb.AppendLine(@"        public void Dispose()");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            Connection.Execute(""rollback"");");
            sb.AppendLine(@"            Connection.Close();");
            sb.AppendLine(@"            Connection.Dispose();");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"}");
            return sb.ToString();
        }
    }
}
