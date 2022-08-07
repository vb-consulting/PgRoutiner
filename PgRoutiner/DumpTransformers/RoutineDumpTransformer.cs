using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DumpTransformers;

public class RoutineDumpTransformer : DumpTransformer
{
    public PgItem Item { get; }

    public RoutineDumpTransformer(PgItem item, List<string> lines) : base(lines)
    {
        this.Item = item;
    }

    public RoutineDumpTransformer BuildLines(
        string paramsString = null,
        bool dbObjectsCreateOrReplace = false,
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

        var name1 = $"{Item.Schema}.{Item.Name}{paramsString ?? "("}";
        var name2 = $"{Item.Schema}.\"{Item.Name}\"{paramsString ?? "("}";
        var name3 = $"\"{Item.Schema}\".\"{Item.Name}\"{paramsString ?? "("}";
        var name4 = $"\"{Item.Schema}\".{Item.Name}{paramsString ?? "("}";

        var startSequence1 = $"CREATE {Item.TypeName} {name1}";
        var startSequence2 = $"CREATE {Item.TypeName} {name2}";
        var startSequence3 = $"CREATE {Item.TypeName} {name3}";
        var startSequence4 = $"CREATE {Item.TypeName} {name4}";

        string statement = "";
        string endSequence = null;

        foreach (var l in lines)
        {
            var line = l;
            if (!isCreate && (line.StartsWith("--") || line.StartsWith("SET ") || line.StartsWith("SELECT ")))
            {
                continue;
            }
            if (!isCreate && string.IsNullOrEmpty(statement) && !line.Contains(name1) && !line.Contains(name2) && !line.Contains(name3) && !line.Contains(name4))
            {
                continue;
            }

            var createStart = line.StartsWith(startSequence1) || line.StartsWith(startSequence2) || line.StartsWith(startSequence3) || line.StartsWith(startSequence4);
            var createEnd = endSequence != null && line.Contains($"{endSequence};");
            if (createStart)
            {
                if (dbObjectsCreateOrReplace)
                {
                    line = line.Replace("CREATE", "CREATE OR REPLACE");
                }
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
                if (endSequence == null)
                {
                    endSequence = line.GetSequence();
                }
                if (endSequence == null && line.Contains("RETURN "))
                {
                    endSequence = "";
                    createEnd = line.Contains($";");
                }
                if (endSequence == null && line.Contains("BEGIN"))
                {
                    endSequence = "END";
                }
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
