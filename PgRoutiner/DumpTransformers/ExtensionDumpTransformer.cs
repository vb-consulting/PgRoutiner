
namespace PgRoutiner.DumpTransformers;

public class ExtensionDumpTransformer : DumpTransformer
{
    public string Name { get; }

    public ExtensionDumpTransformer(string name, List<string> lines) : base(lines)
    {
        this.Name = name;
    }

    public ExtensionDumpTransformer BuildLines(
        bool ignorePrepend = false,
        Action<string> lineCallback = null)
    {
        Prepend.Clear();
        Create.Clear();
        Append.Clear();

        foreach (var l in lines)
        {
            if (l.StartsWith("--") || l.StartsWith("SET ") || l.StartsWith("SELECT "))
            {
                continue;
            }
            if (!l.Contains("EXTENSION"))
            {
                continue;
            }
            if (l.Contains(this.Name) || l.Contains($"\"{this.Name}\""))
            {
                if (l.StartsWith("CREATE"))
                {
                    Create.Add(l);
                }
                else
                {
                    Append.Add(l);
                }
            }
        }
        return this;
    }
}
