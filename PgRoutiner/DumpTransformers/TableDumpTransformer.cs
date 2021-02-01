using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class DumpTransformer
    {
        public static string TransformTable(PgItem table, List<string> lines)
        {
            List<string> prepend = new();
            List<string> create = new();
            Dictionary<string, int> names = new();
            List<string> append = new();

            bool isPrepend = true;
            bool isCreate = false;
            bool isAppend = true;

            const string startSequence = "CREATE TABLE ";
            const string endSequence = ");";

            string statement = "";
            int i = 0;

            foreach(var line in lines)
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
                    create.Add(line);
                    if (!createStart && !createEnd)
                    {
                        names.Add(line.Trim().Split(" ").First(), i);
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
                                prepend.Add(statement);
                            }
                        }
                        else if (isAppend)
                        {
                            append.Add(statement);
                        }
                        statement = "";
                    }
                }
            }

            List<string> appendResult = new();
            foreach(var line in append)
            {
                string field = null;
                string fieldStatement = null;
                if (line.Contains("GENERATED"))
                {
                    (field, fieldStatement) = ParseGenerated(table, line);
                }
                else if (line.Contains("PRIMARY KEY"))
                {
                    (field, fieldStatement) = ParsePk(table, line);
                }
                else if (line.Contains("FOREIGN KEY"))
                {
                    (field, fieldStatement) = ParseFk(table, line);
                }
                else if (line.Contains("UNIQUE"))
                {
                    (field, fieldStatement) = ParseUnique(table, line);
                }

                if (field != null && fieldStatement != null)
                {
                    if (names.TryGetValue(field, out var fieldIndex))
                    {
                        var createLine = create[fieldIndex];
                        if (createLine.EndsWith(","))
                        {
                            createLine = createLine.Remove(createLine.Length - 1);
                            createLine = string.Concat(createLine, " ", fieldStatement, ",");
                        }
                        else
                        {
                            createLine = string.Concat(createLine, " ", fieldStatement);
                        }
                        create[fieldIndex] = createLine;
                    }
                    else
                    {
                        appendResult.Add(line);
                    }
                }
                else if (field == null && fieldStatement != null)
                {
                    var len = create.Count;
                    create.Insert(len - 1, string.Concat("    ", fieldStatement));
                    var prev = create[len - 2];
                    create[len - 2] = string.Concat(prev, ",");
                }
                else
                {
                    appendResult.Add(line);
                }
            }

            StringBuilder sb = new();
            if (prepend.Count > 0)
            {
                sb.Append(string.Join(Environment.NewLine, prepend));
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(string.Join(Environment.NewLine, create));
            sb.AppendLine();
            if (appendResult.Count > 0)
            {
                sb.AppendLine();
                sb.Append(string.Join(Environment.NewLine, appendResult));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static (string name, string statement) ParseGenerated(PgItem table, string line)
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

        private static (string name, string statement) ParsePk(PgItem table, string line)
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

        private static (string name, string statement) ParseFk(PgItem table, string line)
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

        private static (string name, string statement) ParseUnique(PgItem table, string line)
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
