namespace PgRoutiner.Builder.CodeBuilders;

public class Module : Code
{
    protected HashSet<string> usings = new()
    {
        "System",
        "System.Linq",
        "System.Collections.Generic"
    };
    protected List<object> items = new();

    public string Namespace { get; set; }

    public Module(Settings settings) : base(settings, null)
    {
        Namespace = settings.Namespace?.Trim('.');
    }

    public void AddUsing(params string[] usings)
    {
        foreach (var u in usings)
        {
            this.usings.Add(u);
        }
    }

    public void AddNamespace(params string[] namespaces)
    {
        foreach (var ns in namespaces)
        {
            if (!string.IsNullOrEmpty(ns))
            {
                Namespace = string.Concat(this.Namespace, ".", ns).Trim('.');
            }
        }
    }

    public void AddItems(params object[] items)
    {
        this.items.AddRange(items);
    }

    public void Flush()
    {
        this.items.Clear();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(settings.SourceHeader))
        {
            builder.AppendLine(string.Format(settings.SourceHeader, DateTime.Now));
        }
        foreach (var ns in usings)
        {
            builder.AppendLine($"using {ns};");
        }
        builder.AppendLine();
        if (!settings.UseFileScopedNamespaces)
        {
            builder.AppendLine($"namespace {Namespace}");
            builder.AppendLine("{");
            builder.Append(string.Join(NL, items.Where(i => i != null)));
            builder.AppendLine("}");
        }
        else
        {
            builder.AppendLine($"namespace {Namespace};");
            builder.AppendLine("");
            builder.Append(string.Join(NL, items.Where(i => i != null)));
        }
        return builder.ToString();
    }
}
