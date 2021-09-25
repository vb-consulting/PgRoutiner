using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudCreateReturningCode : CrudCodeBase
    {
        public CrudCreateReturningCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "CreateReturning")
        {
            this.Params = new()
            {
                new Param(settings)
                {
                    PgName = "model",
                    PgType = this.Name,
                    Type = this.Model,
                    IsInstance = true
                }
            };
            Build();
            if (!this.PkParams.Any())
            {
                throw new ArgumentException($"Table {this.Table} does not have any primary keys!");
            }
        }

        protected override void AddSql()
        {
            Class.AppendLine($"{I2}public static string Sql({this.Model} model) => $@\"");
            Class.AppendLine($"{I3}INSERT INTO {this.Table}");
            Class.AppendLine($"{I3}(");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Select(c => $"{I4}\"\"{c.Name}\"\"")));
            Class.AppendLine($"{I3})");
            var identites = this.Columns.Where(c => c.IsIdentity).Select(c => $"model.{c.Name.ToUpperCamelCase()}").ToArray();
            string exp;
            if (identites.Any())
            {
                exp = identites.Length == 1 ? $"{identites[0]} != default" : $"({string.Join(" || ", identites.Select(i => $"{i} != default"))})";
                Class.AppendLine($"{I3}{{({exp} ? \"OVERRIDING SYSTEM VALUE\" : \"\")}}");
            }
            Class.AppendLine($"{I3}VALUES");
            Class.AppendLine($"{I3}(");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Select(c =>
            {
                var p = $"@{c.Name.ToCamelCase()}";
                if (c.HasDefault || c.IsIdentity)
                {
                    return $"{I4}{{(model.{c.Name.ToUpperCamelCase()} == default ? \"DEFAULT\" : \"{p}\")}}";
                }
                return $"{I4}{p}";
            })));
            Class.AppendLine($"{I3})");
            Class.AppendLine($"{I3}RETURNING{NL}{string.Join($",{NL}", this.Columns.Select(c => $"{I4}\"\"{c.Name}\"\""))}\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"Create{this.Name}Returning";
            var returnMethod = GetReturnMethod(name);
            var actualReturns = returnMethod == null ? $"IEnumerable<{this.Model}>" : this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader(returnMethod == null);
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.Read<{this.Model}>(Sql(model)");
            Class.AppendLine(",");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I5}(\"{p.Name}\", model.{p.ClassName}, {p.DbType})")));

            if (returnMethod == null)
            {
                Class.AppendLine($");");
            }
            else
            {
                Class.AppendLine($")");
                Class.AppendLine($"{I4}.{settings.ReturnMethod}();");
            }

            Class.AppendLine($"{I2}}}");
            AddMethod(name, actualReturns, true);
        }

        protected override void BuildStatementBodyAsyncMethod()
        {
            var name = $"Create{this.Name}ReturningAsync";
            var returnMethod = GetReturnMethod(name);
            var actualReturns = returnMethod == null ? $"IAsyncEnumerable<{this.Model}>" : $"async ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildAsyncMethodCommentHeader(returnMethod == null);
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return {(returnMethod == null ? "" : "await")} connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.ReadAsync<{this.Model}>(Sql(model)");
            Class.AppendLine(",");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I5}(\"{p.Name}\", model.{p.ClassName}, {p.DbType})")));

            if (returnMethod == null)
            {
                Class.AppendLine($");");
            }
            else
            {
                Class.AppendLine($")");
                Class.AppendLine($"{I4}.{settings.ReturnMethod}Async();");
            }

            Class.AppendLine($"{I2}}}");
            AddMethod(name, actualReturns, false);
        }

        protected override void BuildExpressionBodySyncMethod()
        {
            var name = $"Create{this.Name}Returning";
            var returnMethod = GetReturnMethod(name);
            var actualReturns = returnMethod == null ? $"IEnumerable<{this.Model}>" : this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader(returnMethod == null);
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.Read<{this.Model}>(Sql(model)");
            Class.AppendLine(",");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I4}(\"{p.Name}\", model.{p.ClassName}, {p.DbType})")));

            if (returnMethod == null)
            {
                Class.AppendLine($");");
            }
            else
            {
                Class.AppendLine($")");
                Class.AppendLine($"{I3}.{settings.ReturnMethod}();");
            }

            AddMethod(name, actualReturns, true);
        }

        protected override void BuildExpressionBodyAsyncMethod()
        {
            var name = $"Create{this.Name}ReturningAsync";
            var returnMethod = GetReturnMethod(name);
            var actualReturns = returnMethod == null ? $"IAsyncEnumerable<{this.Model}>" : $"async ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildAsyncMethodCommentHeader(returnMethod == null);
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => {(returnMethod == null ? "" : "await")} connection");

            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.ReadAsync<{this.Model}>(Sql(model)");
            Class.AppendLine(",");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I4}(\"{p.Name}\", model.{p.ClassName}, {p.DbType})")));

            if (returnMethod == null)
            {
                Class.AppendLine($");");
            }
            else
            {
                Class.AppendLine($")");
                Class.AppendLine($"{I3}.{settings.ReturnMethod}Async();");
            }

            AddMethod(name, actualReturns, false);
        }

        private void BuildSyncMethodCommentHeader(bool enumerable)
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Insert new record in table {this.Table} with values instance of a \"{Namespace}.{Model}\" class and return updated record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have the default when null value is supplied.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            if (!enumerable)
            {
                Class.AppendLine($"{I2}/// <returns>Single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
            }
            else
            {
                Class.AppendLine($"{I2}/// <returns>Enumerable of instances of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
            }
        }

        private void BuildAsyncMethodCommentHeader(bool enumerable)
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously insert new record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class and return updated record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have the default when null value is supplied.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            if (!enumerable)
            {
                Class.AppendLine($"{I2}/// <returns>ValueTask whose Result property is a single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
            }
            else
            {
                Class.AppendLine($"{I2}/// <returns>Async Enumerable of instances of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
            }
        }

        private void AddMethod(string name, string actualReturns, bool sync)
        {
            Methods.Add(new Method
            {
                Name = name,
                Namespace = Namespace,
                Params = this.Params,
                Returns = new Return { PgName = this.Name, Name = this.Model, IsVoid = false, IsEnumerable = false },
                ActualReturns = actualReturns,
                Sync = sync
            });
        }
    }
}
