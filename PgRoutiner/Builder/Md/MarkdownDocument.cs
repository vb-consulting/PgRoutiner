using System.Xml.Linq;
using PgRoutiner.DataAccess;
using PgRoutiner.DataAccess.Models;
using static PgRoutiner.Builder.Dump.DumpBuilder;

namespace PgRoutiner.Builder.Md;

public class MarkdownDocument
{
    private string I1 => string.Join("", Enumerable.Repeat(" ", settings.Ident));
    private readonly Current settings;
    private readonly NpgsqlConnection connection;
    private readonly string NL = "\n";

    private readonly string connectionName = null;
    private readonly string baseUrl = null;

    private const string Open = "<!-- ";
    private const string Close = " -->";
    private const string CommentStatement = "comment on ";
    private const string Param = "@until-end-tag";
    private static string CommentIs => $" is {Param};";
    private static string StartTag(string on, string name) => $"{Open}{CommentStatement}{on} {name}{CommentIs}{Close}";
    private static string EndTag => $"{Open}end{Close}";
    private static string Hashtag(string name) => $"<a id=\"user-content-{name}\" href=\"#{name}\">#</a>";

    public MarkdownDocument(Current settings, NpgsqlConnection connection)
    {
        this.settings = settings;
        this.connection = connection;
        if (settings.MdIncludeSourceLinks)
        {
            connectionName = (settings.Connection ?? $"{connection.Host}_{connection.Port}_{connection.Database}").SanitazePath();
            baseUrl = PathoToUrl(string.Format(settings.DbObjectsDir, connectionName));
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

        var scriptName = $"${settings.Connection}_comments_update$";
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
            var routinesHeader = false;
            foreach (var result in connection.GetRoutineComments(settings, schema))
            {
                if (!routinesHeader)
                {
                    content.AppendLine();
                    content.AppendLine("## Routines");
                    routinesHeader = true;
                }

                content.AppendLine();
                content.AppendLine(
                    $"### {result.Type.First().ToString().ToUpper()}{result.Type[1..]} `{schema}.{result.Signature}`");
                var routineAnchor = result.Signature.ToLower()
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(",", "")
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace(" ", "-");
                header.AppendLine($"- {result.Type.First().ToString().ToUpper()}{result.Type[1..]} [`{schema}.{result.Signature}`](#{result.Type.ToLower()}-{schema.ToLower()}{routineAnchor})");
                content.AppendLine();
                content.AppendLine($"- Returns `{result.Returns}`");
                content.AppendLine();
                content.AppendLine($"- Language is `{result.Language}`");
                content.AppendLine();
                if (settings.MdIncludeSourceLinks)
                {
                    var url = GetUrl(string.Equals(result.Type.ToLowerInvariant(), "function", StringComparison.InvariantCulture) ? DumpType.Functions : DumpType.Procedures, schema, result.Name);
                    content.AppendLine($"- Source: [{url}]({url})");
                    content.AppendLine();
                }
                if (settings.MdIncludeExtensionLinks && settings.OutputDir != null)
                {
                    var dir = schema == null ? settings.OutputDir : string.Format(settings.OutputDir, schema == "public" ? "" : schema.ToUpperCamelCase());
                    var url = Path.Combine(settings.MdSourceLinkRoot ?? "", dir, $"{result.Name.ToUpperCamelCase()}.cs")
                        .Replace("..", "")
                        .Replace("\\", "/")
                        .Replace("./", "/")
                        .Replace("//", "/");
                    content.AppendLine($"- Data Access Extension: [{url}]({url})");
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
                    content.AppendLine($"- Unit Test: [{url}]({url})");
                    content.AppendLine();
                }

                content.AppendLine(StartTag(result.Type, $"\"{schema}\".{result.Signature.Replace(result.Name, $"\"{result.Name}\"")}"));
                if (result.Comment != null)
                {
                    content.AppendLine(string.Join($"{NL}{NL}", result.Comment));
                }

                content.AppendLine(EndTag);

                content.AppendLine();
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
            var viewsHeader = false;
            foreach (var result in connection.GetViewComments(settings, schema))
            {
                if (!viewsHeader)
                {
                    content.AppendLine();
                    content.AppendLine("## Views");
                    viewsHeader = true;
                }

                string comment = null;
                if (result.Column == null)
                {
                    if (result.Comment != null)
                    {
                        comment = string.Join($"{NL}{NL}", result.Comment);
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

                    content.AppendLine($"### View `{schema}.{result.Table}`");
                    header.AppendLine($"- View [`{schema}.{result.Table}`](#view-{schema.ToLower()}{result.Table.ToLower()})");
                    content.AppendLine();
                    content.AppendLine(StartTag("view", $"\"{schema}\".\"{result.Table}\""));
                    if (comment != null)
                    {
                        content.AppendLine(comment);
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
                            typeMarkup = $" <sub>user definded</sub>";
                        }
                        else
                        {
                            if (!settings.MdSkipEnums)
                            {
                                typeMarkup = $" <sub>user definded `AS ENUM ({enumValue})` [➝](#enum-{schema.ToLower()}-{result.ColumnType.ToLower()})</sub>";
                            }
                            else
                            {
                                typeMarkup = $" <sub>user definded `AS ENUM ({enumValue})`</sub>";
                            }
                        }
                    }
                    content.AppendLine(
                        $"| {(result.IsPk == true ? "**" : "")}`{result.Column}`{(result.IsPk == true ? "**" : "")} " +
                        $"| `{result.ColumnType}`{typeMarkup}" +
                        $"| {StartTag("column", $"\"{schema}\".\"{result.Table}\".\"{result.Column}\"")}{comment}{EndTag} |");
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
        var tablesHeader = false;
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
            foreach (var result in connection.GetTableComments(settings, schema).ToList())
            {
                if (!tablesHeader)
                {
                    content.AppendLine("## Tables");
                    tablesHeader = true;
                }

                string comment = null;
                if (result.Column == null)
                {

                    if (result.Comment != null)
                    {
                        comment = string.Join($"{NL}{NL}", result.Comment);
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

                    content.AppendLine($"### Table `{schema}.{result.Table}`");
                    header.AppendLine($"- Table [`{schema}.{result.Table}`](#table-{schema.ToLower()}{result.Table.ToLower()})");
                    content.AppendLine();
                    content.AppendLine(StartTag("table", $"\"{schema}\".\"{result.Table}\""));

                    if (comment != null)
                    {
                        content.AppendLine(comment);
                    }
                    content.AppendLine(EndTag);

                    if (result.HasPartitions)
                    {
                        content.AppendLine();
                        content.AppendLine("*Partitions*:");
                        var partitions = connection.GetPartitionTables(new PgItem { Name = result.Table, Schema = schema });
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
                    if (result.Comment != null)
                    {
                        comment = string.Join(" ", result.Comment);
                    }
                    var name = $"{schema.ToLower()}-{result.Table.ToLower()}-{result.Column.ToLower()}";
                    
                    string enumValue = null;
                    string typeMarkup = "";
                    if (result.IsUdt == true)
                    {
                        enumValue = connection.GetEnumValueAggregate(schema, result.ColumnType);
                        if (enumValue == null)
                        {
                            typeMarkup = $" <sub>user definded</sub>";
                        }
                        else
                        {
                            if (!settings.MdSkipEnums)
                            {
                                typeMarkup = $" <sub>user definded `AS ENUM ({enumValue})` [➝](#enum-{schema.ToLower()}-{result.ColumnType.ToLower()})</sub>";
                            }
                            else
                            {
                                typeMarkup = $" <sub>user definded `AS ENUM ({enumValue})`</sub>";
                            }
                        }
                    }
                    content.AppendLine(
                        $"| {Hashtag(name)}{(result.IsPk == true ? "**" : "")}`{result.Column}`{(result.IsPk == true ? "**" : "")} " +
                        $"| {result.ConstraintMarkup} " +
                        $"| `{result.ColumnType}`{typeMarkup}" +
                        $"| {result.Nullable} " +
                        $"| {result.DefaultMarkup} " +
                        $"| {StartTag("column", $"\"{schema}\".\"{result.Table}\".\"{result.Column}\"")}{comment}{EndTag} |");
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
            var enumHeader = false;
            foreach (var result in connection.GetEnumComments(settings, schema))
            {
                anyEnums = true;
                if (!enumHeader)
                {
                    content.AppendLine();
                    content.AppendLine("## Enums");
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
                header.AppendLine($"- Enum [`{schema}.{result.Name}`](#{name})");

                if (settings.MdIncludeSourceLinks)
                {
                    var url = GetUrl(DumpType.Types, schema, result.Name);
                    content.AppendLine(
                        $"| {Hashtag(name)}`{schema}.{result.Name}` " +
                        $"| `{result.Values}` " +
                        $"| {StartTag("type", $"\"{schema}\".\"{result.Name}\"")}{result.Comment}{EndTag} " +
                        $"| [{url}]({url}) |");
                }
                else
                {
                    content.AppendLine(
                        $"| {Hashtag(name)}`{schema}.{result.Name}` " +
                        $"| `{result.Values}` " +
                        $"| {StartTag("type", $"\"{schema}\".\"{result.Name}\"")}{result.Comment}{EndTag} |");
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
        header.AppendLine(
            $"- Server: PostgreSQL `{connection.Host}:{connection.Port}`, version `{connection.ServerVersion}`");
        header.AppendLine($"- Local time stamp: `{DateTime.Now:o}`");
        if (schemas.Count == 1)
        {
            header.AppendLine($"- Schema: {schemas.First()}");
        }
        else
        {
            header.AppendLine($"- Schema's: {string.Join(", ", schemas.Select(s => $"`{s}`"))}");
        }
        
        if (settings.MdIncludeSourceLinks)
        {
            if (settings.SchemaDumpFile != null)
            {
                var file = PathoToUrl(string.Format(settings.SchemaDumpFile, connectionName));
                header.AppendLine($"- Schema file: [{file}]({file})");
            }

            if (Current.Value.DataDumpFile != null)
            {
                var file = PathoToUrl(string.Format(settings.DataDumpFile, connectionName));
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
        header.AppendLine("## Table of Contents");
        header.AppendLine();
    }

    private string PathoToUrl(string path)
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
            
            return PathoToUrl(Path.Combine(baseUrl, string.Format(dir, schema == "public" ? "" : schema)));
        }

        var dir = GetDir();
        if (dir == null)
        {
            return null;
        }

        return PathoToUrl(Path.Combine(dir, PgItemExt.GetFileName(new PgItem { Name = name, Schema = schema })));
    }
}
