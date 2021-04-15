using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudCreateOnConflictDoNothingReturningCode : CrudCodeBase
    {
        public CrudCreateOnConflictDoNothingReturningCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "CreateOnConflictDoNothingReturning")
        {
            if (!this.PkParams.Any())
            {
                throw new ArgumentException($"Table {this.Table} does not have any primary keys!");
            }
        }

        protected override void AddSql()
        {
            Class.AppendLine($"{I2}public const string Sql = @\"");
            Class.AppendLine($"{I3}INSERT INTO {this.Table}");
            Class.AppendLine($"{I3}(");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Select(c => $"{I4}[{c.Name}]")));
            Class.AppendLine($"{I3})");
            if (this.Columns.Any(c => c.IsIdentity))
            {
                Class.AppendLine($"{I3}OVERRIDING SYSTEM VALUE");
            }
            Class.AppendLine($"{I3}VALUES");
            Class.AppendLine($"{I3}(");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Select(c =>
            {
                var p = $"@{c.Name.ToCamelCase()}";
                if (c.HasDefault || c.IsIdentity)
                {
                    return $"{I4}CASE WHEN {p} IS NULL THEN DEFAULT ELSE {p} END";
                }
                return $"{I4}{p}";
            })));
            Class.AppendLine($"{I3})");
            Class.AppendLine($"{I3}ON CONFLICT DO NOTHING");
            Class.AppendLine($"{I3}RETURNING{NL}{string.Join($",{NL}", this.Columns.Select(c => $"{I4}[{c.Name}]"))}\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"CreateOnConflictDoNothingReturning{Name.ToUpperCamelCase()}";
            var actualReturns = this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.Read<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I5}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I4}.Single();");
            Class.AppendLine($"{I2}}}");
            AddMethod(name, actualReturns, true);
        }

        protected override void BuildStatementBodyAsyncMethod()
        {
            var name = $"CreateOnConflictDoNothingReturning{Name.ToUpperCamelCase()}Async";
            var actualReturns = $"ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static async {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return await connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.ReadAsync<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I5}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I4}.SingleAsync();");
            Class.AppendLine($"{I2}}}");
            AddMethod(name, actualReturns, false);
        }

        protected override void BuildExpressionBodySyncMethod()
        {
            var name = $"CreateOnConflictDoNothingReturning{Name.ToUpperCamelCase()}";
            var actualReturns = this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.Read<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I4}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I3}.Single();");
            AddMethod(name, actualReturns, true);
        }

        protected override void BuildExpressionBodyAsyncMethod()
        {
            var name = $"CreateOnConflictDoNothingReturning{Name.ToUpperCamelCase()}Async";
            var actualReturns = $"ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static async {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => await connection");

            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.ReadAsync<{this.Model}>(Sql");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{I4}(\"{p.PgName}\", model.{p.ClassName}, {p.DbType})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I3}.SingleAsync();");
            AddMethod(name, actualReturns, false);
        }

        protected override void BuildSyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Insert new record in table {this.Table} with values instance of a \"{Namespace}.{Model}\" class and return updated record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have the default when null value is supplied.");
            Class.AppendLine($"{I2}/// When conflict occures, do nothing (skip).");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            Class.AppendLine($"{I2}/// <returns>Single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
        }

        protected override void BuildAsyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously insert new record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class and return updated record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have the default when null value is supplied.");
            Class.AppendLine($"{I2}/// When conflict occures, do nothing (skip).");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            Class.AppendLine($"{I2}/// <returns>ValueTask whose Result property is a single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
        }

        private void AddMethod(string name, string actualReturns, bool sync)
        {
            Methods.Add(new Method(name, Namespace, ColumnParams, new Return(this.Name, name, false, true), actualReturns, sync));
        }
    }
}
