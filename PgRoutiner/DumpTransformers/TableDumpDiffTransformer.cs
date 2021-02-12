using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class TableDumpTransformer
    {
        private class DiffEntries
        {
            public Dictionary<string, (string statement, EntryType type)> Statements { get; } = new();
            public Dictionary<string, string> Indexes { get; } = new();
            public Dictionary<string, string> Comments { get; } = new();
        }

        private class DiffStatements
        {
            public DiffEntries Source { get; } = new();
            public DiffEntries Target { get; } = new();
        }

        public bool Equals(TableDumpTransformer to)
        {
            if (Create.Count != to.Create.Count)
            {
                return false;
            }
            if (Append.Count != to.Append.Count)
            {
                return false;
            }
            foreach (var (line, idx) in Create.Select((l, idx) => (l, idx)))
            {
                if (!string.Equals(line, to.Create[idx]))
                {
                    return false;
                }
            }
            foreach (var (line, idx) in Append.Select((l, idx) => (l, idx)))
            {
                if (!string.Equals(line, to.Append[idx]))
                {
                    return false;
                }
            }
            return true;
        }

        public StringBuilder ToDiff(TableDumpTransformer source, Statements statements)
        {
            StringBuilder alters = new();

            BuildAlterColumnsDiff(source, alters);
            BuildCreateColumnsDiff(source, alters);
            BuildDropColumnsDiff(source, alters);

            var diffStatements = BuildStatementsDicts(source);

            BuildAlterIndexesDiff(diffStatements, statements);
            BuildDropStatementsDiff(diffStatements, statements);
            BuildAlterStatementsDiff(diffStatements, statements);
            BuildCreateStatementsDiff(diffStatements, statements);
            BuildCommentsDiff(diffStatements, statements);

            return alters;
        }

        private static void BuildAlterIndexesDiff(DiffStatements diffStatements, Statements statements)
        {
            foreach (var (sourceKey, sourceValue) in diffStatements.Source.Indexes)
            {
                foreach (var (targetKey, targetValue) in diffStatements.Target.Indexes)
                {
                    if (Equals(sourceValue, targetValue) && !Equals(sourceKey, targetKey))
                    {
                        statements.AlterIndexes.AppendLine($"ALTER INDEX {targetKey} RENAME TO {sourceKey};");
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
                    statements.TableComments.AppendLine(sourceValue);
                }
            }
            foreach (var (sourceKey, sourceValue) in diffStatements.Source.Comments.Where(c => !diffStatements.Target.Comments.Keys.Contains(c.Key)))
            {
                statements.TableComments.AppendLine(sourceValue);
            }
        }

        private void BuildDropStatementsDiff(DiffStatements diffStatements, Statements statements)
        {
            foreach(var (targetKey, targetValue) in diffStatements.Target.Statements.Where(c => !diffStatements.Source.Statements.Keys.Contains(c.Key)))
            {
                if (targetValue.type != EntryType.Contraint && targetValue.type != EntryType.Index)
                {
                    continue;
                }
                string element = targetValue.type == EntryType.Contraint ? "CONSTRAINT" : "INDEX";
                statements.Drop.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" DROP {element} \"{targetKey.Trim('"')}\";");
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
                    statements.Drop.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" DROP {element} \"{sourceKey.Trim('"')}\";");
                }
                if (sourceValue.statement.IsUniqueStatemnt())
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
                if (statement.IsUniqueStatemnt())
                {
                    statements.Unique.AppendLine(statement);
                }
                else
                {
                    statements.Create.AppendLine(statement);
                }
            }
        }

        private DiffStatements BuildStatementsDicts(TableDumpTransformer source)
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
                }
            }

            foreach(var entry in source.Names.Where(n => n.Value.entryType == EntryType.Contraint))
            {
                result.Source.Statements.Add(entry.Key, 
                    ($"ALTER TABLE ONLY {source.Table.Schema}.\"{source.Table.Name}\" ADD CONSTRAINT {entry.Key.Trim('"')} {entry.Value.content};", EntryType.Contraint));
            }
            foreach (var entry in source.Append)
            {
                AddEntry(entry, result.Source);
            }
            foreach (var entry in Names.Where(n => n.Value.entryType == EntryType.Contraint))
            {
                result.Target.Statements.Add(entry.Key,
                    ($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ADD CONSTRAINT {entry.Key.Trim('"')} {entry.Value.content};", EntryType.Contraint));
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
                sb.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" DROP COLUMN \"{field}\";");
            }
        }

        private void BuildCreateColumnsDiff(TableDumpTransformer source, StringBuilder sb)
        {
            var targetFields = Names.Where(n => n.Value.entryType == EntryType.Field).Select(n => n.Key).ToList();
            foreach (var field in source.Names.Where(n => n.Value.entryType == EntryType.Field && !targetFields.Contains(n.Key)).Select(n => n.Key))
            {
                var (position, content, entryType) = source.Names[field];
                sb.AppendLine($"ALTER TABLE ONLY {source.Table.Schema}.\"{source.Table.Name}\" ADD COLUMN \"{field}\" {content.TrimEnd(',')};");
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
                    sb.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ALTER COLUMN \"{field}\" TYPE {sourceType};");
                }
                if (!string.Equals(sourceDefault, targetDefault))
                {
                    if (string.IsNullOrEmpty(sourceDefault))
                    {
                        sb.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ALTER COLUMN \"{field}\" DROP DEFAULT;");
                    }
                    else
                    {
                        sb.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ALTER COLUMN \"{field}\" SET DEFAULT {sourceDefault};");
                    }

                }
                if (sourceNotNull != targetNotNull)
                {
                    if (!sourceNotNull)
                    {
                        sb.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ALTER COLUMN \"{field}\" DROP NOT NULL;");
                    }
                    else
                    {
                        sb.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ALTER COLUMN \"{field}\" SET NOT NULL;");
                    }
                }
            }
        }
    }
}
