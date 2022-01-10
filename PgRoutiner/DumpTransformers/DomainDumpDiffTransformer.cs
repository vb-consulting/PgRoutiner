namespace PgRoutiner.DumpTransformers;

public partial class DomainDumpTransformer
{
    public StringBuilder ToDiff(DomainDumpTransformer source)
    {
        StringBuilder alters = new();
        BuildAlterNullsAndDefaults(source, alters);
        BuildAlterConstraints(source, alters);
        return alters;
    }

    private void BuildAlterNullsAndDefaults(DomainDumpTransformer source, StringBuilder sb)
    {
        if (source.IsNull != this.IsNull)
        {
            sb.AppendLine($"ALTER DOMAIN {this.Item.Schema}.\"{this.Item.Name}\" {(source.IsNull ? "DROP NOT NULL" : "SET NOT NULL")};");
        }
        if (source.Default != this.Default)
        {
            if (source.Default == null)
            {
                sb.AppendLine($"ALTER DOMAIN {this.Item.Schema}.\"{this.Item.Name}\" DROP DEFAULT;");
            }
            else
            {
                sb.AppendLine($"ALTER DOMAIN {this.Item.Schema}.\"{this.Item.Name}\" SET DEFAULT {source.Default};");
            }
        }
    }

    private void BuildAlterConstraints(DomainDumpTransformer source, StringBuilder sb)
    {
        var skipSourceKeys = new HashSet<string>();
        var skipTargetKeys = new HashSet<string>();
        foreach (var (sourceKey, sourceValue) in source.Constraints)
        {
            foreach (var (targetKey, targetValue) in this.Constraints)
            {
                if (Equals(sourceValue, targetValue) && !Equals(sourceKey, targetKey))
                {
                    sb.AppendLine($"ALTER DOMAIN {this.Item.Schema}.\"{this.Item.Name}\" RENAME CONSTRAINT \"{targetKey.Trim('"')}\" TO \"{sourceKey.Trim('"')}\";");
                    skipSourceKeys.Add(sourceKey);
                    skipTargetKeys.Add(targetKey); ;
                }
            }
        }

        foreach (var targetKey in Constraints.Keys.Where(k => !skipTargetKeys.Contains(k) && !source.Constraints.Keys.Contains(k)))
        {
            sb.AppendLine($"ALTER DOMAIN {this.Item.Schema}.\"{this.Item.Name}\" DROP CONSTRAINT \"{targetKey.Trim('"')}\" RESTRICT;");
        }

        foreach (var sourceKey in source.Constraints.Keys.Where(k => !skipSourceKeys.Contains(k) && !Constraints.Keys.Contains(k)))
        {
            var sourceValue = source.Constraints[sourceKey];
            sb.AppendLine($"ALTER DOMAIN {this.Item.Schema}.\"{this.Item.Name}\" ADD CONSTRAINT \"{sourceKey.Trim('"')}\" {sourceValue};");
        }
    }
}
