﻿using PgRoutiner.Builder.DiffBuilder;

namespace PgRoutiner.DumpTransformers;

public partial class TableDumpTransformer
{
    private class DiffEntries
    {
        public Dictionary<string, (string statement, EntryType type)> Statements { get; } = new();
        public Dictionary<string, string> Indexes { get; } = new();
        public Dictionary<string, string> Comments { get; } = new();
        public Dictionary<string, List<string>> Grants { get; } = new();
        public Dictionary<string, string> Triggers { get; } = new();
    }

    private class DiffStatements
    {
        public DiffEntries Source { get; } = new();
        public DiffEntries Target { get; } = new();
    }

    public StringBuilder ToDiff(TableDumpTransformer source, Statements statements)
    {
        StringBuilder alters = new();

        BuildAlterColumnsDiff(source, alters);
        BuildCreateColumnsDiff(source, alters);
        BuildDropColumnsDiff(source, alters);

        var diffStatements = BuildDiffStatements(source);

        BuildAlterIndexesDiff(diffStatements, statements);
        BuildAlterTriggersDiff(diffStatements, statements);
        BuildDropStatementsDiff(diffStatements, statements);
        BuildAlterStatementsDiff(diffStatements, statements);
        BuildCreateStatementsDiff(diffStatements, statements);
        BuildCommentsDiff(diffStatements, statements);
        BuildGrantsDiff(diffStatements, statements);

        return alters;
    }

    private void BuildGrantsDiff(DiffStatements diffStatements, Statements statements)
    {
        void Build()
        {
            foreach (var targetKey in diffStatements.Target.Grants.Keys)
            {
                statements.TableGrants.AppendLine($"REVOKE ALL ON {Item.Schema}.\"{Item.Name}\" FROM {targetKey};");
            }
            foreach (var (sourceKey, sourceValue) in diffStatements.Source.Grants)
            {
                statements.TableGrants.AppendLine($"REVOKE ALL ON {Item.Schema}.\"{Item.Name}\" FROM {sourceKey};");
                foreach (var value in sourceValue)
                {
                    statements.TableGrants.AppendLine(value);
                }
            }
        }
        if (diffStatements.Source.Grants.Keys.Count != diffStatements.Target.Grants.Keys.Count)
        {
            Build();
        }
        else
        {
            foreach (var (sourceKey, sourceValue) in diffStatements.Source.Grants)
            {
                if (!diffStatements.Target.Grants.TryGetValue(sourceKey, out var targetValue))
                {
                    Build();
                    return;
                }
                if (sourceValue.Count != targetValue.Count)
                {
                    Build();
                    return;
                }
                if (!Equals(string.Join("", sourceValue.OrderBy(s => s)), string.Join("", sourceValue.OrderBy(s => s))))
                {
                    Build();
                    return;
                }
            }
            if (diffStatements.Target.Grants.Keys.Where(k => !diffStatements.Source.Grants.Keys.Contains(k)).Any())
            {
                Build();
                return;
            }
        }
    }

    private static void BuildAlterIndexesDiff(DiffStatements diffStatements, Statements statements)
    {
        foreach (var (sourceKey, sourceValue) in diffStatements.Source.Indexes)
        {
            foreach (var (targetKey, targetValue) in diffStatements.Target.Indexes)
            {
                if (Equals(sourceValue, targetValue) && !Equals(sourceKey, targetKey))
                {
                    statements.AlterIndexes.AppendLine($"ALTER INDEX \"{targetKey.Trim('"')}\" RENAME TO \"{sourceKey.Trim('"')}\";");
                    diffStatements.Source.Statements.Remove(sourceKey);
                    diffStatements.Target.Statements.Remove(targetKey);
                }
            }
        }
    }

    private void BuildAlterTriggersDiff(DiffStatements diffStatements, Statements statements)
    {
        foreach (var (sourceKey, sourceValue) in diffStatements.Source.Triggers)
        {
            foreach (var (targetKey, targetValue) in diffStatements.Target.Triggers)
            {
                if (Equals(sourceValue, targetValue) && !Equals(sourceKey, targetKey))
                {
                    statements.CreateTriggers.AppendLine($"ALTER TRIGGER \"{targetKey.Trim('"')}\" ON {Item.Schema}.\"{Item.Name}\" RENAME TO \"{sourceKey.Trim('"')}\";");
                    diffStatements.Source.Statements.Remove(sourceKey);
                    diffStatements.Target.Statements.Remove(targetKey);
                }
            }
        }
    }

    private static void BuildCommentsDiff(DiffStatements diffStatements, Statements statements)
    {
        foreach (var (targetKey, targetValue) in diffStatements.Target.Comments)
        {
            if (!diffStatements.Source.Comments.TryGetValue(targetKey, out var sourceValue))
            {
                statements.TableComments.AppendLine($"{targetValue.Split(" IS ", 2).First()} IS NULL;");
            }
            else
            {
                if (!Equals(sourceValue, targetValue))
                {
                    statements.TableComments.AppendLine(sourceValue);
                }
            }
        }
        foreach (var (sourceKey, sourceValue) in diffStatements.Source.Comments.Where(c => !diffStatements.Target.Comments.Keys.Contains(c.Key)))
        {
            statements.TableComments.AppendLine(sourceValue);
        }
    }

    private void BuildDropStatementsDiff(DiffStatements diffStatements, Statements statements)
    {
        foreach (var (targetKey, targetValue) in diffStatements.Target.Statements.Where(c => !diffStatements.Source.Statements.Keys.Contains(c.Key)))
        {
            if (targetValue.type == EntryType.Contraint || targetValue.type == EntryType.Index)
            {
                string element = targetValue.type == EntryType.Contraint ? "CONSTRAINT" : "INDEX";
                statements.Drop.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" DROP {element} \"{targetKey.Trim('"')}\";");
            }
            else if (targetValue.type == EntryType.Trigger)
            {
                statements.DropTriggers.AppendLine($"DROP TRIGGER \"{targetKey.Trim('"')}\" ON {Item.Schema}.\"{Item.Name}\";");
            }
        }
    }

    private void BuildAlterStatementsDiff(DiffStatements diffStatements, Statements statements)
    {
        foreach (var (sourceKey, sourceValue) in diffStatements.Source.Statements)
        {
            if (!diffStatements.Target.Statements.TryGetValue(sourceKey, out var targetValue))
            {
                continue;
            }
            if (Equals(sourceValue, targetValue))
            {
                continue;
            }
            if (sourceValue.type == EntryType.Contraint || sourceValue.type == EntryType.Index)
            {
                string element = sourceValue.type == EntryType.Contraint ? "CONSTRAINT" : "INDEX";
                statements.Drop.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" DROP {element} \"{sourceKey.Trim('"')}\";");
            }
            if (sourceValue.statement.IsUniqueStatement())
            {
                statements.Unique.AppendLine(sourceValue.statement);
            }
            else
            {
                statements.Create.AppendLine(sourceValue.statement);
            }
        }
    }

    private static void BuildCreateStatementsDiff(DiffStatements diffStatements, Statements statements)
    {
        foreach (var sourceKey in diffStatements.Source.Statements.Keys.Where(c => !diffStatements.Target.Statements.Keys.Contains(c)))
        {
            var (statement, type) = diffStatements.Source.Statements[sourceKey];
            if (statement.IsUniqueStatement())
            {
                statements.Unique.AppendLine(statement);
            }
            else
            {
                if (type == EntryType.Trigger)
                {
                    statements.CreateTriggers.AppendLine(statement);
                }
                else
                {
                    statements.Create.AppendLine(statement);
                }
            }
        }
    }

    private DiffStatements BuildDiffStatements(TableDumpTransformer source)
    {
        var result = new DiffStatements();

        static void AddEntry(string entry, DiffEntries diffEntries)
        {
            var name = entry.FirstWordAfter(" CONSTRAINT");
            var type = EntryType.Contraint;
            if (name == null)
            {
                name = entry.FirstWordAfter(" INDEX");
                type = EntryType.Index;
                if (name != null)
                {
                    diffEntries.Indexes.Add(name, entry.Split(" ON ").Last());
                }
            }
            if (name == null)
            {
                name = entry.FirstWordAfter(" TRIGGER");
                type = EntryType.Trigger;
                if (name != null)
                {
                    diffEntries.Triggers.Add(name, entry.FirstWordAfter(name, null));
                }
            }
            if (name == null)
            {
                name = entry.FirstWordAfter("SEQUENCE NAME");
                type = EntryType.Sequence;
            }
            if (name != null)
            {
                diffEntries.Statements.Add(name, (entry, type));
            }
            else
            {
                name = entry.FirstWordAfter("COMMENT ON TABLE");
                if (name == null)
                {
                    name = entry.FirstWordAfter("COMMENT ON COLUMN");
                }
                if (name != null)
                {
                    diffEntries.Comments.Add(name, entry);
                }
                else if (entry.StartsWith("GRANT "))
                {
                    name = entry.FirstWordAfter("TO");
                    if (name != null)
                    {
                        name = name.TrimEnd(';');
                        if (diffEntries.Grants.TryGetValue(name, out var grants))
                        {
                            grants.Add(entry);
                            diffEntries.Grants[name] = grants;
                        }
                        diffEntries.Grants.Add(name, new List<string>() { entry });
                    }
                }
            }
        }

        foreach (var entry in source.Names.Where(n => n.Value.entryType == EntryType.Contraint))
        {
            result.Source.Statements.Add(entry.Key,
                ($"ALTER TABLE ONLY {source.Item.Schema}.\"{source.Item.Name}\" ADD CONSTRAINT {entry.Key.Trim('"')} {entry.Value.content};", EntryType.Contraint));
        }
        foreach (var entry in source.Append)
        {
            AddEntry(entry, result.Source);
        }
        foreach (var entry in Names.Where(n => n.Value.entryType == EntryType.Contraint))
        {
            result.Target.Statements.Add(entry.Key,
                ($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" ADD CONSTRAINT {entry.Key.Trim('"')} {entry.Value.content};", EntryType.Contraint));
        }
        foreach (var entry in Append)
        {
            AddEntry(entry, result.Target);
        }
        return result;
    }

    private void BuildDropColumnsDiff(TableDumpTransformer source, StringBuilder sb)
    {
        var sourceFields = source.Names.Where(n => n.Value.entryType == EntryType.Field).Select(n => n.Key).ToList();
        foreach (var field in Names.Where(n => n.Value.entryType == EntryType.Field && !sourceFields.Contains(n.Key)).Select(n => n.Key))
        {
            sb.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" DROP COLUMN \"{field}\";");
        }
    }

    private void BuildCreateColumnsDiff(TableDumpTransformer source, StringBuilder sb)
    {
        var targetFields = Names.Where(n => n.Value.entryType == EntryType.Field).Select(n => n.Key).ToList();
        foreach (var field in source.Names.Where(n => n.Value.entryType == EntryType.Field && !targetFields.Contains(n.Key)).Select(n => n.Key))
        {
            var (position, content, entryType) = source.Names[field];
            sb.AppendLine($"ALTER TABLE ONLY {source.Item.Schema}.\"{source.Item.Name}\" ADD COLUMN \"{field}\" {content.TrimEnd(',')};");
        }
    }

    private void BuildAlterColumnsDiff(TableDumpTransformer source, StringBuilder sb)
    {
        foreach (var field in source.Names.Where(n => n.Value.entryType == EntryType.Field).Select(n => n.Key))
        {
            var (position, content, entryType) = source.Names[field];
            if (!Names.TryGetValue(field, out var targetValue))
            {
                continue;
            }
            if (string.Equals(content, targetValue.content))
            {
                continue;
            }
            var (sourceType, sourceDefault, sourceNotNull) = ParseFieldContent(content.TrimEnd(','));
            var (targetType, targetDefault, targetNotNull) = ParseFieldContent(targetValue.content.TrimEnd(','));
            if (!string.Equals(sourceType, targetType))
            {
                sb.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" ALTER COLUMN \"{field}\" TYPE {sourceType};");
            }
            if (!string.Equals(sourceDefault, targetDefault))
            {
                if (string.IsNullOrEmpty(sourceDefault))
                {
                    sb.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" ALTER COLUMN \"{field}\" DROP DEFAULT;");
                }
                else
                {
                    sb.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" ALTER COLUMN \"{field}\" SET DEFAULT {sourceDefault};");
                }

            }
            if (sourceNotNull != targetNotNull)
            {
                if (!sourceNotNull)
                {
                    sb.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" ALTER COLUMN \"{field}\" DROP NOT NULL;");
                }
                else
                {
                    sb.AppendLine($"ALTER TABLE ONLY {Item.Schema}.\"{Item.Name}\" ALTER COLUMN \"{field}\" SET NOT NULL;");
                }
            }
        }
    }
}
