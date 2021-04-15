﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudCreateCode : CrudCodeBase
    {
        public CrudCreateCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "Create")
        {
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
            Class.Append($"{I3})");
            Class.AppendLine($"\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"Create{Name.ToUpperCamelCase()}";
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
            var name = $"Create{Name.ToUpperCamelCase()}Async";
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
            var name = $"Create{Name.ToUpperCamelCase()}";
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
            var name = $"Create{Name.ToUpperCamelCase()}Async";
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
            Class.AppendLine($"{I2}/// Insert new record in table {this.Table} with values instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have tzhe default when null value is supplied.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
        }

        protected override void BuildAsyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously insert new record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// Fields with defined default values {string.Join(", ", this.Columns.Where(c => c.HasDefault || c.IsIdentity).Select(c => c.Name))} will have tzhe default when null value is supplied.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            Class.AppendLine($"{I2}/// <returns>ValueTask without result.</returns>");
        }

        private void AddMethod(string name, bool sync)
        {
            Methods.Add(new Method(name, Namespace, this.ColumnParams, new Return("void", "void", true, true), "void", sync));
        }
    }
}
