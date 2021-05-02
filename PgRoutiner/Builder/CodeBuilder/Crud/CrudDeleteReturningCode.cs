using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudDeleteReturningCode : CrudCodeBase
    {
        public CrudDeleteReturningCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "DeleteReturning")
        {
            this.Params = new()
            {
                new Param
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
            Class.AppendLine($"{I2}public const string Sql = @\"");
            Class.AppendLine($"{I3}DELETE FROM {this.Table}");
            Class.Append($"{I3}WHERE{NL}{I4}");
            Class.AppendLine(string.Join($"{NL}{I1}AND ", this.PkParams.Select(c => $"\"\"{c.PgName}\"\" = @{c.Name}")));
            Class.AppendLine($"{I3}RETURNING{NL}{string.Join($",{NL}", this.Columns.Select(c => $"{I4}\"\"{c.Name}\"\""))}\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"Delete{this.Name}Returning";
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
            Class.Append($"{I4}.Read<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.PkParams.Select(p => $"{I5}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));

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
            var name = $"Delete{this.Name}ReturningAsync";
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
            Class.Append($"{I4}.ReadAsync<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.PkParams.Select(p => $"{I5}(\"{p.Name}\", model.{p.ClassName}, {p.DbType})")));

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
            var name = $"Delete{this.Name}Returning";
            var returnMethod = GetReturnMethod(name);
            var actualReturns = returnMethod == null ? $"IEnumerable<{this.Model}>" : this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader(returnMethod == null);
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.Read<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.PkParams.Select(p => $"{I5}(\"{p.Name}\", model.{p.ClassName}, {p.DbType})")));

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
            var name = $"Delete{this.Name}ReturningAsync";
            var returnMethod = GetReturnMethod(name);
            var actualReturns = returnMethod == null ? $"IAsyncEnumerable<{this.Model}>" : $"async ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildAsyncMethodCommentHeader(returnMethod == null);
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => {(returnMethod == null ? "" : "await")} connection");

            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.ReadAsync<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.PkParams.Select(p => $"{I5}(\"{p.Name}\", model.{p.ClassName}, {p.DbType})")));

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
            Class.AppendLine($"{I2}/// Delete record of table {this.Table} by matching values of key fields: {string.Join(", ", this.PkParams.Select(p => p.Name))} and return deleted record mapped to an instance of a \"{Namespace}.{Model}\" class.");
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
            Class.AppendLine($"{I2}/// Asynchronously delete record of table {this.Table} by matching values of key fields: {string.Join(", ", this.PkParams.Select(p => p.Name))} and return deleted record mapped to an instance of a \"{Namespace}.{Model}\" class.");
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
