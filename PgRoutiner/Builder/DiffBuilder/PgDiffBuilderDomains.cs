using PgRoutiner.DumpTransformers;

namespace PgRoutiner.Builder.DiffBuilder;

public partial class PgDiffBuilder
{
    private void BuildAlterDomains(StringBuilder sb)
    {
        var header = false;
        foreach (var sourceKey in targetDomains.Keys.Where(k => sourceDomains.Keys.Contains(k)))
        {
            var sourceItem = sourceDomains[sourceKey];
            var targetItem = targetDomains[sourceKey];
            var sourceTransformer = new DomainDumpTransformer(sourceItem, SourceLines).BuildLines(ignorePrepend: true);
            var targetTransformer = new DomainDumpTransformer(targetItem, TargetLines).BuildLines(ignorePrepend: true);
            var alters = targetTransformer.ToDiff(sourceTransformer);
            if (alters.Length > 0)
            {
                if (!header)
                {
                    AddComment(sb, "#region ALTER DOMAINS");
                    header = true;
                }
                sb.Append(alters);
            }
        }
        if (header)
        {
            AddComment(sb, "#endregion ALTER DOMAINS");
        }
    }

    private void BuildDropDomainsNotInSource(StringBuilder sb)
    {
        var header = false;
        foreach (var domainKey in targetDomains.Keys.Where(k => !sourceDomains.Keys.Contains(k)))
        {
            if (!header)
            {
                AddComment(sb, "#region DROP NON EXISTING DOMAINS");
                header = true;
            }
            sb.AppendLine($"DROP DOMAIN {domainKey.Schema}.\"{domainKey.Name}\";");
        }
        if (header)
        {
            AddComment(sb, "#endregion DROP NON EXISTING DOMAINS");
        }
    }

    private void BuildCreateDomainsNotInTarget(StringBuilder sb)
    {
        var header = false;
        foreach (var domainKey in sourceDomains.Keys.Where(k => !targetDomains.Keys.Contains(k)))
        {
            if (!header)
            {
                AddComment(sb, "#region CREATE NON EXISTING DOMAINS");
                header = true;
            }
            var item = sourceDomains[domainKey];
            var content = new DomainDumpTransformer(item, SourceLines).BuildLines(ignorePrepend: true);
            sb.AppendLine(content.ToString());
        }
        if (header)
        {
            AddComment(sb, "#endregion CREATE NON EXISTING DOMAINS");
        }
    }
}
