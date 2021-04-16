using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class CrudReadByCode : CrudCodeBase
    {
        public CrudReadByCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "ReadBy")
        {
            Build();
            if (!this.PkParams.Any())
            {
                throw new ArgumentException($"Table {this.Table} does not have any primary keys!");
            }
        }

        protected override void AddSql()
        {
            Class.AppendLine($"{I2}public const string Sql = @\"");
            Class.AppendLine($"{I3}SELECT");
            Class.AppendLine(string.Join($",{NL}", this.Columns.Select(c => $"{I4}[{c.Name}]")));
            Class.AppendLine($"{I3}FROM");
            Class.AppendLine($"{I4}{this.Table}");
            Class.Append($"{I3}WHERE{NL}{I4}");
            Class.Append(string.Join($"{NL}{I1}AND ", this.PkParams.Select(c => $"[{c.PgName}] = @{c.Name}")));
            Class.AppendLine($"\";");
        }

        protected override void BuildStatementBodySyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()))}";
            var actualReturns = this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}"))})");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.Read<{this.Model}>(Sql");

            if (PkParams.Count > 0)
            {
                Class.AppendLine(", ");
                Class.Append(string.Join($",{NL}", PkParams.Select(p => $"{I5}(\"{p.PgName}\", {p.Name}, {p.DbType})")));
            }
            Class.AppendLine($")");
            Class.AppendLine($"{I4}.{settings.SingleLinqMethod}();");
            Class.AppendLine($"{I2}}}");
            NewMethod(name, actualReturns, true);
        }

        protected override void BuildStatementBodyAsyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()))}Async";
            var actualReturns = $"ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildAsyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static async {actualReturns} {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}"))})");
            Class.AppendLine($"{I2}{{");
            Class.AppendLine($"{I3}return await connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.ReadAsync<{this.Model}>(Sql");

            if (PkParams.Count > 0)
            {
                Class.AppendLine(", ");
                Class.Append(string.Join($",{NL}", PkParams.Select(p => $"{I5}(\"{p.PgName}\", {p.Name}, {p.DbType})")));
            }
            Class.AppendLine($")");
            Class.AppendLine($"{I4}.{settings.SingleLinqMethod}Async();");
            Class.AppendLine($"{I2}}}");
            NewMethod(name, actualReturns, false);
        }

        protected override void BuildExpressionBodySyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()).ToArray())}";
            var actualReturns = this.Model;
            Class.AppendLine();
            BuildSyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}").ToArray())}) => connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.Read<{this.Model}>(Sql");

            if (PkParams.Count > 0)
            {
                Class.AppendLine(", ");
                Class.Append(string.Join($",{NL}", PkParams.Select(p => $"{I4}(\"{p.PgName}\", {p.Name}, {p.DbType})")));
            }
            Class.AppendLine($")");
            Class.AppendLine($"{I3}.{settings.SingleLinqMethod}();");
            NewMethod(name, actualReturns, true);
        }

        protected override void BuildExpressionBodyAsyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()).ToArray())}Async";
            var actualReturns = $"ValueTask<{this.Model}>";
            Class.AppendLine();
            BuildAsyncMethodCommentHeader();
            Class.AppendLine($"{I2}public static async {actualReturns} {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}"))}) => await connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I3}.Prepared()");
            }
            Class.Append($"{I3}.ReadAsync<{this.Model}>(Sql");

            if (PkParams.Count > 0)
            {
                Class.AppendLine(", ");
                Class.Append(string.Join($",{NL}", PkParams.Select(p => $"{I4}(\"{p.PgName}\", {p.Name}, {p.DbType})")));
            }
            Class.AppendLine($")");
            Class.AppendLine($"{I3}.{settings.SingleLinqMethod}Async();");
            NewMethod(name, actualReturns, false);
        }

        protected override void BuildSyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Select table {this.Table} by primary keys and return a single record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// </summary>");
            foreach (var p in this.PkParams)
            {
                Class.AppendLine($"{I2}/// <param name=\"{p.Name}\">Select table {this.Table} where field {p.PgName} {p.PgType} is this value.</param>");
            }
            Class.AppendLine($"{I2}/// <returns>Single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
        }

        protected override void BuildAsyncMethodCommentHeader()
        {
            Class.AppendLine($"{I2}/// <summary>");
            Class.AppendLine($"{I2}/// Asynchronously select table {this.Table} by primary keys and return a single record mapped to an instance of a \"{Namespace}.{Model}\" class.");
            Class.AppendLine($"{I2}/// </summary>");
            foreach (var p in this.PkParams)
            {
                Class.AppendLine($"{I2}/// <param name=\"{p.Name}\">Select table {this.Table} where field {p.PgName} {p.PgType} is this value.</param>");
            }
            Class.AppendLine($"{I2}/// <returns>ValueTask whose Result property is a single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
        }

        private void NewMethod(string name, string actualReturns, bool sync)
        {
            Methods.Add(new Method
            {
                Name = name,
                Namespace = Namespace,
                Params = this.PkParams,
                Returns = new Return { PgName = this.Name, Name = this.Model, IsVoid = false, IsEnumerable = false },
                ActualReturns = actualReturns,
                Sync = sync
            });
        }
    }
}
