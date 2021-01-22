﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRoutiner
{
    public partial class DumpTransformer
    {
        public static string TransformView(List<string> lines)
        {
            List<string> prepend = new();
            List<string> create = new();
            List<string> append = new();

            bool isPrepend = true;
            bool isCreate = false;
            bool isAppend = true;

            const string startSequence = "CREATE VIEW ";
            const string endSequence = ";";

            string statement = "";

            foreach(var line in lines)
            {
                if (line.StartsWith("--") || line.StartsWith("SET ") || (!isCreate && line.StartsWith("SELECT ")))
                {
                    continue;
                }

                var createStart = line.StartsWith(startSequence);
                var createEnd = line.EndsWith(endSequence);
                if (createStart)
                {
                    isPrepend = false;
                    isCreate = true;
                    isAppend = false;
                }
                if (isCreate)
                {
                    create.Add(line);
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
                            prepend.Add(statement);
                        }
                        else if (isAppend)
                        {
                            append.Add(statement);
                        }
                        statement = "";
                    }
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
            if (append.Count > 0)
            {
                sb.AppendLine();
                sb.Append(string.Join(Environment.NewLine, append));
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
