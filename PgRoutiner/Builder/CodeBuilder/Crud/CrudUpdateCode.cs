using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudUpdateCode : CrudCodeBase
    {
        private readonly List<Param> Params;

        public CrudUpdateCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "Update")
        {
            this.Params = new()
            {
                new Param
                {
                    PgName = "model",
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
            Class.AppendLine($"{I3}UPDATE {this.Table}");
            Class.AppendLine($"{I3}SET");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Where(c => !c.IsPk).Select(c =>
            {
                var p = $"@{c.Name.ToCamelCase()}";
                if (c.HasDefault)
                {
                    return $"{I4}[{c.Name}] = CASE WHEN {p} IS NULL THEN DEFAULT ELSE {p} END";
                }
                return $"{I4}[{c.Name}] = {p}";
            })));
            Class.Append($"{I3}WHERE{NL}{I4}");
            Class.Append(string.Join($"{NL}{I1}AND ", this.PkParams.Select(c => $"[{c.PgName}] = @{c.Name}")));
            Class.AppendLine($"\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"Update{Name.ToUpperCamelCase()}";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static void {name}(this NpgsqlConnection connection, {this.Model} model)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.Execute(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I5}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($");");
            Class.AppendLine($"{I2}}}");
            AddMethod(name, true);
        }

        protected override void BuildStatementBodyAsyncMethod()
        {
            var name = $"Update{Name.ToUpperCamelCase()}Async";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static async ValueTask {name}(this NpgsqlConnection connection, {this.Model} model)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}await connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.ExecuteAsync(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I5}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($");");
            Class.AppendLine($"{I2}}}");
            AddMethod(name, false);
        }

        protected override void BuildExpressionBodySyncMethod()
        {
            var name = $"Update{Name.ToUpperCamelCase()}";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static void {name}(this NpgsqlConnection connection, {this.Model} model) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.Execute(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I4}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($");");
            AddMethod(name, true);
        }

        protected override void BuildExpressionBodyAsyncMethod()
        {
            var name = $"Update{Name.ToUpperCamelCase()}Async";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static async ValueTask {name}(this NpgsqlConnection connection, {this.Model} model) => await connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.ExecuteAsync(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I4}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($");");
            AddMethod(name, false);
        }

        protected override void BuildSyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Update record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class by matching values of key fields: {string.Join(", ", this.PkParams.Select(p => p.Name))}");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have the default when null value is supplied.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
        }

        protected override void BuildAsyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously update record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class by matching values of key fields: {string.Join(", ", this.PkParams.Select(p => p.Name))}");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have the default when null value is supplied.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            Class.AppendLine($"{I2}/// <returns>ValueTask without result.</returns>");
        }

        private void AddMethod(string name, bool sync)
        {
            Methods.Add(new Method
            {
                Name = name,
                Namespace = Namespace,
                Params = this.Params,
                Returns = new Return { PgName = "void", Name = "void", IsVoid = true, IsInstance = true },
                ActualReturns = "void",
                Sync = sync
            });
        }
    }
}
