using PgRoutiner.DataAccess.Models;
using PgRoutiner.DumpTransformers;

namespace PgRoutiner.Builder.DiffBuilder;

public partial class PgDiffBuilder
{
    private readonly Dictionary<Routine, DumpTransformer> routinesToUpdate = new();

    private void BuildDropRoutinesNotInSource(StringBuilder sb)
    {
        var header = false;
        void AddDropView(Routine key, PgRoutineGroup value)
        {
            if (!header)
            {
                AddComment(sb, "#region DROP NON EXISTING ROUTINES");
                header = true;
            }
            sb.AppendLine($"DROP {value.RoutineType.ToUpper()} {key.Schema}.\"{key.Name}\"{key.Params};");
        }
        routinesToUpdate.Clear();
        foreach (var (routineKey, targetValue) in targetRoutines)
        {
            if (!sourceRoutines.TryGetValue(routineKey, out var sourceValue))
            {
                AddDropView(routineKey, targetValue);
            }
            else
            {
                var item = new PgItem
                {
                    Schema = routineKey.Schema,
                    Name = routineKey.Name,
                    TypeName = sourceValue.RoutineType.ToUpper()
                };
                var sourceTransformer = new RoutineDumpTransformer(item, SourceLines)
                    .BuildLines(
                        paramsString: routineKey.Params,
                        dbObjectsCreateOrReplace: true,
                        ignorePrepend: true,
                        lineCallback: null);
                item.TypeName = targetValue.RoutineType.ToUpper();
                var targetTransformer = new RoutineDumpTransformer(item, TargetLines)
                    .BuildLines(
                        paramsString: routineKey.Params,
                        dbObjectsCreateOrReplace: true,
                        ignorePrepend: true,
                        lineCallback: null);
                if (sourceTransformer.Equals(targetTransformer))
                {
                    continue;
                }
                AddDropView(routineKey, sourceValue);
                routinesToUpdate.Add(routineKey, sourceTransformer);
            }
        }
        if (header)
        {
            AddComment(sb, "#endregion DROP NON EXISTING ROUTINES");
        }
    }

    private void BuildCreateRoutinesNotInTarget(StringBuilder sb)
    {
        var routinesToInclude = sourceRoutines.Keys.Where(k => routinesToUpdate.Keys.Contains(k) || !targetRoutines.Keys.Contains(k)).ToList();
        Dictionary<Routine, (string content, HashSet<Routine> references)> result = new();

        foreach (var routineKey in routinesToInclude)
        {
            var routineValue = sourceRoutines[routineKey];
            HashSet<Routine> references = new();
            var isSql = string.Equals(routineValue.Language, "sql", StringComparison.InvariantCultureIgnoreCase);
            if (routinesToUpdate.TryGetValue(routineKey, out var dumpTransformer))
            {
                if (isSql)
                {
                    foreach (var line in dumpTransformer.Create.Union(dumpTransformer.Append))
                    {
                        RoutineLineCallback(line, routineKey, routinesToInclude, references);
                    }
                }
                result.Add(routineKey, (dumpTransformer.ToString(), references));
            }
            else
            {
                var item = new PgItem
                {
                    Schema = routineKey.Schema,
                    Name = routineKey.Name,
                    TypeName = routineValue.RoutineType.ToUpper()
                };
                var content = new RoutineDumpTransformer(item, SourceLines)
                    .BuildLines(
                        paramsString: routineKey.Params,
                        dbObjectsCreateOrReplace: true,
                        ignorePrepend: true,
                        lineCallback: isSql ? line => RoutineLineCallback(line, routineKey, routinesToInclude, references) : null)
                    .ToString();
                result.Add(routineKey, (content, references));
            }
        }

        HashSet<Routine> added = new();
        void AddRoutineRecursively(Routine key, (string content, HashSet<Routine> references) value)
        {
            if (added.Contains(key))
            {
                return;
            }
            foreach (var reference in value.references)
            {
                AddRoutineRecursively(reference, result[reference]);
            }
            sb.AppendLine(value.content);
            added.Add(key);
        }

        var header = false;
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

    private void RoutineLineCallback(string line, Routine routineKey, List<Routine> routinesToInclude, HashSet<Routine> references)
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
}
