namespace PgRoutiner.Builder.CodeBuilders.Models;

public class Param
{
    private readonly Settings settings;

    public string DbType { get; init; }
    public string Name
    {
        get
        {
            var name = PgName.ToCamelCase();
            if (settings.MethodParameterNames.TryGetValue(name, out var result))
            {
                return result;
            }
            return name;
        }
    }

    public string ClassName
    {
        get
        {
            var name = PgName.ToUpperCamelCase();
            if (settings.MethodParameterNames.TryGetValue(name, out var result))
            {
                return result;
            }
            return name;
        }
    }

    public string PgName { get; init; }
    public string PgType { get; init; }
    public string Type { get; init; }
    public bool IsInstance { get; init; } = false;

    public Param(Settings settings)
    {
        this.settings = settings;
    }
}
