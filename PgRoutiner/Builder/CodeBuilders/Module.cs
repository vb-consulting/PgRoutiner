namespace PgRoutiner.Builder.CodeBuilders;

public class Module : Code
{
    public HashSet<string> Usings = new()
    {
        //"System",
        //"System.Linq",
        //"System.Collections.Generic"
    };

    protected List<object> items = new();
    private readonly bool skipUsing;
    private readonly bool skipPragma;

    public string Namespace { get; set; }

    public Module(Current settings, bool skipUsing = false, bool skipPragma = false) : base(settings, null)
    {
        Namespace = settings.Namespace?.Trim('.');
        this.skipUsing = skipUsing;
        this.skipPragma = skipPragma;
    }

    public void AddUsing(params string[] usings)
    {
        foreach (var u in usings)
        {
            this.Usings.Add(u);
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
        if (settings.SourceHeaderLines != null && settings.SourceHeaderLines.Count > 0)
        {
            foreach(var line in settings.SourceHeaderLines)
            {
                if (!skipPragma)
                {
                    builder.AppendLine(string.Format(line, DateTime.Now));
                }
                else
                {
                    var value = string.Format(line, DateTime.Now);
                    if (!value.StartsWith("#pragma"))
                    {
                        builder.AppendLine(value);
                    }
                }
                
            }
        }
        if (!skipUsing)
        {
            foreach (var ns in Usings)
            {
                builder.AppendLine($"using {ns};");
            }
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
