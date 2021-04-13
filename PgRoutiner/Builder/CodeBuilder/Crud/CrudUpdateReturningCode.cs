using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudUpdateReturningCode : CrudCodeBase
    {
        public CrudUpdateReturningCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "UpdateReturning")
        {
            if (!this.Pk.Any())
            {
                throw new ArgumentException($"Table {this.Table} does not have any primary keys!");
            }
        }

        protected override void AddQuery()
        {
            Class.AppendLine($"{I2}public const string Query = @\"");
            Class.AppendLine($"{I3}update {this.Table}");
            Class.AppendLine($"{I3}set");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Where(c => !c.IsPk).Select(c =>
            {
                var p = $"@{c.Name.ToCamelCase()}";
                if (c.HasDefault)
                {
                    return $"{I4}[{c.Name}] = case when {p} is null then DEFAULT else {p} end";
                }
                return $"{I4}[{c.Name}] = {p}";
            })));
            Class.Append($"{I3}where{NL}{I4}");
            Class.AppendLine(string.Join($"{NL}{I1}and ", this.Pk.Select(c => $"[{c.PgName}] = @{c.Name}")));
            Class.AppendLine($"{I3}returning{NL}{string.Join($",{NL}", this.Columns.Select(c => $"{I4}[{c.Name}]"))}\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"UpdateReturning{Name.ToUpperCamelCase()}";
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
            Class.Append($"{I4}.Read<{this.Model}>(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I5}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I4}.Single();");
            Class.AppendLine($"{I2}}}");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, true));
        }

        protected override void BuildStatementBodyAsyncMethod()
        {
            var name = $"UpdateReturning{Name.ToUpperCamelCase()}Async";
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
            Class.Append($"{I4}.ReadAsync<{this.Model}>(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I5}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I4}.SingleAsync();");
            Class.AppendLine($"{I2}}}");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, false));
        }

        protected override void BuildExpressionBodySyncMethod()
        {
            var name = $"UpdateReturning{Name.ToUpperCamelCase()}";
            var actualReturns = this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.Read<{this.Model}>(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I4}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I3}.Single();");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, true));
        }

        protected override void BuildExpressionBodyAsyncMethod()
        {
            var name = $"UpdateReturning{Name.ToUpperCamelCase()}Async";
            var actualReturns = $"ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static async {actualReturns} {name}(this NpgsqlConnection connection, {this.Model} model) => await connection");

            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.ReadAsync<{this.Model}>(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I4}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($")");
            Class.AppendLine($"{I3}.SingleAsync();");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, false));
        }

        protected override void BuildSyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Update record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class by matching values of key fields: {string.Join(", ", this.Pk.Select(p => p.Name))} and return updated record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            Class.AppendLine($"{I2}/// <returns>Single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
        }

        protected override void BuildAsyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously update record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class by matching values of key fields: {string.Join(", ", this.Pk.Select(p => p.Name))} and return updated record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            Class.AppendLine($"{I2}/// <returns>ValueTask whose Result property is a single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
        }
    }
}
