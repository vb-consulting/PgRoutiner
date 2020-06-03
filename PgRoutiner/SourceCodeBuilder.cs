using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class SourceCodeBuilder
    {
        private readonly Settings _settings;
        private readonly GetRoutinesResult _item;
        private readonly string _namespace;

        public SourceCodeBuilder(Settings settings, GetRoutinesResult item)
        {
            _settings = settings;
            _item = item;
            _namespace = settings.Namespace;
            if (!string.IsNullOrEmpty(settings.OutputDir))
            {
                _namespace = string.Concat(_namespace, ".", settings.OutputDir.Replace("/", ".").Replace("\\", "."));
            }
        }

        public string Build()
        {
            var name = _item.Name.ToUpperCamelCase();
            var builder = new StringBuilder($@"{_settings.SourceHeader}
using System;
using System.Collections.Generic;
using Norm.Extensions;
using Npgsql;

namespace {_namespace}
{{
");
            string result;
            if (_item.Returns.Record != null)
            {
                result = $"{name}Result";
                builder.AppendLine($@"    public class {result}
    {{");
                foreach (var rec in _item.Returns.Record.OrderBy(r => r.Ordinal))
                {
                    var type = GetType(rec, $"result type \"{rec.Name}\"");
                    builder.AppendLine($"       public {type} {rec.Name.ToUpperCamelCase()} {{ get; set; }}");
                }

                builder.AppendLine("    }");
                builder.AppendLine();
            }
            else
            {
                result = _item.Returns.Type == "void" ? "void" : GetType(_item.Returns, "result type");
            }
            
            builder.AppendLine($@"    public static class PgRoutine{name}
    {{");
            builder.AppendLine($"        public const strong Name = \"{_item.Name}\";");
            builder.AppendLine();

            BuildSyncMethod(builder, result, name);

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private void BuildSyncMethod(StringBuilder builder, string result, string name)
        {
            builder.AppendLine($@"        public static {result} {name}(this NpgsqlConnection connection, {BuildParameters(_item.Parameters)})
        {{");

            builder.Append("            ");


            if (_item.Returns.Type != "void")
            {
                if (_item.Returns.Record == null)
                {
                    builder.Append($"return connection.Single<{GetType(_item.Returns, "result type")}>(");
                }
                else
                {
                    builder.Append($"return connection.Read<{BuildGenericTypes(_item.Returns.Record)}>(");
                }

            }
            else
            {
                builder.Append("connection.Execute(");
            }
            builder.Append("Name, ");

            builder.AppendLine();
            builder.AppendLine("        }");

        }


        private string GetType(PgBaseType pgType, string description)
        {
            if (Settings.TypeMapping.TryGetValue(pgType.Type, out var result))
            {
                return result;
            }
            throw new ArgumentException($"Could not find type \"{pgType}\" for {description}, routine \"{_item.Name}\"");
        }

        private string BuildParameters(IEnumerable<PgType> parameters)
        {
            return string.Join(", ", parameters.Select(p => string.Concat(
                GetType(p, $"parameter for \"{_item.Name}\" at position {p.Ordinal}"), 
                " ", 
                p.Name.ToCamelCase())));
        }

        private string BuildGenericTypes(IEnumerable<PgType> parameters)
        {
            return string.Join(", ", parameters.Select(p => GetType(p, $"parameter for \"{_item.Name}\" at position {p.Ordinal}")));
        }
    }
}
