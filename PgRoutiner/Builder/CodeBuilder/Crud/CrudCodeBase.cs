using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public abstract class CrudCodeBase : Code
    {
        protected readonly string Namespace;
        protected readonly IEnumerable<PgColumnGroup> Columns;
        protected readonly List<Param> PkParams = new();
        protected readonly List<Param> ColumnParams = new();
        protected readonly string Model;
        protected readonly string Table;

        public CrudCodeBase(
            Settings settings,
            (string schema, string name) item,
            string @namespace,
            IEnumerable<PgColumnGroup> columns,
            string suffix) : base(settings, item.name)
        {
            this.Table = item.schema == "public" ? $"[{item.name}]" : $"{ item.schema}.[{item.name}]";
            this.Namespace = @namespace;
            this.Columns = columns;
            foreach(var column in this.Columns)
            {
                var p = new Param
                {
                    PgName = column.Name,
                    PgType = column.DataType,
                    Type = GetParamType(column),
                    DbType = GetParamDbType(column)
                };
                if (column.IsPk)
                {
                    PkParams.Add(p);
                }
                ColumnParams.Add(p);
            }
            this.Model = BuildModel();
            BeginClass(suffix);
            AddName();
            AddSql();
            if (!settings.SkipSyncMethods)
            {
                if (!settings.UseExpressionBody)
                {
                    BuildStatementBodySyncMethod();
                }
                else
                {
                    BuildExpressionBodySyncMethod();
                }
            }
            if (!settings.SkipAsyncMethods)
            {
                if (!settings.UseExpressionBody)
                {
                    BuildStatementBodyAsyncMethod();
                }
                else
                {
                    BuildExpressionBodyAsyncMethod();
                }
            }
            EndClass();
        }

        protected abstract void AddSql();

        protected abstract void BuildStatementBodySyncMethod();

        protected abstract void BuildStatementBodyAsyncMethod();

        protected abstract void BuildExpressionBodySyncMethod();

        protected abstract void BuildExpressionBodyAsyncMethod();

        protected abstract void BuildSyncMethodCommentHeader();

        protected abstract void BuildAsyncMethodCommentHeader();

        private void BeginClass(string suffix)
        {
            Class.AppendLine($"{I1}public static class {Name.ToUpperCamelCase()}{suffix}");
            Class.AppendLine($"{I1}{{");
        }

        private void EndClass()
        {
            Class.AppendLine($"{I1}}}");
        }

        private void AddName()
        {
            Class.AppendLine($"{I2}public const string Name = \"{this.Table}\";");
        }

        private string BuildModel()
        {
            var name = Name.ToUpperCamelCase();
            var model = new StringBuilder();
            if (!settings.UseRecords)
            {
                model.AppendLine($"{I1}public class {name}");
                model.AppendLine($"{I1}{{");
                foreach (var item in this.Columns)
                {
                    model.AppendLine($"{I2}public {GetParamType(item)} {item.Name.ToUpperCamelCase()} {{ get; set; }}");
                }
                model.AppendLine($"{I1}}}");
            }
            else
            {
                model.Append($"{I1}public record {name}(");
                model.Append(string.Join(", ", this.Columns.Select(item => $"{GetParamType(item)} {item.Name.ToUpperCamelCase()}")));
                model.AppendLine($");");
            }

            UserDefinedModels.Add(name);
            Models.Add(name, model);
            return name;
        }
    }
}
