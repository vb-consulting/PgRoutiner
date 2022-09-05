using PgRoutiner.Builder.CodeBuilders.Models;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.CodeBuilders.Crud;

public class CrudReadAllCode : CrudCodeBase
{
    public CrudReadAllCode(
        Current settings,
        (string schema, string name) item,
        string @namespace,
        IEnumerable<PgColumnGroup> columns) : base(settings, item, @namespace, columns, "ReadAll")
    {
        Build();
    }

    protected override void AddSql()
    {
        Class.AppendLine($"{I2}public const string Sql = @\"");
        Class.AppendLine($"{I3}SELECT");
        Class.AppendLine(string.Join($",{NL}", this.Columns.Select(c => $"{I4}\"\"{c.Name}\"\"")));
        Class.AppendLine($"{I3}FROM");
        Class.AppendLine($"{I4}{this.Table}\";");
    }

    protected override void BuildStatementBodySyncMethod()
    {
        var name = $"Read{this.Name}All";
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
        Class.AppendLine($"{I4}.Read<{this.Model}>(Sql);");
        Class.AppendLine($"{I2}}}");
        AddMethod(name, actualReturns, true);
    }

    protected override void BuildStatementBodyAsyncMethod()
    {
        var name = $"Read{this.Name}AllAsync";
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
        Class.AppendLine($"{I4}.ReadAsync<{this.Model}>(Sql);");
        Class.AppendLine($"{I2}}}");
        AddMethod(name, actualReturns, false);
    }

    protected override void BuildExpressionBodySyncMethod()
    {
        var name = $"Read{this.Name}All";
        var actualReturns = $"IEnumerable<{this.Model}>";
        Class.AppendLine();
        BuildSyncMethodCommentHeader();
        Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection) => connection");
        if (!settings.CrudNoPrepare)
        {
            Class.AppendLine($"{I3}.Prepared()");
        }
        Class.AppendLine($"{I3}.Read<{this.Model}>(Sql);");
        AddMethod(name, actualReturns, true);
    }

    protected override void BuildExpressionBodyAsyncMethod()
    {
        var name = $"Read{this.Name}AllAsync";
        var actualReturns = $"IAsyncEnumerable<{this.Model}>";
        Class.AppendLine();
        BuildAsyncMethodCommentHeader();
        Class.AppendLine($"{I2}public static {actualReturns} {name}(this NpgsqlConnection connection) => connection");
        if (!settings.CrudNoPrepare)
        {
            Class.AppendLine($"{I3}.Prepared()");
        }
        Class.AppendLine($"{I3}.ReadAsync<{this.Model}>(Sql);");
        AddMethod(name, actualReturns, false);
    }

    private void BuildSyncMethodCommentHeader()
    {
        Class.AppendLine($"{I2}/// <summary>");
        Class.AppendLine($"{I2}/// Select table {this.Table} and return enumerator of instances of a \"{Namespace}.{Model}\" class.");
        Class.AppendLine($"{I2}/// </summary>");
        Class.AppendLine($"{I2}/// <returns>Single instance of a \"{Namespace}.{Model}\" class that is mapped to resulting record of table {this.Table}</returns>");
    }

    private void BuildAsyncMethodCommentHeader()
    {
        Class.AppendLine($"{I2}/// <summary>");
        Class.AppendLine($"{I2}/// Asynchronously select table {this.Table} and return enumerator of instances of a \"{this.Model}\" class.");
        Class.AppendLine($"{I2}/// </summary>");
        Class.AppendLine($"{I2}/// <returns>IAsyncEnumerable of \"{Namespace}.{Model}\" instances.</returns>");
    }

    private void AddMethod(string name, string actualReturns, bool sync)
    {
        Methods.Add(new Method
        {
            Name = name,
            Namespace = Namespace,
            Params = new(),
            Returns = new Return { PgName = this.Name, Name = this.Model, IsVoid = false, IsEnumerable = true },
            ActualReturns = actualReturns,
            Sync = sync
        });
    }
}
