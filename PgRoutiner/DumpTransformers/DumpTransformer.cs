namespace PgRoutiner.DumpTransformers;

public abstract class DumpTransformer
{
    protected readonly List<string> lines;

    public List<string> Prepend { get; } = new();
    public List<string> Create { get; } = new();
    public List<string> Append { get; } = new();

    public DumpTransformer(List<string> lines)
    {
        this.lines = lines;
    }

    public bool Equals(DumpTransformer other)
    {
        if (Create.Count != other.Create.Count)
        {
            return false;
        }
        if (Append.Count != other.Append.Count)
        {
            return false;
        }
        foreach (var (line, idx) in Create.Select((l, idx) => (l, idx)))
        {
            if (!string.Equals(line.Trim(), other.Create[idx].Trim()))
            {
                return false;
            }
        }
        foreach (var (line, idx) in Append.Select((l, idx) => (l, idx)))
        {
            if (!string.Equals(line.Trim(), other.Append[idx].Trim()))
            {
                return false;
            }
        }
        return true;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        if (Prepend.Count > 0)
        {
            sb.Append(string.Join(Environment.NewLine, Prepend));
            sb.AppendLine();
            sb.AppendLine();
        }
        sb.Append(string.Join(Environment.NewLine, Create));
        sb.AppendLine();
        if (Append.Count > 0)
        {
            sb.AppendLine();
            sb.Append(string.Join(Environment.NewLine, Append));
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
