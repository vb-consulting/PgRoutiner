using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class SourceCodeBuilder
    {
        private readonly string NL = Environment.NewLine;
        private readonly Settings _settings;
        private readonly GetRoutinesResult _item;
        private readonly string _namespace;
        private readonly bool _isVoid;
        private readonly bool _noRecords;
        private readonly bool _noParams;
        private readonly bool _singleRecordResult;
        private readonly int _recordCount;
        private const int MaxRecords = 12;
        private static readonly HashSet<string> Models = new HashSet<string>();

        public SourceCodeBuilder(Settings settings, GetRoutinesResult item)
        {
            _settings = settings;
            _item = item;
            _namespace = settings.Namespace;

            _isVoid = _item.Returns.Type == "void";
            _noRecords = _item.Returns.Record == null || !_item.Returns.Record.Any();
            _noParams = _item.Parameters == null || !_item.Parameters.Any();
            _singleRecordResult = !_noRecords && (_item.Returns.Record != null && _item.Returns.Record.Count() == 1);
            _recordCount = _item.Returns.Record?.Count() ?? 0;

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
using System.Linq;
using System.Collections.Generic;
using Norm.Extensions;
using Npgsql;

namespace {_namespace}
{{
");
            string result;
            var resultFields = new List<string>();
            if (!_noRecords && !_singleRecordResult)
            {
                result = _item.Returns.UserDefined ? _item.Returns.Type.ToUpperCamelCase() : $"{name}Result";

                if (!Models.Contains(result))
                {

                    builder.AppendLine($@"    public class {result}
    {{");
                    foreach (var rec in _item.Returns.Record.OrderBy(r => r.Ordinal))
                    {
                        var type = GetType(rec, $"result type \"{rec.Name}\"");
                        var fieldName = rec.Name.ToUpperCamelCase();
                        builder.AppendLine($"       public {type} {fieldName} {{ get; set; }}");
                        resultFields.Add(fieldName);
                    }

                    builder.AppendLine("    }");
                    builder.AppendLine();
                    Models.Add(result);
                }
                else
                {
                    foreach (var rec in _item.Returns.Record.OrderBy(r => r.Ordinal))
                    {
                        resultFields.Add(rec.Name.ToUpperCamelCase());
                    }
                }
            }
            else
            {
                result = _isVoid ? "void" : GetType(_item.Returns, "result type");
            }

            builder.AppendLine($@"    public static class PgRoutine{name}
    {{");
            builder.AppendLine($"        public const string Name = \"{_item.Name}\";");
            builder.AppendLine();

            BuildSyncMethod(builder, result, name, resultFields);

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private void BuildSyncMethod(StringBuilder builder, string result, string name, IList<string> resultFields)
        {
            builder.AppendLine(BuildMethodComment());
            string resultType;
            if (!_noRecords || _singleRecordResult)
            {
                resultType = $"IEnumerable<{result}>";
            }
            else
            {
                resultType = result;
            }

            if (_noParams)
            {
                builder.AppendLine($@"        public static {resultType} {name}(this NpgsqlConnection connection)
        {{");
            }
            else
            {
                builder.AppendLine(
                    $@"        public static {resultType} {name}(this NpgsqlConnection connection, {BuildMethodParams(_item.Parameters)})
        {{");
            }

            builder.Append("            ");

            if (!_isVoid)
            {
                if (_noRecords)
                {
                    builder.Append(_singleRecordResult
                        ? $"return connection{NL}                .Read<{GetType(_item.Returns, "result type")}>("
                        : $"return connection{NL}                .Single<{GetType(_item.Returns, "result type")}>(");
                }
                else
                {
                    builder.Append(_recordCount <= MaxRecords
                        ? $"return connection{NL}                .Read<{BuildGenericTypes(_item.Returns.Record)}>("
                        : $"return connection{NL}                .Read(");
                }
            }
            else
            {
                builder.Append($"connection{NL}                .Execute(");
            }

            var parameters = BuildRoutineParams(_item.Parameters);
            builder.Append(_noParams ? "Name)" : $"Name, {parameters})");

            if (_isVoid || _noRecords || _singleRecordResult)
            {
                builder.AppendLine(";");
            }
            else
            {
                builder.AppendLine();
                builder.Append("                ");

                if (_recordCount <= MaxRecords)
                {

                    builder.AppendLine($".Select(tuple => new {result}");
                    builder.AppendLine("                {");
                    builder.Append(string.Join($",{NL}",
                            resultFields.Select((r, index) => string.Concat("                    ", r, " = ", "tuple.Item", index + 1))
                        )
                    );
                    builder.AppendLine();
                    builder.AppendLine("                });");
                }
                else
                {
                    builder.AppendLine($".Select<{result}>();");
                }

            }

            builder.AppendLine("        }");
        }


        private string GetType(PgBaseType pgType, string description)
        {
            if (Settings.TypeMapping.TryGetValue(pgType.Type, out var result))
            {
                return result;
            }
            throw new ArgumentException($"Could not find mapping \"{pgType.Type}\" for {description}, routine \"{_item.Name}\"");
        }

        private string BuildMethodParams(IEnumerable<PgType> parameters)
        {
            return string.Join(", ", parameters.Select(p => string.Concat(
                GetType(p, $"parameter for \"{_item.Name}\" at position {p.Ordinal}"), 
                " ", 
                p.Name.ToCamelCase())));
        }

        private string BuildRoutineParams(IEnumerable<PgType> parameters)
        {
            return string.Join(", ", parameters.Select(p => string.Concat(
                "(\"", 
                p.Name,
                "\", ",
                p.Name.ToCamelCase(),
                ")")));
        }

        private string BuildGenericTypes(IEnumerable<PgType> parameters)
        {
            return string.Join(", ", parameters.Select(p => GetType(p, $"parameter for \"{_item.Name}\" at position {p.Ordinal}")));
        }

        private string BuildMethodComment()
        {
            return @$"        /// <summary>
        /// {_item.Language} {_item.RoutineType} ""{_item.Name}""{(string.IsNullOrWhiteSpace(_item.Description) ? "" : string.Concat($"{NL}        /// ", _item.Description))}
        /// </summary>";
        }
    }
}
