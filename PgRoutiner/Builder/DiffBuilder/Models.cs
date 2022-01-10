namespace PgRoutiner.Builder.DiffBuilder;

public record Table(string Schema, string Name);
public record Domain(string Schema, string Name);
public record Type(string Schema, string Name);
public record Routine(string Schema, string Name, string Params);
public record Seq(string Schema, string Name);

public class Statements
{
    public StringBuilder Drop { get; } = new();
    public StringBuilder Unique { get; } = new();
    public StringBuilder Create { get; } = new();
    public StringBuilder DropTriggers { get; } = new();
    public StringBuilder CreateTriggers { get; } = new();
    public StringBuilder AlterIndexes { get; } = new();
    public StringBuilder TableComments { get; } = new();
    public StringBuilder TableGrants { get; } = new();
}

