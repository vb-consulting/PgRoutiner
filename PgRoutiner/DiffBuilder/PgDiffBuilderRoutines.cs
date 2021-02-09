using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using System.Reflection;
using System.Text;

namespace PgRoutiner
{
    public partial class PgDiffBuilder
    { 
        private List<string> _routineLines = null;
        private List<string> RoutineLines 
        { 
            get 
            { 
                if (_routineLines != null)
                {
                    return _routineLines;
                }
                return _routineLines = sourceBuilder.GetRawRoutinesDumpLines(true);
            } 
        }

        private void BuildDropRoutinesNotInSource(StringBuilder sb)
        {
            var header = false;
            foreach(var routineKey in targetRoutines.Keys.Where(k => !sourceRoutines.Keys.Contains(k)))
            {
                if (!header)
                {
                    AddComment(sb, "#region DROP NON EXISTING ROUTINES");
                    header = true;
                }

                var routineValue = targetRoutines[routineKey];
                sb.AppendLine($"DROP {routineValue.RoutineType.ToUpper()} {routineKey.Schema}.\"{routineKey.Name}\"{routineKey.Params};");
            }
            if (header)
            {
                AddComment(sb, "#endregion DROP NON EXISTING ROUTINES");
            }
        }

        private void BuildCreateRoutinesNotInTarget(StringBuilder sb)
        {
            var header = false;
            var routinesToInclude = sourceRoutines.Keys.Where(k => !targetRoutines.Keys.Contains(k)).ToList();
            Dictionary<Routine, (string content, HashSet<Routine> references)> result = new();

            foreach (var routineKey in routinesToInclude)
            {
                var routineValue = sourceRoutines[routineKey];
                var isSql = string.Equals(routineValue.Language, "sql", StringComparison.InvariantCultureIgnoreCase);
                HashSet<Routine> references = new();
                void LineCallback(string line)
                {
                    foreach (var reference in routinesToInclude)
                    {
                        if (reference == routineKey || references.Contains(reference))
                        {
                            continue;
                        }

                        var search = $"{reference.Name}(";
                        var searhIndex = line.IndexOf(search);
                        if (searhIndex > -1)
                        {
                            searhIndex += search.Length;
                            var paramsSubstring = line[searhIndex..line.IndexOf(')', searhIndex)];
                            if (paramsSubstring.Split(',').Length == sourceRoutines[reference].Parameters.Count)
                            {
                                references.Add(reference);
                            }
                        }
                    }
                }

                var content = DumpTransformer.TransformRoutine(
                    new PgItem { Schema = routineKey.Schema, Name = routineKey.Name, TypeName = routineValue.RoutineType.ToUpper()},
                    RoutineLines,
                    paramsString: routineKey.Params,
                    dbObjectsNoCreateOrReplace: true,
                    ignorePrepend: true,
                    lineCallback: isSql ? LineCallback : null);

                result[routineKey] = (content, references);
            }

            HashSet<Routine> added = new();
            void AddRoutineRecursively(Routine key, (string content, HashSet<Routine> references) value)
            {
                if (added.Contains(key))
                {
                    return;
                }
                foreach(var reference in value.references)
                {
                    AddRoutineRecursively(reference, result[reference]);
                }
                sb.AppendLine(value.content);
                added.Add(key);
            }

            foreach (var routine in result)
            {
                if (!header)
                {
                    AddComment(sb, "#region CREATE NON EXISTING ROUTINES");
                    header = true;
                }
                AddRoutineRecursively(routine.Key, routine.Value);
            }
            if (header)
            {
                AddComment(sb, "#endregion CREATE NON EXISTING ROUTINES");
            }
        }
    }
}
