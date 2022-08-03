using PgRoutiner.Builder.CodeBuilders.Models;

namespace PgRoutiner.Builder.CodeBuilders.UnitTests;

public class UnitTestBuilder
{
    public static void BuildUnitTests(NpgsqlConnection connection, string schemaFile, string dataFile)
    {
        if (!Settings.Value.UnitTests || Settings.Value.UnitTestsDir == null)
        {
            return;
        }
        string sufix = Settings.Value.GetAssumedNamespace();
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
                    Writer.DumpFormat("Deleting all existing files in dir: {0} ...", relativeDir);
                    foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles())
                    {
                        fi.Delete();
                    }
                }
                Writer.DumpFormat("Removing dir: {0} ...", relativeDir);
                Directory.Delete(dir, true);
                exists = false;
            }
        }
        if (!exists)
        {
            Writer.DumpFormat("Creating dir: {0} ...", relativeDir);
            Directory.CreateDirectory(dir);
        }


        var projectFile = Path.GetFullPath(Path.Join(dir, $"{name}.csproj"));
        List<ExtensionMethods> extensions = new();
        extensions.AddRange(new CodeRoutinesBuilder(connection, Settings.Value, CodeSettings.ToRoutineSettings(Settings.Value)).GetMethods());
        extensions.AddRange(new Crud.CodeCrudBuilder(connection, Settings.Value, CodeSettings.ToCrudSettings(Settings.Value)).GetMethods());

        if (!File.Exists(projectFile))
        {
            Writer.DumpRelativePath("Creating file: {0} ...", projectFile);
            if (!Writer.WriteFile(projectFile, new UnitTestProjectCsprojFile(Settings.Value, dir).ToString()))
            {
                return;
            }
            Process.Run("dotnet", "add package Microsoft.NET.Test.Sdk", dir);
            Process.Run("dotnet", "add package Microsoft.Extensions.Configuration", dir);
            Process.Run("dotnet", "add package Microsoft.Extensions.Configuration.Json", dir);
            Process.Run("dotnet", "add package Microsoft.Extensions.Configuration.Binder", dir);
            Process.Run("dotnet", "add package Norm.net", dir);
            if (extensions.Any(e => e.Methods.Any(m => m.Sync == false)))
            {
                Process.Run("dotnet", "add package System.Linq.Async", dir);
            }
            Process.Run("dotnet", "add package Npgsql", dir);
            Process.Run("dotnet", "add package xunit", dir);
            Process.Run("dotnet", "add package xunit.runner.visualstudio", dir);
            Process.Run("dotnet", "add package coverlet.collector", dir);
            Process.Run("dotnet", "add package FluentAssertions", dir);
        }
        else
        {
            Writer.DumpRelativePath("Skipping {0}, already exists ...", projectFile);
        }

        var settingsFile = Path.GetFullPath(Path.Join(dir, "testsettings.json"));
        if (!File.Exists(settingsFile))
        {
            Writer.DumpRelativePath("Creating file: {0} ...", settingsFile);
            if (!Writer.WriteFile(settingsFile, GetTestSettingsContent(connection, dir, schemaFile, dataFile)))
            {
                return;
            }
        }
        else
        {
            Writer.DumpRelativePath("Skipping {0}, already exists ...", settingsFile);
        }

        var settings = new Settings { Namespace = name, UseFileScopedNamespaces = Settings.Value.UseFileScopedNamespaces };
        var useGlobalUsing = Settings.Value.UnitTestProjectTargetFramework == "net6.0" || Settings.Value.UnitTestProjectLangVersion == "10";

        HashSet<string> usings = new();
        if (extensions.Any())
        {
            foreach (var ext in extensions)
            {
                var className = $"{ext.Name}UnitTests";
                var moduleFile = ext.Schema == "public" ? 
                    Path.GetFullPath(Path.Join(dir, $"{className}.cs")) :
                    Path.GetFullPath(Path.Join(dir, ext.Schema.ToUpperCamelCase(), $"{className}.cs"));

                if (File.Exists(moduleFile))
                {
                    Writer.DumpRelativePath("Skipping {0}, already exists ...", moduleFile);
                    continue;
                }
                var module = new Module(settings, useGlobalUsing);
                if (ext.Schema != "public")
                {
                    module.Namespace = $"{module.Namespace}.{ext.Schema.ToUpperCamelCase()}";
                }
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
                Writer.DumpRelativePath("Creating file: {0} ...", moduleFile);
                Writer.WriteFile(moduleFile, module.ToString());
                if (useGlobalUsing)
                {
                    foreach(var u in module.Usings)
                    {
                        usings.Add(u);
                    }
                }
            }
        }
        else
        {
            var className = $"UnitTest1";
            var moduleFile = Path.GetFullPath(Path.Join(dir, $"{className}.cs"));
            if (File.Exists(moduleFile))
            {
                Writer.DumpRelativePath("Skipping {0}, already exists ...", moduleFile);
                return;
            }
            var module = new Module(settings, useGlobalUsing);
            module.AddUsing("Xunit");
            module.AddUsing("Norm");

            var code = new UnitTestCode(Settings.Value, className, null);
            module.AddItems(code.Class);
            Writer.DumpRelativePath("Creating file: {0} ...", moduleFile);
            Writer.WriteFile(moduleFile, module.ToString());
            if (useGlobalUsing)
            {
                foreach (var u in module.Usings)
                {
                    usings.Add(u);
                }
            }
        }

        var fixtureFile = Path.GetFullPath(Path.Join(dir, "TestFixtures.cs"));
        if (!File.Exists(fixtureFile))
        {
            Writer.DumpRelativePath("Creating file: {0} ...", fixtureFile);
            var fixtureCode = new TestFixtures(settings, usings);
            if (!Writer.WriteFile(fixtureFile, fixtureCode.Class.ToString()))
            {
                return;
            }
        }
        else
        {
            Writer.DumpRelativePath("Skipping {0}, already exists ...", fixtureFile);
        }
    }

    private static string GetTestSettingsContent(NpgsqlConnection connection, string dir, string schemaFile, string dataFile)
    {
        StringBuilder sb = new();

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
        sb.AppendLine(@"    // Path to the external JSON configuration file.");
        sb.AppendLine(@"    // External configuration is only used to parse the ConnectionStrings section.");
        sb.AppendLine(@"    // Use this setting to set TestConnection in a different configuration file, so that the connection string doesn't have to be duplicated.");
        sb.AppendLine(@"    //");

        sb.AppendLine(@"    ""ConfigPath"": null,");

        sb.AppendLine();
        sb.AppendLine(@"    //");
        sb.AppendLine(@"    // Name of the database recreated on each testing session.");
        sb.AppendLine(@"    // Database on the server defined by the TestConnection with this name will be created before the first test starts and dropped after the last test ends.");
        sb.AppendLine(@"    // Make sure that the database with the name doesn't already exist on server.");
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
        if (schemaFile != null && File.Exists(schemaFile))
        {
            scripts.Add($"\"{Path.GetRelativePath(dir, schemaFile).Replace("\\", "/")}\"");
        }
        if (dataFile != null && File.Exists(dataFile))
        {
            scripts.Add($"\"{Path.GetRelativePath(dir, dataFile).Replace("\\", "/")}\"");
        }

        sb.AppendLine();
        sb.AppendLine(@"    //");
        sb.AppendLine(@"    // List of the SQL scripts to be executed in order after the test database has been created and just before the first test starts.");
        sb.AppendLine(@"    // This can be any SQL script file like migrations, schema, or data dumps.");
        sb.AppendLine(@"    //");
        sb.AppendLine(@$"    ""UpScripts"": [ {string.Join(", ", scripts)} ],");

        sb.AppendLine();
        sb.AppendLine(@"    //");
        sb.AppendLine(@"    // List of the SQL scripts to be executed in order before the test database is dropped and after the last is finished.");
        sb.AppendLine(@"    //");
        sb.AppendLine(@"    ""DownScripts"": [ ],");

        sb.AppendLine();
        sb.AppendLine(@"    //");
        sb.AppendLine(@"    // Set this to true to run each test in an isolated transaction.");
        sb.AppendLine(@"    // Transaction is created before each test starts and rolled back after each test finishes.");
        sb.AppendLine(@"    //");

        sb.AppendLine(@"    ""UnitTestsUnderTransaction"": true,");

        sb.AppendLine();
        sb.AppendLine(@"    //");
        sb.AppendLine(@"    // Set this to true to run each unit test connection in a new and uniquely created database that is created by using a template from the test database. ");
        sb.AppendLine(@"    // New database is created as a template database from a test database before each test starts and dropped after the test finishes.");
        sb.AppendLine(@"    // That new database will be named the same as the test database plus a new guid.");
        sb.AppendLine(@"    // This settings cannot be combined with TestDatabaseFromTemplate, UnitTestsUnderTransaction, UpScripts and DownScripts");
        sb.AppendLine(@"    //");
        sb.AppendLine(@"    ""UnitTestsNewDatabaseFromTemplate"": false");
        sb.AppendLine(@"  }");
        sb.AppendLine(@"}");
        return sb.ToString();
    }
}
