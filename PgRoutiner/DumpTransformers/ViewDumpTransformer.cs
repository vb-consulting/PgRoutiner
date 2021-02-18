using System;
using System.Collections.Generic;
using System.Text;

namespace PgRoutiner
{
    public class ViewDumpTransformer : DumpTransformer
    {
        public ViewDumpTransformer(List<string> lines) : base(lines) {}

        public ViewDumpTransformer BuildLines(
            bool dbObjectsCreateOrReplace = false,
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

            const string startSequence = "CREATE VIEW ";
            const string endSequence = ";";

            string statement = "";

            foreach (var l in lines)
            {
                var line = l;
                if (line.StartsWith("--") || line.StartsWith("SET ") || (!isCreate && line.StartsWith("SELECT ")))
                {
                    continue;
                }

                var createStart = line.StartsWith(startSequence);
                var createEnd = line.EndsWith(endSequence);
                if (createStart)
                {
                    if (dbObjectsCreateOrReplace)
                    {
                        line = line.Replace("CREATE", "CREATE OR REPLACE");
                    }
                    isPrepend = false;
                    isCreate = true;
                    isAppend = false;
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
                    if (!createStart)
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
