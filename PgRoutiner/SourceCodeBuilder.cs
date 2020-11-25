using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class SourceCodeBuilder
    {
        public string Content { get; }
        public string ModelContent { get; private set; }
        public string ModelName { get; private set; }

        private readonly string NL = Environment.NewLine;
        private readonly Settings _settings;
        private readonly GetRoutinesResult _item;
        private readonly string _namespace;

        private const int MaxRecords = 12;
        private static readonly HashSet<string> Models = new HashSet<string>();

        private IEnumerable<PgType> _parameters;
        private PgReturns _returns;
        private bool _isVoid;
        private bool _noRecords;
        private bool _noParams;
        private bool _singleRecordResult;
        private int _recordCount;
        private readonly StringBuilder _modelBuilder = new StringBuilder(); 

        public SourceCodeBuilder(Settings settings, GetRoutinesResult item)
        {
            _settings = settings;
            _item = item;
            _namespace = settings.Namespace;

            if (!string.IsNullOrEmpty(settings.OutputDir))
            {
                _namespace = string.Concat(_namespace, ".", settings.OutputDir.Replace("/", ".").Replace("\\", "."));
            }

            this.Content = this.Build();
        }

        private string Build()
        {
            var modelBuilderTag = "{modelBuilderTag}";
            var extraNsTag = "{extraNsTag}";

            var name = _item.Name.ToUpperCamelCase();
            var builder = CreateModule(_namespace, extraNsTag);

            string customModel = null;
            _settings.CustomModels?.TryGetValue(_item.Name, out customModel);


            builder.Append(modelBuilderTag); // result classes placeholder

            OpenClass(builder, $"PgRoutine{name}", true);
            builder.AppendLine($"        public const string Name = \"{_item.Name}\";");
            builder.AppendLine();

            var count = _item.Parameters.Count();
            var i = 0;
            var modelsCount = 0;
            var totalModels = _item.Returns.Count(r => r.Record != null && r.Record.Any());
            foreach (var itemParameter in _item.Parameters)
            {
                _parameters = itemParameter;
                _returns = _item.Returns[i];
                i++;
                _isVoid = _returns.Type == "void";
                _noRecords = _returns.Record == null || !_returns.Record.Any();
                _noParams = _parameters == null || !_parameters.Any();
                _singleRecordResult = !_noRecords && (_returns.Record != null && _returns.Record.Count() == 1);
                _recordCount = _returns.Record?.Count() ?? 0;

                string result;
                var resultFields = new List<string>();
                if (!_noRecords && !_singleRecordResult)
                {
                    modelsCount++;
                    result = _returns.UserDefined ? _returns.Type.ToUpperCamelCase() : $"{name}Result{(totalModels == 1 ? "" : modelsCount.ToString())}";
                    if (customModel != null)
                    {
                        result = customModel;
                    }
                    else
                    {
                        if (ModelName == null)
                        {
                            ModelName = result;
                        }
                        if (!Models.Contains(result))
                        {

                            OpenClass(_modelBuilder, result);
                            foreach (var rec in _returns.Record.OrderBy(r => r.Ordinal))
                            {
                                var type = GetType(rec, $"result type \"{rec.Name}\"");
                                var fieldName = rec.Name.ToUpperCamelCase();
                                _modelBuilder.AppendLine($"        public {type} {fieldName} {{ get; set; }}");
                                resultFields.Add(fieldName);
                            }

                            CloseClass(_modelBuilder);
                            _modelBuilder.AppendLine();
                            Models.Add(result);
                        }
                        else
                        {
                            foreach (var rec in _returns.Record.OrderBy(r => r.Ordinal))
                            {
                                resultFields.Add(rec.Name.ToUpperCamelCase());
                            }
                        }
                    }


                }
                else
                {
                    result = _isVoid ? "void" : GetType(_returns, "result type");
                }

                if (_settings.SyncMethod)
                {
                    BuildSyncMethod(builder, result, name, resultFields, i-1);
                }
                if (_settings.AsyncMethod)
                {
                    if (_settings.SyncMethod)
                    {
                        builder.AppendLine();
                    }
                    BuildAsyncMethod(builder, result, name, resultFields, i-1);
                }


                if (i < count)
                {
                    builder.AppendLine();
                }
            }

            CloseClass(builder);
            var content = CloseModule(builder);
            var modelContent = _modelBuilder.ToString();
            if (_settings.ModelDir == null)
            {
                return content.Replace(modelBuilderTag, modelContent).Replace(extraNsTag, "");
            }

            var modelNamespace = "";
            if (string.IsNullOrEmpty(modelContent))
            {
                return content.Replace(modelBuilderTag, "").Replace(extraNsTag, modelNamespace);
            }

            modelNamespace = string.Concat(_settings.Namespace, ".", _settings.ModelDir.Replace("/", ".").Replace("\\", "."));

            var modelBuilder = CreateModule(modelNamespace);
            modelNamespace = string.Concat("using ", modelNamespace, ";", NL);
            modelContent = TrimEnd(modelContent, NL);
            modelBuilder.AppendLine(modelContent);
            ModelContent = CloseModule(modelBuilder);

            return content.Replace(modelBuilderTag, "").Replace(extraNsTag, modelNamespace);
        }

        private void BuildSyncMethod(StringBuilder builder, string result, string name, IList<string> resultFields, int index)
        {
            builder.AppendLine(BuildMethodComment(index));
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
                    $@"        public static {resultType} {name}(this NpgsqlConnection connection, {BuildMethodParams(_parameters)})
        {{");
            }

            builder.Append("            ");

            if (!_isVoid)
            {
                if (_noRecords)
                {
                    builder.Append(_singleRecordResult
                        ? $"return connection{NL}                .AsProcedure(){NL}                .Read<{GetType(_returns, "result type")}>("
                        : $"return connection{NL}                .AsProcedure(){NL}                .Single<{GetType(_returns, "result type")}>(");
                }
                else
                {
                    builder.Append(_recordCount <= MaxRecords
                        ? $"return connection{NL}                .AsProcedure(){NL}                .Read<{BuildGenericTypes(_returns.Record)}>("
                        : $"return connection{NL}                .AsProcedure(){NL}                .Read(");
                }
            }
            else
            {
                builder.Append($"connection{NL}                .AsProcedure(){NL}                .Execute(");
            }

            var parameters = BuildRoutineParams(_parameters);
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

        private void BuildAsyncMethod(StringBuilder builder, string result, string name, IList<string> resultFields, int index)
        {
            builder.AppendLine(BuildMethodComment(index));
            string resultType;

            bool enumerable = false;
            if (!_noRecords || _singleRecordResult)
            {
                resultType = $"IAsyncEnumerable<{result}>";
                enumerable = true;
            }
            else
            {
                resultType = _isVoid ? "ValueTask" : $"ValueTask<{result}>";
            }

            if (_noParams)
            {
                builder.AppendLine($@"        public static {(!enumerable ? "async" : "")} {resultType} {name}Async(this NpgsqlConnection connection)
        {{");
            }
            else
            {
                builder.AppendLine(
                    $@"        public static{(!enumerable ? " async" : "")} {resultType} {name}Async(this NpgsqlConnection connection, {BuildMethodParams(_parameters)})
        {{");
            }

            builder.Append("            ");

            if (!_isVoid)
            {
                if (_noRecords)
                {
                    builder.Append(_singleRecordResult
                        ? $"return {(!enumerable ? "await" : "")} connection{NL}                .AsProcedure(){NL}                .ReadAsync<{GetType(_returns, "result type")}>("
                        : $"return {(!enumerable ? "await" : "")} connection{NL}                .AsProcedure(){NL}                .SingleAsync<{GetType(_returns, "result type")}>(");
                }
                else
                {
                    builder.Append(_recordCount <= MaxRecords
                        ? $"return {(!enumerable ? "await" : "")} connection{NL}                .AsProcedure(){NL}                .ReadAsync<{BuildGenericTypes(_returns.Record)}>("
                        : $"return {(!enumerable ? "await" : "")} connection{NL}                .AsProcedure(){NL}                .ReadAsync(");
                }
            }
            else
            {
                builder.Append($"await connection{NL}                .AsProcedure(){NL}                .ExecuteAsync(");
            }

            var parameters = BuildRoutineParams(_parameters);
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

        private static void CloseClass(StringBuilder builder)
        {
            builder.AppendLine("    }");
        }

        private static void OpenClass(StringBuilder builder, string className, bool isStatic = false)
        {
            builder.AppendLine($@"    public{(isStatic ? " static" : "")} class {className}
    {{");
        }

        private static string CloseModule(StringBuilder builder)
        {
            builder.AppendLine("}");
            return builder.ToString();
        }

        private StringBuilder CreateModule(string ns, string extra = "")
        {
            var builder = new StringBuilder($@"{string.Format(_settings.SourceHeader, DateTime.Now.ToString("O"))}
using System;
using System.Linq;
using System.Collections.Generic;{(_settings.AsyncMethod ? string.Concat(NL, "using System.Threading.Tasks;") : "")}
using Norm.Extensions;
using NpgsqlTypes;
using Npgsql;
{extra}
namespace {ns}
{{
");
            return builder;
        }

        private string GetType(PgBaseType pgType, string description)
        {
            if (Settings.TypeMapping.TryGetValue(pgType.Type, out var result))
            {
                if (pgType.Array)
                {
                    return string.Concat(result, "[]");
                }
                if (result != "string" && ((pgType.Nullable.HasValue && pgType.Nullable == true) || !pgType.Nullable.HasValue))
                {
                    result = string.Concat(result, "?");
                }
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
            return string.Join(", ", parameters.Select(p => {
                var type = "NpgsqlDbType.";
                if (ParamTypeMapping.TryGetValue(p.Type, out var map))
                {
                    type = string.Concat(type, map.Item1);
                    if (p.Array)
                    {
                        type = string.Concat("NpgsqlDbType.Array | ", type);
                    }
                    if (map.Item2)
                    {
                        type = string.Concat("NpgsqlDbType.Range | ", type);
                    }
                }
                else
                {
                    type = string.Concat(type, "Unknown");
                }
                return string.Concat("(\"", p.Name, "\", ", p.Name.ToCamelCase(), ", ", type, ")");
            }));
        }

        private string BuildGenericTypes(IEnumerable<PgType> parameters)
        {
            return string.Join(", ", parameters.Select(p => GetType(p, $"parameter for \"{_item.Name}\" at position {p.Ordinal}")));
        }

        private string BuildMethodComment(int index)
        {
            var desc = _item.Description[index];
            var lan = _item.Language[index];
            var rt = _item.RoutineType[index];
            return @$"        /// <summary>
        /// {lan} {rt} ""{_item.Name}""{(string.IsNullOrWhiteSpace(desc) ? "" : string.Concat($"{NL}        /// ", desc))}
        /// </summary>";
        }

        public string TrimEnd(string inputText, string value, StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase)
        {
            if (!string.IsNullOrEmpty(value))
            {
                while (!string.IsNullOrEmpty(inputText) && inputText.EndsWith(value, comparisonType))
                {
                    inputText = inputText.Substring(0, (inputText.Length - value.Length));
                }
            }

            return inputText;
        }

        private static readonly IDictionary<string, (string, bool)> ParamTypeMapping = new Dictionary<string, (string, bool)>
        {
            {"refcursor", ("Refcursor", false)},
            {"tsvector", ("TsVector", false)},
            {"cidr", ("Cidr", false)},
            {"timestamptz", ("TimestampTz", false)},
            {"name", ("Name", false)},
            {"inet", ("Inet", false)},
            {"lseg", ("Lseg", false)},
            {"int8", ("Bigint", false)},
            {"_char", ("Char", false)},
            {"unknown", ("Unknown", false)},
            {"tsquery", ("TsQuery", false)},
            {"float4", ("Real", false)},
            {"timestamp", ("Timestamp", false)},
            {"gtsvector", ("TsVector", false)},
            {"circle", ("Circle", false)},
            {"numeric", ("Numeric", false)},
            {"pg_type", ("Regtype", false)},
            {"regconfig", ("Regconfig", false)},
            {"timetz", ("TimeTZ", false)},
            {"daterange", ("Date", true)},
            {"box", ("Box", false)},
            {"_float4", ("Real", false)},
            {"int4range", ("Integer", true)},
            {"cid", ("Cid", false)},
            {"_regtype", ("Regtype", false)},
            {"_varchar", ("Varchar", false)},
            {"_text", ("Text", false)},
            {"date", ("Date", false)},
            {"xid", ("Xid", false)},
            {"bool", ("Boolean", false)},
            {"_oid", ("Oid", false)},
            {"polygon", ("Polygon", false)},
            {"time", ("Time", false)},
            {"int2vector", ("Int2Vector", false)},
            {"_int4", ("Integer", false)},
            {"int4", ("Integer", false)},
            {"_interval", ("Interval", false)},
            {"_int8", ("Bigint", false)},
            {"int8range", ("Bigint", true)},
            {"interval", ("Interval", false)},
            {"xml", ("Xml", false)},
            {"char", ("Char", false)},
            {"macaddr8", ("MacAddr8", false)},
            {"varchar", ("Varchar", false)},
            {"float8", ("Double", false)},
            {"json", ("Json", false)},
            {"_name", ("Name", false)},
            {"money", ("Money", false)},
            {"text", ("Text", false)},
            {"_float8", ("Double", false)},
            {"regtype", ("Regtype", false)},
            {"bit", ("Bit", false)},
            {"tid", ("Tid", false)},
            {"line", ("Line", false)},
            {"oidvector", ("Oidvector", false)},
            {"int2", ("Smallint", false)},
            {"uuid", ("Uuid", false)},
            {"path", ("Path", false)},
            {"jsonb", ("Jsonb", false)},
            {"bytea", ("Bytea", false)},
            {"_bool", ("Boolean", false)},
            {"macaddr", ("MacAddr", false)},
            {"point", ("Point", false)},
            {"varbit", ("Varbit", false)},
            {"oid", ("Oid", false)},
            {"_int2", ("Smallint", false)}
        };
    }
}
