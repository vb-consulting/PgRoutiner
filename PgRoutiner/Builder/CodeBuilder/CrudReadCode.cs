using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class CrudReadCode : Code
    {
        private readonly string @namespace;
        private readonly IEnumerable<PgColumnGroup> columns;
        private readonly List<Param> pk;
        private readonly string model;
        private readonly string table;

        public CrudReadCode(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns) : base(settings, item.name)
        {
            this.table = item.schema == "public" ? $"[{item.name}]" : $"{ item.schema}.[{item.name}]";
            this.@namespace = @namespace;
            this.columns = columns;
            this.pk = GetParamsFromPk();
            if (!this.pk.Any())
            {
                throw new ArgumentException($"Table {this.table} does not have any primary keys!");
            }
            this.model = BuildModel();
            Build();
        }

        private void Build()
        {
            Class.AppendLine($"{I1}public static class {Name.ToUpperCamelCase()}Read");
            Class.AppendLine($"{I1}{{");
            Class.AppendLine($"{I2}public const string Name = \"{this.table}\";");
            if (!settings.SkipSyncMethods)
            {
                if (settings.UseStatementBody)
                {
                    BuildStatementBodySyncMethod();
                }
                else
                {
                    //BuildExpressionBodySyncMethod();
                }
            }
            if (!settings.SkipAsyncMethods)
            {
                /*
                if (settings.UseStatementBody)
                {
                    BuildStatementBodyAsyncMethod();
                }
                else
                {
                    BuildExpressionBodyAsyncMethod();
                }
                */
            }
            Class.AppendLine($"{I1}}}");
        }

        private void BuildStatementBodySyncMethod()
        {
            var name = $"Read{Name.ToUpperCamelCase()}By{string.Join("And", pk.Select(p => p.Name.ToUpperCamelCase()).ToArray())}";
            var actualReturns = this.model;

            Class.AppendLine();
            //BuildCommentHeader(routine, @return, @params, true);

            Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection, {string.Join(", ", this.pk.Select(p => $"{p.Type} {p.Name}").ToArray())})");

            Class.AppendLine($"{I2}{{");
            
            Class.AppendLine($"{I3}var query = @\"");
            Class.AppendLine($"select");
            Class.AppendLine(string.Join($",{NL}", this.columns.Select(c => $"{I1}[{c.Name}]")));
            Class.AppendLine($"from");
            Class.AppendLine($"{I1}{this.table}");
            Class.Append($"where{NL}{I1}");
            Class.AppendLine(string.Join($"{NL}{I1}and ", this.pk.Select(c => $"[{c.PgName}] = @{c.Name}")));
            Class.AppendLine($"\";");

            Class.AppendLine($"{I3}return connection");
            if (!settings.CrudNoPrepare)
            {
                Class.AppendLine($"{I4}.Prepared()");
            }
            Class.Append($"{I4}.Read<{this.model}>(query");

            if (pk.Count > 0)
            {
                Class.AppendLine(", ");
                Class.Append(string.Join($",{NL}", pk.Select(p => $"{I5}(\"{p.PgName}\", {p.Name}, {p.DbType})")));
            }
            Class.AppendLine($")");
            Class.AppendLine($"{I4}.Single();");

            Class.AppendLine($"{I2}}}");

            Methods.Add(new Method(name, @namespace, pk, new Return(this.Name, name, false, true), actualReturns, true));
        }

        private string BuildModel()
        {
            var name = Name.ToUpperCamelCase();
            var model = new StringBuilder();
            if (!settings.UseRecords)
            {
                model.AppendLine($"{I1}public class {name}");
                model.AppendLine($"{I1}{{");
                foreach (var item in this.columns)
                {
                    model.AppendLine($"{I2}public {GetParamType(item)} {item.Name.ToUpperCamelCase()} {{ get; set; }}");
                }
                model.AppendLine($"{I1}}}");
            }
            else
            {
                model.Append($"{I1}public record {name}(");
                model.Append(string.Join(", ", this.columns.Select(item => $"{GetParamType(item)} {item.Name.ToUpperCamelCase()}")));
                model.AppendLine($");");
            }

            UserDefinedModels.Add(name);
            Models.Add(name, model);
            return name;
        }

        private List<Param> GetParamsFromPk()
        {
            return this.columns
                .Where(c => c.IsPk)
                .Select(p => new Param(p.Name, p.Name.ToCamelCase(), p.DataType, GetParamType(p), GetParamDbType(p)))
                .ToList();
        }
    }
}
