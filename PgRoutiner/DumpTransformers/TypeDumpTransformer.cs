using System;
using System.Collections.Generic;
using System.Text;

namespace PgRoutiner
{
    public partial class TypeDumpTransformer : DumpTransformer
    {
        public PgItem Item { get; }
        
        public TypeDumpTransformer(PgItem item, List<string> lines) : base(lines)
        {
            this.Item = item;
        }

        public TypeDumpTransformer BuildLines(
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

            var name1 = $"{Item.Schema}.{Item.Name}";
            var startSequence1 = $"CREATE TYPE {name1} AS ";

            string statement = "";
            const string endSequence = ";";

            bool shouldContinue(string line)
            {
                return !isCreate && string.IsNullOrEmpty(statement) &&
                    !line.Contains(string.Concat(name1, ";")) &&
                    !line.Contains(string.Concat(name1, " "));
            }

            foreach (var l in lines)
            {
                var line = l;
                if (!isCreate && (line.StartsWith("--") || line.StartsWith("SET ") || line.StartsWith("SELECT ")))
                {
                    continue;
                }
                if (shouldContinue(line))
                {
                    continue;
                }

                var createStart = line.StartsWith(startSequence1);
                var createEnd = line.EndsWith(endSequence);
                if (createStart)
                {
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
}
