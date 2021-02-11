using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class TableDumpTransformer
    {
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
                /*
                if (!line.StartsWith("CONSTRAINT"))
                {
                    continue;
                }
                */
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

            var (sourceStatements, targetStatements) = BuildStatementsDicts(source);

            BuildDropStatementsDiff(sourceStatements, targetStatements, statements);
            BuildAlterStatementsDiff(sourceStatements, targetStatements, statements);
            BuildCreateStatementsDiff(sourceStatements, targetStatements, statements);

            return alters;
        }

        private void BuildDropStatementsDiff(
            Dictionary<string, string> sourceStatements, 
            Dictionary<string, string> targetStatements,
            Statements statements)
        {
            foreach(var targetKey in targetStatements.Keys.Where(c => !sourceStatements.Keys.Contains(c)))
            {
                var name = targetKey.StartsWith('"') ? $"\"{targetKey}\"" : targetKey;
                statements.Drop.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" DROP CONSTRAINT {name};");
            }
        }

        private void BuildAlterStatementsDiff(
            Dictionary<string, string> sourceStatements,
            Dictionary<string, string> targetStatements,
            Statements statements)
        {
            foreach (var (sourceKey, sourceValue) in sourceStatements)
            {
                if (!targetStatements.TryGetValue(sourceKey, out var targetValue))
                {
                    continue;
                }
                if (string.Equals(sourceValue, targetValue))
                {
                    continue;
                }
                var name = sourceKey.StartsWith('"') ? sourceKey : $"\"{sourceKey}\"";
                statements.Drop.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" DROP CONSTRAINT {name};");
                var value = sourceValue.TrimEnd(';');
                if (value.Contains("PRIMARY KEY") || value.Contains("UNIQUE"))
                {
                    statements.Unique.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ADD CONSTRAINT {name} {value};{Environment.NewLine}");
                }
                else
                {
                    statements.Create.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ADD CONSTRAINT {name} {value};");
                }
            }
        }

        private void BuildCreateStatementsDiff(
            Dictionary<string, string> sourceStatements,
            Dictionary<string, string> targetStatements,
            Statements statements)
        {
            foreach (var sourceKey in sourceStatements.Keys.Where(c => !targetStatements.Keys.Contains(c)))
            {
                var sourceValue = sourceStatements[sourceKey];
                var name = sourceKey.StartsWith('"') ? sourceKey : $"\"{sourceKey}\"";
                var value = sourceValue.TrimEnd(';');
                if (value.IsUniqueStatemnt())
                {
                    statements.Unique.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ADD CONSTRAINT {name} {value};{Environment.NewLine}");
                }
                else
                {
                    statements.Create.AppendLine($"ALTER TABLE ONLY {Table.Schema}.\"{Table.Name}\" ADD CONSTRAINT {name} {value};");
                }
            }
        }

        private (Dictionary<string, string> sourceStatements, Dictionary<string, string> targetStatements) 
            BuildStatementsDicts(TableDumpTransformer source)
        {
            Dictionary<string, string> sourceStatements = new(); 
            Dictionary<string, string> targetStatements = new();

            static void AddEntry(string entry, Dictionary<string, string> dict)
            {
                var searchIndex = entry.IndexOf("CONSTRAINT ");
                if (searchIndex == -1)
                {
                    return;
                }
                var statements = entry[(searchIndex + "CONSTRAINT ".Length)..];
                var parts = statements.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    dict.Add(parts[0], parts[1]);
                }
            }

            foreach(var entry in source.Names.Where(n => n.Value.entryType == EntryType.Contraint))
            {
                //ALTER TABLE ONLY public.users ADD CONSTRAINT energy_type_check
                //CHECK(((type = 0) OR(type = 181) OR(type = 182) OR(type = 280) OR(type = 380) OR(type = 480)))
                sourceStatements.Add(entry.Key, entry.Value.content);
            }
            foreach (var entry in source.Append)
            {
                AddEntry(entry, sourceStatements);
            }

            foreach (var entry in Names.Where(n => n.Value.entryType == EntryType.Contraint))
            {
                targetStatements.Add(entry.Key, entry.Value.content);
            }
            foreach (var entry in Append)
            {
                AddEntry(entry, targetStatements);
            }

            return (sourceStatements, targetStatements);
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
