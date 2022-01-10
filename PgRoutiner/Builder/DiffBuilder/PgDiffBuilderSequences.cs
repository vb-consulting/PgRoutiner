using PgRoutiner.DumpTransformers;

namespace PgRoutiner.Builder.DiffBuilder;

public partial class PgDiffBuilder
{
    private void BuildDropSeqsNotInSource(StringBuilder sb)
    {
        var header = false;
        foreach (var domainKey in targetSeqs.Keys.Where(k => !sourceSeqs.Keys.Contains(k)))
        {
            if (!header)
            {
                AddComment(sb, "#region DROP NON EXISTING SEQUENCES");
                header = true;
            }
            sb.AppendLine($"DROP SEQUENCE {domainKey.Schema}.\"{domainKey.Name}\";");
        }
        if (header)
        {
            AddComment(sb, "#endregion DROP NON EXISTING SEQUENCES");
        }
    }

    private void BuildCreateSeqsNotInTarget(StringBuilder sb)
    {
        var header = false;
        foreach (var domainKey in sourceSeqs.Keys.Where(k => !targetSeqs.Keys.Contains(k)))
        {
            if (!header)
            {
                AddComment(sb, "#region CREATE NON EXISTING SEQUENCES");
                header = true;
            }
            var item = sourceSeqs[domainKey];
            var content = new SequenceDumpTransformer(item, sourceBuilder.GetRawTableDumpLines(item, settings.DiffPrivileges))
                    .BuildLines(ignorePrepend: true)
                    .ToString();
            sb.AppendLine(content.ToString());
        }
        if (header)
        {
            AddComment(sb, "#endregion CREATE NON EXISTING SEQUENCES");
        }
    }
}
