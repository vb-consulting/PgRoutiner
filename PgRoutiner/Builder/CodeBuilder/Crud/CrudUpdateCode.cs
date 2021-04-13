using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class CrudUpdateCode : CrudCodeBase
    {
        public CrudUpdateCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "Update")
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
            Class.AppendLine(string.Join($",{NL}", this.Columns.Where(c => !c.IsPk).Select(c => $"{I4}[{c.Name}] = @{c.Name.ToCamelCase()}")));
            Class.Append($"{I3}where{NL}{I4}");
            Class.Append(string.Join($"{NL}{I1}and ", this.Pk.Select(c => $"[{c.PgName}] = @{c.Name}")));
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
            Class.Append($"{I4}.Execute(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I5}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($");");
            Class.AppendLine($"{I2}}}");
            Methods.Add(new Method(name, Namespace, Pk, new Return("void", "void", true, true), "void", true));
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
            Class.Append($"{I4}.ExecuteAsync(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I5}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($");");
            Class.AppendLine($"{I2}}}");
            Methods.Add(new Method(name, Namespace, Pk, new Return("void", "void", true, true), "void", true));
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
            Class.Append($"{I3}.Execute(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I4}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($");");
            Methods.Add(new Method(name, Namespace, Pk, new Return("void", "void", true, true), "void", true));
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
            Class.Append($"{I3}.ExecuteAsync(Query");
            Class.AppendLine(", ");
            Class.Append(string.Join($",{NL}", this.Columns.Select(p => $"{I4}(\"{p.Name}\", model.{p.Name.ToUpperCamelCase()}, {GetParamDbType(p)})")));
            Class.AppendLine($");");
            Methods.Add(new Method(name, Namespace, Pk, new Return("void", "void", true, true), "void", true));
        }

        protected override void BuildSyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Update record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class by matching values of key fields: {string.Join(", ", this.Pk.Select(p => p.Name))}");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
        }

        protected override void BuildAsyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously update record of table {this.Table} with values instance of a \"{Namespace}.{Model}\" class by matching values of key fields: {string.Join(", ", this.Pk.Select(p => p.Name))}");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <param name=\"model\">Instance of a \"{Namespace}.{Model}\" model class.</param>");
            Class.AppendLine($"{I2}/// <returns>ValueTask without result.</returns>");
        }
    }
}
