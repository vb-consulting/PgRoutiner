using System;
using System.Collections.Generic;
using System.Text;

namespace PgRoutiner
{
    public class RoutineDumpTransformer : DumpTransformer
    {
        public PgItem Routine { get; }

        public RoutineDumpTransformer(PgItem routine, List<string> lines) : base(lines)
        {
            this.Routine = routine;
        }

        public RoutineDumpTransformer BuildLines(
            string paramsString = null,
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

            var name1 = $"{Routine.Schema}.{Routine.Name}{paramsString ?? "("}";
            var name2 = $"{Routine.Schema}.\"{Routine.Name}\"{paramsString ?? "("}";
            var name3 = $"\"{Routine.Schema}\".\"{Routine.Name}\"{paramsString ?? "("}";
            var name4 = $"\"{Routine.Schema}\".{Routine.Name}{paramsString ?? "("}";

            var startSequence1 = $"CREATE {Routine.TypeName} {name1}";
            var startSequence2 = $"CREATE {Routine.TypeName} {name2}";
            var startSequence3 = $"CREATE {Routine.TypeName} {name3}";
            var startSequence4 = $"CREATE {Routine.TypeName} {name4}";

            const string endSequence = "$;";

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
                    if (dbObjectsCreateOrReplace)
                    {
                        line = line.Replace("CREATE", "CREATE OR REPLACE");
                    }
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

        public static string TransformRoutine(PgItem Routine, List<string> lines, 
            string paramsString = null,
            bool dbObjectsCreateOrReplace = false,
            bool ignorePrepend = false,
            Action<string> lineCallback = null)
        {
            List<string> Prepend = new();
            List<string> Create = new();
            List<string> Append = new();

            if (lineCallback == null)
            {
                lineCallback = s => { };
            }

            bool isPrepend = true;
            bool isCreate = false;
            bool isAppend = true;

            var name1 = $"{Routine.Schema}.{Routine.Name}{paramsString ?? ""}";
            var name2 = $"{Routine.Schema}.\"{Routine.Name}\"{paramsString ?? ""}";
            var name3 = $"\"{Routine.Schema}\".\"{Routine.Name}\"{paramsString ?? ""}";
            var name4 = $"\"{Routine.Schema}\".{Routine.Name}{paramsString ?? ""}";
            
            var startSequence1 = $"CREATE {Routine.TypeName} {name1}";
            var startSequence2 = $"CREATE {Routine.TypeName} {name2}";
            var startSequence3 = $"CREATE {Routine.TypeName} {name3}";
            var startSequence4 = $"CREATE {Routine.TypeName} {name4}";

            const string endSequence = "$;";

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
                    if (dbObjectsCreateOrReplace)
                    {
                        line = line.Replace("CREATE", "CREATE OR REPLACE");
                    }
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

            StringBuilder sb = new();
            if (!ignorePrepend && Prepend.Count > 0)
            {
                sb.Append(string.Join(Environment.NewLine, Prepend));
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(string.Join(Environment.NewLine, Create));
            sb.AppendLine();
            if (Append.Count > 0)
            {
                sb.AppendLine();
                sb.Append(string.Join(Environment.NewLine, Append));
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
