using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudReadAllCode : CrudCodeBase
    {
        public CrudReadAllCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "ReadAll")
        {
        }

        protected override void AddQuery()
        {
            Class.AppendLine($"{I2}public const string Query = @\"");
            Class.AppendLine($"{I3}select");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Select(c => $"{I4}[{c.Name}]")));
            Class.AppendLine($"{I3}from");
            Class.AppendLine($"{I4}{this.Table}\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}All";
            var actualReturns = $"IEnumerable<{this.Model}>";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.AppendLine($"{I4}.Read<{this.Model}>(Query);");
            Class.AppendLine($"{I2}}}");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, true));
        }

        protected override void BuildStatementBodyAsyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}AllAsync";
            var actualReturns = $"IAsyncEnumerable<{this.Model}>";
            Class.AppendLine();
            BuildAsyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection)");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.AppendLine($"{I4}.ReadAsync<{this.Model}>(Query);");
            Class.AppendLine($"{I2}}}");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, false));
        }

        protected override void BuildExpressionBodySyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}All";
            var actualReturns = $"IEnumerable<{this.Model}>";
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.AppendLine($"{I3}.Read<{this.Model}>(Query);");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, true));
        }

        protected override void BuildExpressionBodyAsyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}AllAsync";
            var actualReturns = $"IAsyncEnumerable<{this.Model}>";
            Class.AppendLine();
            BuildAsyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.AppendLine($"{I3}.ReadAsync<{this.Model}>(Query);");
            Methods.Add(new Method(name, Namespace, Pk, new Return(this.Name, name, false, true), actualReturns, false));
        }

        protected override void BuildSyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Select table {this.Table} and return enumerator of instances of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <returns>Single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
        }

        protected override void BuildAsyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously select table {this.Table} and return enumerator of instances of a \"{this.Model}\" class.");
            Class.AppendLine($"{I2}/// </summary>");
            Class.AppendLine($"{I2}/// <returns>IAsyncEnumerable of \"{Namespace}.{Model}\" instances.</returns>");
        }
    }
}
