using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DumpTransformers;

public class SequenceDumpTransformer : DumpTransformer
{
    public PgItem Item { get; }

    public SequenceDumpTransformer(PgItem item, List<string> lines) : base(lines)
    {
        this.Item = item;
    }

    public SequenceDumpTransformer BuildLines(
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

        var startSequence = $"CREATE SEQUENCE ";

        string statement = "";
        const string endSequence = ";";

        foreach (var l in lines)
        {
            var line = l;
            if (!isCreate && (line.StartsWith("--") || line.StartsWith("SET ") || line.StartsWith("SELECT ")))
            {
                continue;
            }

            var createStart = line.StartsWith(startSequence);
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
