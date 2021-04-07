using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class UnitTestCode : CodeHelpers
    {
        private readonly string name;
        private readonly ExtensionMethods ext;

        public StringBuilder Class { get; } = new();

        public UnitTestCode(Settings settings, string name, ExtensionMethods ext) : base(settings)
        {
            this.name = name;
            this.ext = ext;
            Build();
        }

        private void Build()
        {
            Class.AppendLine($"{I1}public class {name} : PostgreSqlUnitTestFixture");
            Class.AppendLine($"{I1}{{");
            Class.AppendLine($"{I2}public {name}(PostgreSqlFixture fixture) : base(fixture) {{ }}");
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
            Class.AppendLine($"{I3}// Assert.Equal(default(?), result);");
            Class.AppendLine($"{I2}}}");
        }

        private void BuildTests()
        {
            List<string> names = new();
            foreach (var m in ext.Methods)
            {
                var name = m.Name;
                names.Add(name);
                var count = names.Where(n => string.Equals(n, name)).Count();
                Class.AppendLine();
                Class.AppendLine($"{I2}[Fact]");
                if (m.Sync)
                {
                    Class.AppendLine($"{I2}public void {name}_Test{count}()");
                }
                else
                {
                    Class.AppendLine($"{I2}public async Task {name}_Test{count}()");
                }
                Class.AppendLine($"{I2}{{");

                Class.AppendLine($"{I3}// Arrange");
                foreach (var p in m.Params)
                {
                    Class.AppendLine($"{I3}{p.Type} {p.Name} = default;");
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
                    if (!m.Returns.IsVoid && m.Returns.IsInstance)
                    {
                        Class.AppendLine($"await Connection.{name}({string.Join(", ", m.Params.Select(p => p.Name))}).ToListAsync();");
                    }
                    else
                    {
                        Class.AppendLine($"await Connection.{name}({string.Join(", ", m.Params.Select(p => p.Name))});");
                    }
                }
                else
                {
                    Class.AppendLine($"Connection.{name}({string.Join(", ", m.Params.Select(p => p.Name))});");
                }

                Class.AppendLine();

                Class.AppendLine($"{I3}// Assert");
                if (m.Returns.IsVoid)
                {
                    Class.Append($"{I3}// Assert.Equal(default(string), Connection.Read<string>(\"select your assertion value\").Single());");
                }
                else
                {
                    Class.Append($"{I3}// Assert.Equal(default({m.Returns.Name}), result);");
                }
                Class.AppendLine();

                Class.AppendLine($"{I2}}}");
            }
        }
    }
}
