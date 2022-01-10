namespace PgRoutiner.Builder.CodeBuilders.Models;

public class Method
{
    public string ActualReturns { get; init; }
    public string Name { get; init; }
    public string Namespace { get; init; }
    public List<Param> Params { get; init; }
    public Return Returns { get; init; }
    public bool Sync { get; init; }
}
