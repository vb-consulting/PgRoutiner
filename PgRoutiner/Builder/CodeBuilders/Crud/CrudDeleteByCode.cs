using PgRoutiner.Builder.CodeBuilders.Models;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.CodeBuilders.Crud;

public class CrudDeleteByCode : CrudCodeBase
{
    public CrudDeleteByCode(
        Settings settings,
        (string schema, string name) item,
        string @namespace,
        IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "DeleteBy")
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
        Class.AppendLine($"{I3}DELETE FROM {this.Table}");
        Class.Append($"{I3}WHERE{NL}{I4}");
        Class.Append(string.Join($"{NL}{I1}AND ", this.PkParams.Select(c => $"\"\"{c.PgName}\"\" = @{c.Name}")));
        Class.AppendLine($"\";");
    }

    protected override void BuildStatementBodySyncMethod()
    {
        var name = $"Delete{this.Name}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()))}";
        Class.AppendLine();
        BuildSyncMethodCommentHeader();
        Class.AppendLine($"{I2}public static void {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}"))})");
        Class.AppendLine($"{I2}{{");
        Class.AppendLine($"{I3}connection");
        if (!settings.CrudNoPrepare)
        {
            Class.AppendLine($"{I4}.Prepared()");
        }
        this.BuildPkParams(I4);
        Class.Append($"{I4}.Execute(Sql");
        Class.AppendLine($");");
        Class.AppendLine($"{I2}}}");
        AddMethod(name, true);
    }

    protected override void BuildStatementBodyAsyncMethod()
    {
        var name = $"Delete{this.Name}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()))}Async";
        Class.AppendLine();
        BuildAsyncMethodCommentHeader();
        Class.AppendLine($"{I2}public static async ValueTask {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}"))})");
        Class.AppendLine($"{I2}{{");
        Class.AppendLine($"{I3}await connection");
        if (!settings.CrudNoPrepare)
        {
            Class.AppendLine($"{I4}.Prepared()");
        }
        this.BuildPkParams(I4);
        Class.Append($"{I4}.ExecuteAsync(Sql");
        Class.AppendLine($");");
        Class.AppendLine($"{I2}}}");
        AddMethod(name, false);
    }

    protected override void BuildExpressionBodySyncMethod()
    {
        var name = $"Delete{this.Name}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()).ToArray())}";
        Class.AppendLine();
        BuildSyncMethodCommentHeader();
        Class.AppendLine($"{I2}public static void {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}").ToArray())}) => connection");
        if (!settings.CrudNoPrepare)
        {
            Class.AppendLine($"{I3}.Prepared()");
        }
        this.BuildPkParams(I3);
        Class.Append($"{I3}.Execute(Sql");
        Class.AppendLine($");");
        AddMethod(name, true);
    }

    protected override void BuildExpressionBodyAsyncMethod()
    {
        var name = $"Delete{this.Name}By{string.Join("And", PkParams.Select(p => p.Name.ToUpperCamelCase()).ToArray())}Async";
        Class.AppendLine();
        BuildAsyncMethodCommentHeader();
        Class.AppendLine($"{I2}public static async ValueTask {name}(this NpgsqlConnection connection, {string.Join(", ", this.PkParams.Select(p => $"{p.Type} {p.Name}"))}) => await connection");
        if (!settings.CrudNoPrepare)
        {
            Class.AppendLine($"{I3}.Prepared()");
        }
        this.BuildPkParams(I3);
        Class.Append($"{I3}.ExecuteAsync(Sql");
        Class.AppendLine($");");
        AddMethod(name, false);
    }

    private void BuildSyncMethodCommentHeader()
    {
        Class.AppendLine($"{I2}/// <summary>");
        Class.AppendLine($"{I2}///  Delete record of table {this.Table} by primary keys.");
        Class.AppendLine($"{I2}/// </summary>");
        foreach (var p in this.PkParams)
        {
            Class.AppendLine($"{I2}/// <param name=\"{p.Name}\">Select table {this.Table} where field {p.PgName} {p.PgType} is this value.</param>");
        }
    }

    private void BuildAsyncMethodCommentHeader()
    {
        Class.AppendLine($"{I2}/// <summary>");
        Class.AppendLine($"{I2}/// Asynchronously delete record of table {this.Table} by primary keys.");
        Class.AppendLine($"{I2}/// </summary>");
        foreach (var p in this.PkParams)
        {
            Class.AppendLine($"{I2}/// <param name=\"{p.Name}\">Select table {this.Table} where field {p.PgName} {p.PgType} is this value.</param>");
        }
    }

    private void AddMethod(string name, bool sync)
    {
        Methods.Add(new Method
        {
            Name = name,
            Namespace = Namespace,
            Params = this.PkParams,
            Returns = new Return { PgName = "void", Name = "void", IsVoid = true, IsEnumerable = true },
            ActualReturns = "void",
            Sync = sync
        });
    }
}
