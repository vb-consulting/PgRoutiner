﻿using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DumpTransformers;

public partial class TableDumpTransformer : DumpTransformer
{
    private readonly NpgsqlConnection connection;

    public PgItem Item { get; }
    public enum EntryType { Field, Contraint, Index, Sequence, Trigger }
    public Dictionary<string, (int position, string content, EntryType entryType)> Names { get; } = new();

    private string name;

    public TableDumpTransformer(PgItem table, List<string> lines, NpgsqlConnection connection) : base(lines)
    {
        this.Item = table;
        this.connection = connection;
    }

    public TableDumpTransformer BuildLines()
    {
        Prepend.Clear();
        Create.Clear();
        Names.Clear();
        Append.Clear();

        bool isPrepend = true;
        bool isCreate = false;
        bool isAppend = true;

        const string startSequence = "CREATE TABLE ";
        const string endSequence = ");";

        string statement = "";
        int i = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("--") || line.StartsWith("SET ") || line.StartsWith("SELECT "))
            {
                continue;
            }

            var createStart = line.StartsWith(startSequence);
            var createEnd = line.StartsWith(endSequence) || line.EndsWith(endSequence);
            var createClosed = string.Equals(line, ")");
            if (createStart)
            {
                isPrepend = false;
                isCreate = true;
                isAppend = false;
                name = line.Trim().Replace("CREATE TABLE", "").Replace("(", "").Trim();
            }
            if (isCreate)
            {
                Create.Add(line);
                if (!createStart && !createEnd && !createClosed)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("CONSTRAINT"))
                    {
                        var parts = trimmed.Split(" ", 3);
                        Names.Add(parts[1], (i, parts[2], EntryType.Contraint));
                    }
                    else
                    {
                        var parts = trimmed.Split(" ", 2);
                        Names.Add(parts[0], (i, parts[1], EntryType.Field));
                    }
                }
                i++;
                if (createEnd)
                {
                    isPrepend = false;
                    isCreate = false;
                    isAppend = true;
                }
            }
            else
            {
                statement = string.Concat(statement, statement == "" ? "" : Environment.NewLine, line);
                if (statement.EndsWith(";"))
                {
                    if (isPrepend)
                    {
                        if (!statement.Contains("DROP CONSTRAINT"))
                        {
                            Prepend.Add(statement);
                        }
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

    public override string ToString()
    {
        List<string> appendResult = new();
        foreach (var line in Append)
        {
            string field = null;
            string fieldStatement = null;
            if (line.Contains("GENERATED"))
            {
                (field, fieldStatement) = ParseGenerated(line);
            }
            else if (line.Contains("PRIMARY KEY"))
            {
                (field, fieldStatement) = ParsePk(line);
            }
            else if (line.Contains("FOREIGN KEY"))
            {
                (field, fieldStatement) = ParseFk(line);
            }
            else if (line.Contains("UNIQUE"))
            {
                (field, fieldStatement) = ParseUnique(line);
            }
            else if (line.Contains("SET DEFAULT"))
            {
                (field, fieldStatement) = ParseDefault(line);
            }

            if (field != null && fieldStatement != null)
            {
                if (Names.TryGetValue(field, out var entry))
                {
                    var createLine = Create[entry.position];
                    if (createLine.EndsWith(","))
                    {
                        createLine = createLine.Remove(createLine.Length - 1);
                        createLine = string.Concat(createLine, " ", fieldStatement, ",");
                    }
                    else
                    {
                        createLine = string.Concat(createLine, " ", fieldStatement);
                    }
                    Create[entry.position] = createLine;
                }
                else
                {
                    appendResult.Add(line);
                }
            }
            else if (field == null && fieldStatement != null)
            {
                var len = Create.Count;
                int delta = 1;
                var entry = Create[len - delta];
                if (string.Equals(entry, ")") || entry.StartsWith("PARTITION BY"))
                {
                    while (true)
                    {
                        entry = Create[len - delta];
                        if (!string.Equals(entry, ")") && !entry.StartsWith("PARTITION BY"))
                        {
                            delta--;
                            break;
                        }
                        if (delta == len)
                        {
                            break;
                        }
                        delta++;
                    }
                }
                
                Create.Insert(len - delta, string.Concat("    ", fieldStatement));
                var prev = Create[len - delta - 1];
                Create[len - delta - 1] = string.Concat(prev, ",");
            }
            else
            {
                appendResult.Add(line);
            }
        }

        StringBuilder sb = new();
        if (Prepend.Count > 0)
        {
            sb.Append(string.Join(Environment.NewLine, Prepend));
            sb.AppendLine();
            sb.AppendLine();
        }
        sb.Append(string.Join(Environment.NewLine, Create));
        sb.AppendLine();
        if (appendResult.Count > 0)
        {
            sb.AppendLine();
            sb.Append(string.Join(Environment.NewLine, appendResult));
            sb.AppendLine();
        }

        // add partitions if any
        var hasPartitions = false;
        foreach(var line in Create)
        {
            if (line.StartsWith("PARTITION BY"))
            {
                hasPartitions = true;
                break;
            }
        }
        if (hasPartitions)
        {
            var partitions = connection.GetPartitionTables(Item).ToList();
            if (partitions.Any())
            {
                sb.AppendLine();
                foreach(var partition in partitions)
                {
                    var partitionName = $"{(string.Equals(partition.Schema, "public") ? "public" : string.Concat("\"", partition.Schema, "\""))}.\"{partition.Table}\"";
                    sb.AppendLine($"CREATE TABLE {partitionName} PARTITION OF {name} {partition.Expression};");
                }
            }
        }

        return sb.ToString();
    }

    private static (string type, string defaultValue, bool notNull) ParseFieldContent(string content)
    {
        string type;
        string defaultValue = null;
        bool notNull = false;
        if (content.EndsWith(" NOT NULL"))
        {
            notNull = true;
            content = content.Substring(0, content.Length - " NOT NULL".Length);
        }
        var searchIndex = content.IndexOf(" DEFAULT ");
        if (searchIndex > -1)
        {
            var defaultPosition = searchIndex + " DEFAULT ".Length;
            defaultValue = content[defaultPosition..];
            content = content.Substring(0, searchIndex);
        }
        type = content;
        return (type, defaultValue, notNull);
    }

    private (string name, string statement) ParseGenerated(string line)
    {
        var name = line.FirstWordAfter("COLUMN");
        var exp = line.FirstWordAfter("ADD", '(');
        var par = line.Between('(', ')');
        if (par != null)
        {
            var opts = "";
            var defaultName = $"{Item.Schema}.{Item.Name}_{name}_seq";
            var segment = par.FirstWordAfter("SEQUENCE NAME");
            if (!string.Equals(defaultName, segment))
            {
                opts = string.Concat(opts, string.Equals("", opts) ? "" : " ", $"SEQUENCE NAME {segment}");
            }
            segment = par.FirstWordAfter("START WITH");
            if (!string.Equals("1", segment))
            {
                opts = string.Concat(opts, string.Equals("", opts) ? "" : " ", $"START {segment}");
            }
            segment = par.FirstWordAfter("INCREMENT BY");
            if (!string.Equals("1", segment))
            {
                opts = string.Concat(opts, string.Equals("", opts) ? "" : " ", $"INCREMENT {segment}");
            }
            if (!par.Contains("NO MINVALUE"))
            {
                segment = par.FirstWordAfter("MINVALUE");
                if (segment != null)
                {
                    opts = string.Concat(opts, string.Equals("", opts) ? "" : " ", $"MINVALUE {segment}");
                }
            }
            if (!par.Contains("NO MAXVALUE"))
            {
                segment = par.FirstWordAfter("MAXVALUE");
                if (segment != null)
                {
                    opts = string.Concat(opts, string.Equals("", opts) ? "" : " ", $"MAXVALUE {segment}");
                }
            }
            segment = par.FirstWordAfter("CACHE");
            if (!string.Equals("1", segment))
            {
                opts = string.Concat(opts, string.Equals("", opts) ? "" : " ", $"CACHE {segment}");
            }
            if (!string.Equals("", opts))
            {
                exp = string.Concat(exp, " ( ", opts, " )");
            }
        }
        return (name, exp);
    }

    private (string name, string statement) ParsePk(string line)
    {
        var defaultName = $"{Item.Name}_pkey";
        var segment = line.FirstWordAfter("ADD CONSTRAINT");
        if (!string.Equals(defaultName, segment))
        {
            var statement = line.FirstWordAfter("ADD", ';');
            return (null, statement);
        }
        var par = line.Between('(', ')');
        if (par != null)
        {
            string statement;
            if (par.Contains(","))
            {
                statement = line.FirstWordAfter(defaultName, ';');
                return (null, statement);
            }
            statement = line.FirstWordAfter(defaultName, '(');
            return (par, statement);
        }
        return (null, null);
    }

    private (string name, string statement) ParseFk(string line)
    {
        var segment = line.FirstWordAfter("FOREIGN KEY");
        var field = segment.Between('(', ')');
        if (field == null)
        {
            return (null, null);
        }
        var defaultName = $"{Item.Name}_{field.Replace("\"", "")}_fkey";
        segment = line.FirstWordAfter("ADD CONSTRAINT");
        string statement;
        if (!string.Equals(defaultName, segment))
        {
            statement = line.FirstWordAfter("ADD", ';');
            return (null, statement);
        }
        statement = line.FirstWordAfter(segment, ';');
        return (null, statement);
    }

    private (string name, string statement) ParseUnique(string line)
    {
        var segment = line.FirstWordAfter("UNIQUE");
        var field = segment.Between('(', ')');
        if (field == null)
        {
            return (null, null);
        }
        var defaultName = $"{Item.Name}_{field.Replace("\"", "")}_key";
        segment = line.FirstWordAfter("ADD CONSTRAINT");

        if (!string.Equals(defaultName, segment))
        {
            var statement = line.FirstWordAfter("ADD", ';');
            return (null, statement);
        }
        var par = line.Between('(', ')');
        if (par != null)
        {
            string statement;
            if (par.Contains(","))
            {
                statement = line.FirstWordAfter(defaultName, ';');
                return (null, statement);
            }
            statement = line.FirstWordAfter(defaultName, '(');
            return (par, statement);
        }
        return (null, null);
    }

    private (string name, string statement) ParseDefault(string line)
    {
        var statement = line.FirstWordAfter("SET DEFAULT");
        var field = line.FirstWordAfter("ALTER COLUMN");
        return (field, $"DEFAULT {statement.TrimEnd(';')}");
    }
}
