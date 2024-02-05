using System.Data;
using PgRoutiner.DataAccess.Models;
using static PgRoutiner.Builder.Dump.DumpBuilder;
using PgRoutiner.DataAccess;

namespace PgRoutiner.Builder.Md;

public class MarkdownDocument
{
    private string I1 => string.Join("", Enumerable.Repeat(" ", settings.Ident));
    private readonly Current settings;
    private readonly NpgsqlConnection connection;
    private readonly string NL = "\n";

    private readonly string connectionName = null;
    private readonly string baseUrl = null;

    private readonly string additionalCommentsSql = null;

    private const string Open = "<!-- ";
    private const string Close = " -->";
    private const string CommentStatement = "comment on ";
    private const string Param = "@until-end-tag";
    private static string CommentIs => $" is {Param};";
    private static string StartTag(string on, string name) => $"{Open}{CommentStatement}{on} {name}{CommentIs}{Close}";
    private static string EndTag => $"{Open}end{Close}";
    //private static string Hashtag(string name) => $"<a id=\"user-content-{name}\" href=\"#{name}\">#</a>";
    private static string Hashtag(string name) => $"<a id=\"user-content-{name}\" href=\"#user-content-{name}\">#</a>";

    public MarkdownDocument(Current settings, NpgsqlConnection connection)
    {
        this.settings = settings;
        this.connection = connection;
        if (settings.MdIncludeSourceLinks)
        {
            connectionName = (settings.Connection ?? $"{connection.Host}_{connection.Port}_{connection.Database}").SanitazePath();
            baseUrl = PathToUrl(string.Format(settings.DbObjectsDir, connectionName));
        }
        if (settings.MdAdditionalCommentsSql != null)
        {
            var file = Path.Combine(Program.CurrentDir, settings.MdAdditionalCommentsSql);
            if (File.Exists(file))
            {
                additionalCommentsSql = File.ReadAllText(file);
            }
            else
            {
                additionalCommentsSql = settings.MdAdditionalCommentsSql;
            }
        }
    }

    public string Build()
    {
        StringBuilder content = new();
        StringBuilder header = new();

        var schemas = connection.GetSchemas(settings, schemaSimilarTo: settings.MdSchemaSimilarTo, schemaNotSimilarTo: settings.MdSchemaNotSimilarTo).ToList();

        BuildHeader(header, schemas);

        if (settings.MdRoutinesFirst)
        {
            BuildRoutines(content, header, schemas);
            BuildTables(content, header, schemas);
            BuildViews(content, header, schemas);
            BuildEnums(content, header, schemas);
        }
        else
        {
            BuildTables(content, header, schemas);
            BuildViews(content, header, schemas);
            BuildEnums(content, header, schemas);
            BuildRoutines(content, header, schemas);
        }

        if (header.Length > 0 && !settings.MdSkipToc)
        {
            header.AppendLine();
        }
        
        return string.Concat(header.ToString(), content.ToString());
    }

    public string BuildDiff(string file)
    {
        StringBuilder result = new();
        var content = File.ReadAllText(file);
        var schemas = connection.GetSchemas(settings).ToList();
        var comments = new Dictionary<string, string>();

        foreach (var schema in schemas)
        {
            var dict = connection
                .GetTableComments(settings, schema)
                .Where(t => t.Comment != null)
                .ToDictionary(
                    t => $"\"{schema}\".\"{t.Table}\"{(t.Column == null ? "" : $".\"{t.Column}\"")}",
                    t => t.Comment);
            dict.ToList().ForEach(x => comments.Add(x.Key, x.Value));

            dict = connection
                .GetViewComments(settings, schema)
                .Where(t => t.Comment != null)
                .ToDictionary(
                    t => $"\"{schema}\".\"{t.Table}\"{(t.Column == null ? "" : $".\"{t.Column}\"")}",
                    t => t.Comment);
            dict.ToList().ForEach(x => comments.Add(x.Key, x.Value));

            dict = connection
                .GetRoutineComments(settings, schema)
                .Where(t => t.Comment != null)
                .ToDictionary(
                    t => $"\"{schema}\".{t.Signature.Replace(t.Name, $"\"{t.Name}\"")}",
                    t => t.Comment);

            dict.ToList().ForEach(x => comments.Add(x.Key, x.Value));
        }

        var scriptName = $"${connection.Database.SanitazeName()}_comments_update$";
        result.AppendLine($"do {scriptName}");
        result.AppendLine("begin");
        result.AppendLine("");

        var start = 0;
        var search = $"{Open}{CommentStatement}";
        var close = Close;
        var endTag = EndTag;
        while (true)
        {
            start = content.IndexOf(search, start, StringComparison.Ordinal);
            if (start == -1)
            {
                break;
            }
            var end = content.IndexOf(close, start, StringComparison.Ordinal);
            if (end == -1)
            {
                break;
            }
            var commentTag = content.Substring(start, end - start + close.Length);
            start = end + close.Length;
            end = content.IndexOf(endTag, start, StringComparison.Ordinal);
            if (end == -1)
            {
                break;
            }
            var comment = content[start..end].Trim();
            start = end;

            var statement = commentTag
                .Replace(Open, "")
                .Replace(Close, "");
            var part = statement
                .Replace(CommentStatement, "")
                .Replace(CommentIs, "");
            var sep = part.IndexOf(" ", StringComparison.Ordinal) + 1;
            var entry = part[sep..];
            if (comments.TryGetValue(entry, out var old))
            {
                var old1 = string.Join(" ", old.Split("\n")).Trim();
                var old2 = string.Join($"{NL}", old.Split("\n")).Trim();
                var old3 = string.Join($"{NL}{NL}", old.Split("\n")).Trim();

                if (!string.Equals(comment, old1) && !string.Equals(comment, old2) && !string.Equals(comment, old3))
                {
                    result.AppendLine($"{I1}{statement.Replace(Param, $"$${comment}$$")}");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(comment))
                {
                    result.AppendLine($"{I1}{statement.Replace(Param, $"$${comment}$$")}");
                }
            }
        }

        result.AppendLine();
        result.AppendLine("end");
        result.AppendLine($"{scriptName};");

        return result.ToString();
    }

    private void BuildRoutines(StringBuilder content, StringBuilder header, List<string> schemas)
    {
        foreach (var schema in schemas)
        {
            if (settings.MdSkipRoutines)
            {
                break;
            }

            var routineComments = connection.GetRoutineComments(settings, schema).ToList();

            var count = routineComments.Count();
            if (count > 0)
            {
                header.AppendLine();
                header.AppendLine($"### {count} Routine{(count == 1 ? "" : "s")} in Schema \"{schema}\"");
                header.AppendLine();
            }

            foreach (var result in routineComments)
            {
                content.AppendLine();
                content.AppendLine(
                    $"## {result.Type.First().ToString().ToUpper()}{result.Type[1..]} \"{schema}\".\"{result.Name}\"");

                content.AppendLine();
                content.AppendLine(StartTag(result.Type, $"\"{schema}\".{result.Signature.Replace(result.Name, $"\"{result.Name}\"")}"));
                if (result.Comment != null)
                {
                    content.AppendLine(string.Join($"{NL}{NL}", result.Comment?.Trim() ?? ""));
                }
                content.AppendLine(EndTag);
                

                if (!settings.MdSkipToc)
                {
                    header.AppendLine($"- {result.Type.First().ToString().ToUpper()}{result.Type[1..]} [`{schema}.{result.Name}`](#{result.Type.ToLower()}-{schema.ToLower()}{result.Name})");
                }

                content.AppendLine();
                content.AppendLine($"- Signature `{schema}.{result.Signature}`");

                if (result.Parameters.Length > 0)
                {
                    content.AppendLine();
                    content.AppendLine($"- Parameters:");
                    foreach(var p in result.Parameters)
                    {
                        content.AppendLine($"    - `{p}`");
                    }
                }
                
                content.AppendLine();
                content.AppendLine($"- Returns `{result.Returns}`");
                content.AppendLine();

                if (result.Returns == "record" || result.Returns == "USER-DEFINED")
                {
                    var records = connection.GetRoutineReturnsRecord(new PgRoutineGroup
                    {
                        SpecificName = result.SpecificName,
                        SpecificSchema = schema
                    }).ToList();
                    if (records.Any())
                    {
                        content.AppendLine($"```");
                        content.AppendLine(result.IsSet ? $"TABLE (" : $"RECORD (");
                        content.AppendLine(string.Join($",{NL}", records.Select(r => $"  {r.Name} {r.DataTypeFormatted}")));
                        content.AppendLine($")");
                        content.AppendLine($"```");
                        content.AppendLine();
                    }
                }

                content.AppendLine($"- Language is `{result.Language}`");
                content.AppendLine();
                if (settings.MdIncludeSourceLinks)
                {
                    var url = GetUrl(
                        string.Equals(result.Type.ToLowerInvariant(), "function", StringComparison.InvariantCulture) ?
                        DumpType.Functions : DumpType.Procedures,
                        schema, result.Name);

                    content.AppendLine($"- Source: [{url}]({url})");
                    content.AppendLine();
                }
                if (settings.MdIncludeExtensionLinks && settings.OutputDir != null)
                {
                    string customDir = null;
                    if (settings.CustomDirs != null)
                    {
                        foreach (var ns in settings.CustomDirs)
                        {
                            //if (this.connection.WithParameters(result.Name, ns.Key).Read<bool>("select $1 similar to $2").Single())
                            if (this.connection.Read<bool>([(result.Name, DbType.AnsiString, null), (ns.Key, DbType.AnsiString, null)], "select $1 similar to $2", r => r.Val<bool>(0)).Single())
                            {
                                customDir = ns.Value;
                                break;
                            }
                        }
                    }

                    var dir = customDir == null ?
                        schema == null ? settings.OutputDir : string.Format(settings.OutputDir, schema == "public" ? "" : schema.ToUpperCamelCase()) :
                        Path.Combine(schema == null ? settings.OutputDir : string.Format(settings.OutputDir, schema == "public" ? "" : schema.ToUpperCamelCase()), customDir);
                    var url = Path.Combine(settings.MdSourceLinkRoot ?? "", dir, $"{result.Name.ToUpperCamelCase()}.cs")
                        .Replace("..", "")
                        .Replace("\\", "/")
                        .Replace("./", "/")
                        .Replace("//", "/");
                    content.AppendLine($"- Data Access Code: [{url}]({url})");
                    content.AppendLine();
                }

                if (settings.MdIncludeUnitTestsLinks && settings.UnitTestsDir != null)
                {
                    string sufix = settings.GetAssumedNamespace();
                    var dir = string.Format(settings.UnitTestsDir, sufix);
                    var url = Path.Combine(dir, schema == "public" ? "" : schema.ToUpperCamelCase(), $"{result.Name.ToUpperCamelCase()}UnitTests.cs")
                        .Replace("..", "")
                        .Replace("\\", "/")
                        .Replace("./", "/")
                        .Replace("//", "/");
                    content.AppendLine($"- Unit Tests: [{url}]({url})");
                    content.AppendLine();
                }

                if (settings.MdIncludeRoutineDefinitions)
                {
                    content.AppendLine($"- Definition:");
                    content.AppendLine();
                    content.AppendLine("```sql");
                    content.AppendLine(result.Definition);
                    content.AppendLine("```");
                }

                content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
            }
        }
    }

    private void BuildViews(StringBuilder content, StringBuilder header, List<string> schemas)
    {
        var any = false;

        foreach (var schema in schemas)
        {
            if (settings.MdSkipViews)
            {
                break;
            }
            //var viewsHeader = false;
            var viewComments = connection.GetViewComments(settings, schema).ToList();
            var count = viewComments.GroupBy(t => t.Table).Count();
            if (count > 0)
            {
                header.AppendLine();
                header.AppendLine($"### {count} View{(count == 1 ? "" : "s")} in Schema \"{schema}\"");
                header.AppendLine();
            }

            foreach (var result in viewComments)
            {
                //if (!viewsHeader)
                //{
                //    content.AppendLine();
                //    content.AppendLine("## Views");
                //    viewsHeader = true;
                //}

                string comment = null;
                if (result.Column == null)
                {
                    if (result.Comment != null)
                    {
                        comment = result.Comment.Replace("\n", "").Replace("\r", "");
                    }
                    content.AppendLine();

                    if (!any)
                    {
                        any = true;
                    }
                    else
                    {
                        content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
                        content.AppendLine();
                    }

                    content.AppendLine($"## View \"{schema}\".\"{result.Table}\"");
                    if (!settings.MdSkipToc)
                    {
                        header.AppendLine($"- View [`{schema}.{result.Table}`](#view-{schema.ToLower()}{result.Table.ToLower()})");
                    }
                    content.AppendLine();
                    content.AppendLine(StartTag("view", $"\"{schema}\".\"{result.Table}\""));
                    if (comment != null)
                    {
                        content.AppendLine(comment?.Trim() ?? "");
                    }

                    content.AppendLine(EndTag);
                    if (settings.MdIncludeSourceLinks)
                    {
                        var url = GetUrl(DumpType.Views, schema, result.Table);
                        content.AppendLine($"- Source: [{url}]({url})");
                    }
                    content.AppendLine();
                    content.AppendLine("| Column | Type | Comment |");
                    content.AppendLine("| ------ | ---- | --------|");
                }
                else
                {
                    if (result.Comment != null)
                    {
                        comment = string.Join(" ", result.Comment);
                    }
                    string enumValue;
                    string typeMarkup = "";
                    if (result.IsUdt == true)
                    {
                        enumValue = connection.GetEnumValueAggregate(schema, result.ColumnType);
                        if (enumValue == null)
                        {
                            typeMarkup = $" <sub>user defined</sub>";
                        }
                        else
                        {
                            if (!settings.MdSkipEnums)
                            {
                                typeMarkup = $" <sub>user defined `AS ENUM ({enumValue})` [➝](#enum-{schema.ToLower()}-{result.ColumnType.ToLower()})</sub>";
                            }
                            else
                            {
                                typeMarkup = $" <sub>user defined `AS ENUM ({enumValue})`</sub>";
                            }
                        }
                    }
                    content.AppendLine(
                        $"| {(result.IsPk == true ? "**" : "")}`{result.Column}`{(result.IsPk == true ? "**" : "")} " +
                        $"| `{result.ColumnType}`{typeMarkup}" +
                        $"| {StartTag("column", $"\"{schema}\".\"{result.Table}\".\"{result.Column}\"")}{comment?.Trim() ?? ""}{EndTag} |");
                }
            }
        }

        if (any)
        {
            content.AppendLine();
            content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
        }
    }

    private void BuildTables(StringBuilder content, StringBuilder header, List<string> schemas)
    {
        var any = false;
        string prevTable = null;
        string lastSchema = null;

        void WriteStats(string schema, string table)
        {
            if (settings.MdIncludeTableStats)
            {
                content.AppendLine();
                content.AppendLine($"- Stats for `{schema}.{table}`:");
                content.AppendLine();
                content.AppendLine("| **Sequence Scan** | **Index Scan** | **Rows** | **Vaccum** | **Analyze** |");
                content.AppendLine("| ----------------- | -------------- | -------- | ---------- | ----------- |");
                var stats = connection.GetTableStats(schema, table);
                content.AppendLine(string.Concat(
                    $"| count={stats.SeqScanCount.FormatStatMdValue()} ",
                    $"| count={stats.IdxScanCount.FormatStatMdValue()} ",
                    $"| inserted={stats.RowsInserted.FormatStatMdValue()}, updated={stats.RowsUpdated.FormatStatMdValue()}, deleted={stats.RowsDeleted.FormatStatMdValue()} ",
                    $"| last={stats.LastVacuum.FormatStatMdValue()}, count={stats.VacuumCount.FormatStatMdValue()} ",
                    $"| last={stats.LastAnalyze.FormatStatMdValue()}, count={stats.AnalyzeCount.FormatStatMdValue()} |"));
                content.AppendLine(string.Concat(
                    $"| rows={stats.SeqScanRows.FormatStatMdValue()} ",
                    $"| rows={stats.IdxScanRows.FormatStatMdValue()} ",
                    $"| live={stats.LiveRows.FormatStatMdValue()}, dead={stats.DeadRows.FormatStatMdValue()} ",
                    $"| last auto={stats.LastAutovacuum.FormatStatMdValue()}, rows inserted since={stats.RowsInsertedSinceVacuum.FormatStatMdValue()} ",
                    $"| last auto={stats.LastAutoanalyze.FormatStatMdValue()}, rows updated since={stats.RowsModifiedSinceAnalyze.FormatStatMdValue()} |"));
                content.AppendLine();
            }
        }

        foreach (var schema in schemas)
        {
            if (settings.MdSkipTables)
            {
                break;
            }
            string additionalTableComment = null;
            Dictionary<string, string> additionalColumnComments = new();
            var tableComments = connection.GetTableComments(settings, schema).ToList();

            var count = tableComments.GroupBy(t => t.Table).Count();
            if (count > 0)
            {
                header.AppendLine();
                header.AppendLine($"### {count} Table{(count == 1 ? "" : "s")} in Schema \"{schema}\"");
                header.AppendLine();
            }

            foreach (var result in tableComments)
            {
                if (result.Column == null && additionalCommentsSql != null)
                {
                    additionalTableComment = null;
                    additionalColumnComments = connection
                        //.WithParameters((schema, DbType.AnsiString), (result.Table, DbType.AnsiString))
                        .Read<(string, string, string)>(
                        [(schema, DbType.AnsiString, null), (result.Table, DbType.AnsiString, null)], 
                        additionalCommentsSql,
                        r => (r.Val<string>(0), r.Val<string>(1), r.Val<string>(2)))
                        .Select(tuple =>
                        {
                            additionalTableComment ??= tuple.Item1;
                            return tuple;
                        })
                        .ToDictionary(tuple => tuple.Item2, tuple => tuple.Item3);
                }

                string comment = null;
                if (result.Column == null)
                {
                    if (result.Comment != null || additionalCommentsSql != null)
                    {
                        comment = string.Concat($"{NL}", $"{result.Comment?.Trim() ?? ""}{NL}{additionalTableComment}".Trim());
                    }

                    if (prevTable != null)
                    {
                        WriteStats(schema, prevTable);
                    }
                    prevTable = result.Table;
                    lastSchema = schema;

                    content.AppendLine();

                    if (!any)
                    {
                        any = true;
                    }
                    else
                    {
                        content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
                        content.AppendLine();
                    }

                    content.AppendLine($"## Table \"{schema}\".\"{result.Table}\"");
                    if (!settings.MdSkipToc)
                    {
                        header.AppendLine($"- Table [`{schema}.{result.Table}`](#table-{schema.ToLower()}{result.Table.ToLower()})");
                    }
                    content.AppendLine();
                    content.AppendLine(StartTag("table", $"\"{schema}\".\"{result.Table}\""));

                    if (comment != null)
                    {
                        content.AppendLine(comment?.Trim() ?? "");
                    }
                    content.AppendLine(EndTag);

                    if (result.HasPartitions)
                    {
                        content.AppendLine();
                        content.AppendLine("*Partitions*:");
                        var partitions = connection.GetPartitionTables(new PgItem { Name = result.Table, Schema = schema }).ToList();
                        content.AppendLine(string.Join(", ", partitions.Select(p => $"`{p.Schema}.{p.Table} {p.Expression}`")));
                    }
                    if (settings.MdIncludeTableCountEstimates)
                    {
                        content.AppendLine($"- Count estimate: **{connection.GetTableEstimatedCount(schema, result.Table).ToString("##,#")}**");
                    }
                    if (settings.MdIncludeSourceLinks)
                    {
                        var url = GetUrl(DumpType.Tables, schema, result.Table);
                        content.AppendLine($"- Source: [{url}]({url})");
                    }

                    content.AppendLine();
                    content.AppendLine("| Column |             | Type | Nullable | Default | Comment |");
                    content.AppendLine("| ------ | ----------- | -----| :------: | ------- | ------- |");
                }
                else
                {
                    additionalColumnComments.TryGetValue(result.Column, out var additional);
                    if (result.Comment != null || additional != null)
                    {
                        if (result.Comment != null)
                        {
                            comment = result.Comment.Replace("\n", "").Replace("\r", "");
                        }
                        if (additional != null)
                        {
                            comment = string.Concat(result.Comment != null ? " " : "", additional);
                        }
                    }
                    var name = $"{schema.ToLower()}-{result.Table.ToLower()}-{result.Column.ToLower()}";
                    
                    string enumValue = null;
                    string typeMarkup = "";
                    if (result.IsUdt == true)
                    {
                        enumValue = connection.GetEnumValueAggregate(schema, result.ColumnType);
                        if (enumValue == null)
                        {
                            typeMarkup = $" <sub>user defined</sub>";
                        }
                        else
                        {
                            if (!settings.MdSkipEnums)
                            {
                                typeMarkup = $" <sub>user defined `AS ENUM ({enumValue})` [➝](#enum-{schema.ToLower()}-{result.ColumnType.ToLower()})</sub>";
                            }
                            else
                            {
                                typeMarkup = $" <sub>user defined `AS ENUM ({enumValue})`</sub>";
                            }
                        }
                    }
                    content.AppendLine(
                        $"| {Hashtag(name)}{(result.IsPk == true ? "**" : "")}`{result.Column}`{(result.IsPk == true ? "**" : "")} " +
                        $"| {result.ConstraintMarkup} " +
                        $"| `{result.ColumnType}`{typeMarkup}" +
                        $"| {result.Nullable} " +
                        $"| {result.DefaultMarkup} " +
                        $"| {StartTag("column", $"\"{schema}\".\"{result.Table}\".\"{result.Column}\"")}{comment?.Trim() ?? ""}{EndTag} |");
                }

            }
        }

        if (any)
        {
            if (prevTable != null)
            {
                WriteStats(lastSchema, prevTable);
            }
            content.AppendLine();
            content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
        }
    }

    private void BuildEnums(StringBuilder content, StringBuilder header, List<string> schemas)
    {
        var anyEnums = false;

        foreach (var schema in schemas)
        {
            if (settings.MdSkipEnums)
            {
                break;
            }
            var enumComments = connection.GetEnumComments(settings, schema).ToList();
            var count = enumComments.Count();
            if (count > 0)
            {
                header.AppendLine();
                header.AppendLine($"### {count} Enum{(count == 1 ? "" : "s")} in Schema \"{schema}\"");
                header.AppendLine();
            }

            var enumHeader = false;
            foreach (var result in enumComments)
            {
                anyEnums = true;
                if (!enumHeader)
                {
                    content.AppendLine();
                    content.AppendLine($"## Enums in Schema \"{schema}\"");
                    content.AppendLine();
                    
                    if (settings.MdIncludeSourceLinks)
                    {
                        content.AppendLine("| Type name | Values | Comment | Source |");
                        content.AppendLine("| --------- | ------ | --------| ------ |");
                    }
                    else
                    {
                        content.AppendLine("| Type name | Values | Comment |");
                        content.AppendLine("| --------- | ------ | --------|");
                    }

                    enumHeader = true;
                }

                var name = $"enum-{schema.ToLower()}-{result.Name.ToLower()}";
                if (!settings.MdSkipToc)
                {
                    header.AppendLine($"- Enum [`{schema}.{result.Name}`](#{name})");
                }
                if (settings.MdIncludeSourceLinks)
                {
                    var url = GetUrl(DumpType.Types, schema, result.Name);
                    content.AppendLine(
                        $"| {Hashtag(name)}`{schema}.{result.Name}` " +
                        $"| `{result.Values}` " +
                        $"| {StartTag("type", $"\"{schema}\".\"{result.Name}\"")}{result.Comment?.Trim() ?? ""}{EndTag} " +
                        $"| [{url}]({url}) |");
                }
                else
                {
                    content.AppendLine(
                        $"| {Hashtag(name)}`{schema}.{result.Name}` " +
                        $"| `{result.Values}` " +
                        $"| {StartTag("type", $"\"{schema}\".\"{result.Name}\"")}{result.Comment?.Trim() ?? ""}{EndTag} |");
                }

            }
        }

        if (anyEnums)
        {
            content.AppendLine();
            content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
        }
    }

    private void BuildHeader(StringBuilder header, List<string> schemas)
    {
        header.AppendLine($"# Dictionary for database `{connection.Database}`");
        header.AppendLine();
        

        if (!settings.MdSkipHeader)
        {
            header.AppendLine(
                $"- Server: PostgreSQL `{connection.Host}:{connection.Port}`, version `{connection.ServerVersion}`");
            header.AppendLine($"- Local time stamp: `{DateTime.Now:o}`");
            if (schemas.Count == 1)
            {
                header.AppendLine($"- Schema: `{schemas.First()}`");
            }
            else
            {
                header.AppendLine($"- Schema's: {string.Join(", ", schemas.Select(s => $"`{s}`"))}");
            }
        
            if (settings.MdIncludeSourceLinks)
            {
                if (settings.SchemaDumpFile != null)
                {
                    var file = PathToUrl(string.Format(settings.SchemaDumpFile, connectionName));
                    header.AppendLine($"- Schema file: [{file}]({file})");
                }

                if (Current.Value.DataDumpFile != null)
                {
                    var file = PathToUrl(string.Format(settings.DataDumpFile, connectionName));
                    var line = $"- Data file: [{file}]({file})";
                    if (settings.DataDumpTables != null && settings.DataDumpTables.Count > 0)
                    {
                        line = string.Concat(line, 
                            " for tables ", 
                            string.Join(", ", settings.DataDumpTables.Select(t =>
                            {
                                var split = t.Split('.');
                                if (split.Length == 1)
                                {
                                    return $"[{t}](#table-public{t.ToLower()})";
                                }
                                return $"[{t}](#table-{split[0].ToLower()}{split[1].ToLower()})";
                            })));
                    }
                    header.AppendLine(line);
                }
            }
            header.AppendLine();
        }

        if (!settings.MdSkipToc)
        {
            header.AppendLine("## Table of Contents");
            //header.AppendLine();
        }
    }

    private string PathToUrl(string path)
    {
        if (settings.MdSourceLinkRoot == null)
        {
            return path
                .Replace("\\", "/")
                .Replace("./", "/");
        }
        return Path.Combine(settings.MdSourceLinkRoot, path)
            .Replace("\\", "/")
            .Replace("./", "/")
            .Replace("//", "/");
    }

    private string GetUrl(DumpType type, string schema, string name)
    {
        string GetDir()
        {
            settings.DbObjectsDirNames.TryGetValue(type.ToString(), out var dir);
            if (dir == null)
            {
                return null;
            }

            string extraDir = null;
            if (name != null)
            {
                if (settings.CustomDirs != null)
                {
                    foreach (var ns in settings.CustomDirs)
                    {
                        //if (this.connection.WithParameters(name, ns.Key).Read<bool>("select $1 similar to $2").Single())
                        if (this.connection.Read<bool>([(name, DbType.AnsiString, null), (ns.Key, DbType.AnsiString, null)], "select $1 similar to $2", r => r.Val<bool>(0)).Single())
                        {
                            extraDir = ns.Value;
                            break;
                        }
                    }
                }
                if (extraDir != null)
                {
                    return PathToUrl(Path.Combine(Path.Combine(baseUrl, string.Format(dir, schema == "public" ? "" : schema)), extraDir));
                }
            }

            return PathToUrl(Path.Combine(baseUrl, string.Format(dir, schema == "public" ? "" : schema)));
        }

        var dir = GetDir();
        if (dir == null)
        {
            return null;
        }

        return PathToUrl(Path.Combine(dir, PgItemExt.GetFileName(new PgItem { Name = name, Schema = schema })));
    }
}
