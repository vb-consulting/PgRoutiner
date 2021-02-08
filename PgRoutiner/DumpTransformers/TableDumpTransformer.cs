using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class TableDumpTransformer
    {
        private readonly PgItem table;
        private readonly List<string> lines;

        public enum EntryType { Field, Contraint }

        public List<string> Prepend { get; } = new();
        public List<string> Create { get; } = new();
        public Dictionary<string, (int position, string content, EntryType entryType)> Names { get; } = new();
        public List<string> Append { get; } = new();

        public TableDumpTransformer(PgItem table, List<string> lines)
        {
            this.table = table;
            this.lines = lines;
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
                var createEnd = line.StartsWith(endSequence);
                if (createStart)
                {
                    isPrepend = false;
                    isCreate = true;
                    isAppend = false;
                }
                if (isCreate)
                {
                    Create.Add(line);
                    if (!createStart && !createEnd)
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

        public string ToCreateString()
        {
            List<string> appendResult = new();
            foreach(var line in Append)
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
                    Create.Insert(len - 1, string.Concat("    ", fieldStatement));
                    var prev = Create[len - 2];
                    Create[len - 2] = string.Concat(prev, ",");
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
            return sb.ToString();
        }

        public bool Equals(TableDumpTransformer to)
        {
            return false;
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
                if (!string.Equals(line, Create[idx]))
                {
                    return false;
                }
            }
            foreach (var (line, idx) in Append.Select((l, idx) => (l, idx)))
            {
                if (!line.StartsWith("CONSTRAINT"))
                {
                    continue;
                }
                if (!string.Equals(line, to.Append[idx]))
                {
                    return false;
                }
            }
            return true;
        }

        public string ToDiffString(TableDumpTransformer source)
        {
            StringBuilder sb = new();
              /*
            var names = Names.ToDictionary(n => n.Value, n => n.Key);
            var sourceNames = source.Names.ToDictionary(n => n.Value, n => n.Key);
            
            
            foreach (var (line, idx) in Create.Select((l, idx) => (l, idx)))
            {
                var sourceLine = source.Create[idx];
                if (string.Equals(line, sourceLine))
                {
                    continue;
                }
            }

            if (sb.Length == 0)
            {
                return null;
            }
            */
            return sb.ToString();
        }

        private (string name, string statement) ParseGenerated(string line)
        {
            var name = line.FirstWordAfter("COLUMN");
            var exp = line.FirstWordAfter("ADD", '(');
            var par = line.Between('(', ')');
            if (par != null)
            {
                var opts = "";
                var defaultName = $"{table.Schema}.{table.Name}_{name}_seq";
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
            var defaultName = $"{table.Name}_pkey";
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
            var defaultName = $"{table.Name}_{field.Replace("\"", "")}_fkey";
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
            var defaultName = $"{table.Name}_{field.Replace("\"", "")}_key";
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
    }
}
