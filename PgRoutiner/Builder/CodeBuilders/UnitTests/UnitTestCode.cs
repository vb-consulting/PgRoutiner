using PgRoutiner.Builder.CodeBuilders.Models;

namespace PgRoutiner.Builder.CodeBuilders.UnitTests;

public class UnitTestCode : Code
{
    private readonly ExtensionMethods ext;

    public UnitTestCode(Settings settings, string name, ExtensionMethods ext) : base(settings, name)
    {
        this.ext = ext;
        Build();
    }

    private void Build()
    {
        Class.AppendLine($"{I1}public class {Name} : PostgreSqlUnitTestFixture");
        Class.AppendLine($"{I1}{{");
        Class.AppendLine($"{I2}public {Name}(PostgreSqlFixture fixture) : base(fixture) {{ }}");
        if (ext == null || ext.Methods == null || !ext.Methods.Any())
        {
            BuildEmptyTest();
        }
        else
        {
            BuildTests();
        }
        Class.AppendLine($"{I1}}}");
    }

    private void BuildEmptyTest()
    {
        Class.AppendLine();
        Class.AppendLine($"{I2}[Fact]");
        Class.AppendLine($"{I2}public void Test1()");
        Class.AppendLine($"{I2}{{");

        Class.AppendLine($"{I3}// Arrange");
        Class.AppendLine($"{I3}// string param1 = default");
        Class.AppendLine($"{I3}// int param2 = default");
        Class.AppendLine();

        Class.AppendLine($"{I3}// Act");
        Class.AppendLine($"{I3}// var result = Connection.PostgreSqlFunctionName(param1, param2);");
        Class.AppendLine();

        Class.AppendLine($"{I3}// Assert");
        Class.AppendLine($"{I3}// todo: adjust assert logic template to match actual logic");
        Class.AppendLine($"{I3}// Assert.Equal(default(?), result);");
        Class.AppendLine($"{I2}}}");
    }

    private void BuildTests()
    {
        List<string> names = new();
        var skipSync = settings.UnitTestsSkipSyncMethods;
        var skipAsync = settings.UnitTestsSkipAsyncMethods;
        if (skipSync && skipAsync)
        {
            return;
        }
        foreach (var m in ext.Methods.Where(m => (!skipSync && !skipAsync) || ((skipSync || m.Sync) && (skipAsync || !m.Sync))))
        {
            var methodName = m.Name;
            names.Add(methodName);
            var count = names.Where(n => string.Equals(n, methodName)).Count();
            Class.AppendLine();

            if (m.Routine != null || m.Description != null)
            {
                Class.AppendLine($"{I2}///<summary>");
                Class.AppendLine($"{I2}/// Test method{(m.Routine != null ? $" for {m.Routine}" : "")}.");
                if (m.Description != null)
                {
                    Class.AppendLine($"{I2}///");
                    Class.Append($"{I2}");
                    Class.AppendLine(string.Join($"{I2}", m.Description.Split("\n").Select(d => $"/// {d}")));
                }
                Class.AppendLine($"{I2}///</summary>");
            }

            Class.AppendLine($"{I2}[Fact]");
            if (m.Sync)
            {
                Class.AppendLine($"{I2}public void {methodName}_Test{count}()");
            }
            else
            {
                Class.AppendLine($"{I2}public async Task {methodName}_Test{count}()");
            }
            Class.AppendLine($"{I2}{{");

            Class.AppendLine($"{I3}// Arrange");
            foreach (var p in m.Params)
            {
                if (p.IsInstance)
                {
                    Class.AppendLine($"{I3}var {p.Name} = new {p.Type}();");
                }
                else
                {
                    Class.AppendLine($"{I3}{p.Type} {p.Name} = default;");
                }
            }
            Class.AppendLine();

            Class.AppendLine($"{I3}// Act");
            if (m.Returns.IsVoid)
            {
                Class.Append($"{I3}");
            }
            else
            {
                Class.Append($"{I3}var result = ");
            }

            if (!m.Sync)
            {
                if (!m.Returns.IsVoid && m.Returns.IsEnumerable)
                {
                    Class.AppendLine($"await Connection.{methodName}({string.Join(", ", m.Params.Select(p => p.Name))}).ToListAsync();");
                }
                else
                {
                    Class.AppendLine($"await Connection.{methodName}({string.Join(", ", m.Params.Select(p => p.Name))});");
                }
            }
            else
            {
                if (!m.Returns.IsVoid && m.Returns.IsEnumerable)
                {
                    Class.AppendLine($"Connection.{methodName}({string.Join(", ", m.Params.Select(p => p.Name))}).ToList();");
                }
                else
                {
                    Class.AppendLine($"Connection.{methodName}({string.Join(", ", m.Params.Select(p => p.Name))});");
                }
            }

            Class.AppendLine();

            Class.AppendLine($"{I3}// Assert");
            Class.AppendLine($"{I3}// todo: adjust assert logic template to match actual logic");
            if (m.Returns.IsVoid)
            {
                if (m.Params.Any())
                {
                    var p = m.Params.First();
                    if (p.Type == "string")
                    {
                        Class.Append($"{I3}{p.Name}.Should().BeEquivalentTo(Connection.Read<{p.Type}>(\"select * from {p.PgType}\").Single());");
                    }
                    else
                    {
                        Class.Append($"{I3}{p.Name}.Should().Be(Connection.Read<{p.Type}>(\"select * from {p.PgType}\").Single());");
                    }

                }
                else
                {
                    Class.Append($"{I3}Assert.Equal(default(string), Connection.Read<string>(\"select your assertion value\").Single());");
                }
            }
            else
            {
                if (m.Params.Any() && string.Equals(m.Returns.Name, m.Params.FirstOrDefault().Type) && m.Returns.IsVoid == false)
                {
                    Class.Append($"{I3}{m.Params.FirstOrDefault().Name}.Should().BeEquivalentTo(result);");
                }
                else if (!m.Returns.IsVoid && m.Returns.IsEnumerable)
                {
                    Class.Append($"{I3}Assert.Equal(default(List<{m.Returns.Name}>), result);");
                }
                else
                {
                    Class.Append($"{I3}Assert.Equal(default({m.Returns.Name}), result);");
                }
            }
            Class.AppendLine();

            Class.AppendLine($"{I2}}}");
        }
    }
}
