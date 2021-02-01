using System;
using System.Collections.Generic;
using System.Text;

namespace PgRoutiner
{
    public partial class DumpTransformer
    {
        public static string TransformRoutine(PgItem routine, List<string> lines, Settings settings)
        {
            List<string> prepend = new();
            List<string> create = new();
            List<string> append = new();

            bool isPrepend = true;
            bool isCreate = false;
            bool isAppend = true;

            var name1 = $"{routine.Schema}.{routine.Name}";
            var name2 = $"{routine.Schema}.\"{routine.Name}\"";
            var name3 = $"\"{routine.Schema}\".\"{routine.Name}\"";
            var name4 = $"\"{routine.Schema}\".{routine.Name}";
            
            var startSequence1 = $"CREATE {routine.TypeName} {name1}";
            var startSequence2 = $"CREATE {routine.TypeName} {name2}";
            var startSequence3 = $"CREATE {routine.TypeName} {name3}";
            var startSequence4 = $"CREATE {routine.TypeName} {name4}";

            const string endSequence = "$$;";

            string statement = "";

            foreach (var l in lines)
            {
                var line = l;
                if (!isCreate && (line.StartsWith("--") || line.StartsWith("SET ") || line.StartsWith("SELECT ")))
                {
                    continue;
                }
                if (!isCreate && string.IsNullOrEmpty(statement) && !line.Contains(name1) && !line.Contains(name2) && !line.Contains(name3) && !line.Contains(name4))
                {
                    continue;
                }

                var createStart = line.StartsWith(startSequence1) || line.StartsWith(startSequence2) || line.StartsWith(startSequence3) || line.StartsWith(startSequence4);
                var createEnd = line.EndsWith(endSequence);
                if (createStart)
                {
                    if (!settings.DbObjectsNoCreateOrReplace)
                    {
                        line = line.Replace("CREATE", "CREATE OR REPLACE");
                    }
                    isPrepend = false;
                    isCreate = true;
                    isAppend = false;
                    if (create.Count > 0)
                    {
                        create.Add("");
                    }
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
