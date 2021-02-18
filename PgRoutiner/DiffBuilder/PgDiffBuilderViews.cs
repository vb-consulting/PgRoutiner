using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class PgDiffBuilder
    {
        private readonly char[] terminators = new[] { ' ', ';', '"', '\n', '\r' };
        private readonly Dictionary<Table, DumpTransformer> viewsToUpdate = new();

        private void BuildDropViews(StringBuilder sb)
        {
            Dictionary<Table, HashSet<Table>> result = new();
            viewsToUpdate.Clear();
            var targetViewsKeys = targetViews.Keys.ToList();
            foreach (var (viewKey, viewValue) in targetViews)
            {
                HashSet<Table> references = new();
                if (!sourceViews.TryGetValue(viewKey, out var sourceValue))
                {
                    new ViewDumpTransformer(targetBuilder.GetRawTableDumpLines(viewValue, settings.DiffPrivileges))
                        .BuildLines(
                            dbObjectsCreateOrReplace: true, 
                            ignorePrepend: true, 
                            lineCallback: line => ViewLineCallback(line, viewKey, targetViewsKeys, references));

                    result.Add(viewKey, references);
                }
                else
                {
                    var targetTransformer = new ViewDumpTransformer(targetBuilder.GetRawTableDumpLines(viewValue, settings.DiffPrivileges))
                        .BuildLines(
                            dbObjectsCreateOrReplace: true,
                            ignorePrepend: true,
                            lineCallback: line => ViewLineCallback(line, viewKey, targetViewsKeys, references));

                    var sourceTransformer = new ViewDumpTransformer(sourceBuilder.GetRawTableDumpLines(viewValue, settings.DiffPrivileges))
                        .BuildLines(
                            dbObjectsCreateOrReplace: true,
                            ignorePrepend: true,
                            lineCallback: null);
                    
                    if (targetTransformer.Equals(sourceTransformer))
                    {
                        continue;
                    }

                    result.Add(viewKey, references);
                    viewsToUpdate.Add(viewKey, sourceTransformer);
                }
            }

            HashSet<Table> added = new();
            Stack<string> statements = new();
            void AddViewRecursively(Table key, HashSet<Table> references)
            {
                if (added.Contains(key))
                {
                    return;
                }
                foreach (var reference in references)
                {
                    if (result.ContainsKey(reference))
                    {
                        AddViewRecursively(reference, result[reference]);
                    }
                }
                statements.Push($"DROP VIEW {key.Schema}.\"{key.Name}\";");
                added.Add(key);
            }

            foreach (var routine in result)
            {
                AddViewRecursively(routine.Key, routine.Value);
            }

            var header = false;
            while (statements.TryPop(out var statement))
            {
                if (!header)
                {
                    AddComment(sb, "#region DROP NON EXISTING VIEWS");
                    header = true;
                }
                sb.AppendLine(statement);
            }
            if (header)
            {
                AddComment(sb, "#endregion DROP NON EXISTING VIEWS");
            }
        }

        private void BuildCreateViews(StringBuilder sb)
        {
            var viewsToInclude = sourceViews.Keys.Where(k => viewsToUpdate.Keys.Contains(k) || !targetViews.Keys.Contains(k)).ToList();
            Dictionary<Table, (string content, HashSet<Table> references)> result = new();
            
            foreach (var viewKey in viewsToInclude)
            {
                var viewValue = sourceViews[viewKey];
                HashSet<Table> references = new();

                if (viewsToUpdate.TryGetValue(viewKey, out var dumpTransformer))
                {
                    foreach(var line in dumpTransformer.Create.Union(dumpTransformer.Append))
                    {
                        ViewLineCallback(line, viewKey, viewsToInclude, references);
                    }

                    result.Add(viewKey, (dumpTransformer.ToString(), references));
                }
                else
                {
                    var content = new ViewDumpTransformer(sourceBuilder.GetRawTableDumpLines(viewValue, settings.DiffPrivileges))
                        .BuildLines(
                            dbObjectsCreateOrReplace: true,
                            ignorePrepend: true,
                            lineCallback: line => ViewLineCallback(line, viewKey, viewsToInclude, references))
                        .ToString();

                    result.Add(viewKey, (content, references));
                }
            }

            HashSet<Table> added = new();
            void AddViewRecursively(Table key, (string content, HashSet<Table> references) value)
            {
                if (added.Contains(key))
                {
                    return;
                }
                foreach (var reference in value.references)
                {
                    if (result.ContainsKey(reference))
                    {
                        AddViewRecursively(reference, result[reference]);
                    }
                }
                sb.AppendLine(value.content);
                added.Add(key);
            }

            var header = false;
            foreach (var routine in result)
            {
                if (!header)
                {
                    AddComment(sb, "#region CREATE VIEWS");
                    header = true;
                }
                AddViewRecursively(routine.Key, routine.Value);
            }
            if (header)
            {
                AddComment(sb, "#endregion CREATE VIEWS");
            }
        }

        private void ViewLineCallback(string line, Table viewKey, List<Table> viewsToInclude, HashSet<Table> references)
        {
            foreach (var reference in viewsToInclude)
            {
                if (reference == viewKey || references.Contains(reference))
                {
                    continue;
                }
                var search = reference.Name;
                var searhIndex = line.IndexOf(search);
                if (searhIndex > -1)
                {
                    var endIndex = searhIndex + search.Length;
                    if (endIndex == line.Length || terminators.Contains(line[endIndex]))
                    {
                        if (searhIndex == 0)
                        {
                            references.Add(reference);
                        }
                        else
                        {
                            var startsWith = line[searhIndex - 1];
                            if (startsWith == '.' || startsWith == '"')
                            {
                                var schemaLen = reference.Schema.Length;
                                var schemaStart = searhIndex - 1 - schemaLen + (startsWith == '"' ? -1 : 0);
                                if (schemaStart < 0)
                                {
                                    continue;
                                }
                                if (string.Equals(reference.Schema, line.Substring(schemaStart, schemaLen)))
                                {
                                    references.Add(reference);
                                }
                            }
                            else if (terminators.Contains(startsWith))
                            {
                                references.Add(reference);
                            }
                        }
                    }
                }
            }
        }
    }
}
