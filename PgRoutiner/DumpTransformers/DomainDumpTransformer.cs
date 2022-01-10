using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DumpTransformers;

public partial class DomainDumpTransformer : DumpTransformer
{
    public PgItem Item { get; }

    public bool IsNull { get; set; }
    public string Default { get; set; }
    public Dictionary<string, string> Constraints { get; } = new();

    public DomainDumpTransformer(PgItem item, List<string> lines) : base(lines)
    {
        this.Item = item;
    }

    public DomainDumpTransformer BuildLines(
        bool ignorePrepend = false,
        Action<string> lineCallback = null)
    {
        Prepend.Clear();
        Create.Clear();
        Append.Clear();

        if (lineCallback == null)
        {
            lineCallback = s => { };
        }

        bool isPrepend = true;
        bool isCreate = false;
        bool isAppend = true;

        var name1 = $"{Item.Schema}.{Item.Name}";
        var name2 = $"{Item.Schema}.\"{Item.Name}\"";
        var name3 = $"\"{Item.Schema}\".\"{Item.Name}\"";
        var name4 = $"\"{Item.Schema}\".{Item.Name}";

        var startSequence1 = $"CREATE DOMAIN {name1} AS ";
        var startSequence2 = $"CREATE DOMAIN {name2} AS ";
        var startSequence3 = $"CREATE DOMAIN {name3} AS ";
        var startSequence4 = $"CREATE DOMAIN {name4} AS ";

        string statement = "";
        const string endSequence = ";";

        bool shouldContinue(string line)
        {
            return !isCreate && string.IsNullOrEmpty(statement) &&
                !line.Contains(string.Concat(name1, ";")) && !line.Contains(string.Concat(name2, ";")) && !line.Contains(string.Concat(name3, ";")) && !line.Contains(string.Concat(name4, ";")) &&
                !line.Contains(string.Concat(name1, " ")) && !line.Contains(string.Concat(name2, " ")) && !line.Contains(string.Concat(name3, " ")) && !line.Contains(string.Concat(name4, " "));
        }

        foreach (var l in lines)
        {
            var line = l;
            if (!isCreate && (line.StartsWith("--") || line.StartsWith("SET ") || line.StartsWith("SELECT ")))
            {
                continue;
            }
            if (shouldContinue(line))
            {
                continue;
            }

            var createStart = line.StartsWith(startSequence1) || line.StartsWith(startSequence2) || line.StartsWith(startSequence3) || line.StartsWith(startSequence4);
            var createEnd = line.EndsWith(endSequence);
            if (createStart)
            {
                isPrepend = false;
                isCreate = true;
                isAppend = false;
                if (Create.Count > 0)
                {
                    Create.Add("");
                }
                IsNull = !line.Contains("NOT NULL");
                Default = line.FirstWordAfter("DEFAULT", null);
                if (Default != null)
                {
                    Default = Default.TrimEnd(';');
                }
            }
            if (isCreate)
            {
                Create.Add(line);
                if (createEnd)
                {
                    isPrepend = false;
                    isCreate = false;
                    isAppend = true;
                }
                if (!createStart && !createEnd && !isAppend)
                {
                    lineCallback(line);
                }
                if (line.Contains("CONSTRAINT"))
                {
                    var constrinatName = line.FirstWordAfter("CONSTRAINT");
                    var constrinatValue = line.FirstWordAfter(constrinatName, null);
                    if (constrinatValue != null)
                    {
                        constrinatValue = constrinatValue.TrimEnd(';');
                    }
                    Constraints.Add(constrinatName, constrinatValue);
                }
            }
            else
            {
                statement = string.Concat(statement, statement == "" ? "" : Environment.NewLine, line);
                if (statement.EndsWith(";"))
                {
                    if (isPrepend && !ignorePrepend)
                    {
                        Prepend.Add(statement);
                    }
                    else if (isAppend)
                    {
                        Append.Add(statement);
                    }
                    statement = "";
                }
            }
        }
        return this;
    }
}
