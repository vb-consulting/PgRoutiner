using PgRoutiner.Builder.CodeBuilders.Models;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.CodeBuilders.Crud;

public abstract class CrudCodeBase : Code
{
    private readonly (string schema, string name) item;
    protected readonly string Namespace;
    protected readonly IEnumerable<PgColumnGroup> Columns;
    private readonly string suffix;
    protected readonly List<Param> PkParams = new();
    protected readonly List<Param> ColumnParams = new();
    protected readonly string Model;
    protected readonly string Table;
    protected List<Param> Params;

    public CrudCodeBase(
        Settings settings,
        (string schema, string name) item,
        string @namespace,
        IEnumerable<PgColumnGroup> columns,
        string suffix) : base(settings, item.name)
    {
        this.Name = this.Name.ToUpperCamelCase();
        if (settings.CustomModels.ContainsKey(this.Name))
        {
            this.Name = settings.CustomModels[this.Name];
        }
        this.Table = item.schema == "public" ? $"\"\"{item.name}\"\"" : $"{item.schema}.\"\"{item.name}\"\"";
        this.item = item;
        this.Namespace = @namespace;
        this.Columns = columns;
        this.suffix = suffix;
        this.Model = BuildModel();
    }

    protected void Build()
    {
        foreach (var column in this.Columns)
        {
            var p = new Param(settings)
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
        BeginClass(suffix);
        AddName();
        Class.AppendLine();
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

    protected string GetReturnMethod(string name)
    {
        if (settings.CrudReturnMethods.TryGetValue(name, out var result))
        {
            return string.IsNullOrEmpty(result) ? null : result;
        }
        if (settings.CrudReturnMethods.TryGetValue(item.name, out result))
        {
            return string.IsNullOrEmpty(result) ? null : result;
        }
        if (settings.CrudReturnMethods.TryGetValue($"\"{item.name}\"", out result))
        {
            return string.IsNullOrEmpty(result) ? null : result;
        }
        if (settings.CrudReturnMethods.TryGetValue($"{item.schema}.{item.name}", out result))
        {
            return string.IsNullOrEmpty(result) ? null : result;
        }
        if (settings.CrudReturnMethods.TryGetValue($"{item.schema}.\"{item.name}\"", out result))
        {
            return string.IsNullOrEmpty(result) ? null : result;
        }
        return settings.ReturnMethod;
    }

    protected void BuildParams(string ident)
    {
        Class.AppendLine(", new");
        Class.AppendLine($"{ident}{{");
        Class.Append(string.Join($",{NL}", this.ColumnParams.Select(p => $"{ident}{I2}@{p.Name} = (model.{p.ClassName}, {p.DbType})")));
        Class.AppendLine();
        Class.Append($"{ident}}}");
    }

    protected void BuildPkParams(string ident)
    {
        if (PkParams.Count > 0)
        {
            Class.AppendLine(", new");
            Class.AppendLine($"{ident}{{");
            Class.Append(string.Join($",{NL}", this.PkParams.Select(p => $"{ident}{I2}@{p.Name} = ({p.Name}, {p.DbType})")));
            Class.AppendLine();
            Class.Append($"{ident}}}");
        }
    }

    protected abstract void AddSql();

    protected abstract void BuildStatementBodySyncMethod();

    protected abstract void BuildStatementBodyAsyncMethod();

    protected abstract void BuildExpressionBodySyncMethod();

    protected abstract void BuildExpressionBodyAsyncMethod();

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
        var name = item.schema == "public" ? item.name : $"{item.schema}.{item.name}";
        Class.AppendLine($"{I2}public const string Name = \"{name}\";");
    }

    private string BuildModel()
    {
        var name = Name.ToUpperCamelCase();
        if (settings.Mapping.TryGetValue(name, out var custom))
        {
            return custom;
        }
        if (settings.CustomModels.ContainsKey(name))
        {
            name = settings.CustomModels[name];
        }
        else if (settings.CustomModels.ContainsKey(Name))
        {
            name = settings.CustomModels[Name];
        }
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
