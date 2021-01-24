using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PgRoutiner
{
    public class MarkdownDocument
    {
        private readonly Settings settings;
        private readonly NpgsqlConnection connection;

        private const string Open = "<!-- ";
        private const string Close = " -->";
        private const string CommentStatement = "comment on ";
        private const string Param = "@until-end-tag";
        private static string CommentIs => $" is {Param};";
        private static string StartTag(string on, string name) => $"{Open}{CommentStatement}{on} {name}{CommentIs}{Close}";
        private static string EndTag => $"{Open}end{Close}";
        private static string Hashtag(string name) => $"<a id=\"user-content-{name}\" href=\"#{name}\">#</a>";

        public MarkdownDocument(Settings settings, NpgsqlConnection connection)
        {
            this.settings = settings;
            this.connection = connection;
        }

        public string Build()
        {
            StringBuilder content = new();
            StringBuilder header = new();

            var schemas = connection.GetSchemas(settings).ToList();

            BuildHeader(header, schemas);

            var writeToc = false;

            BuildTables(content, header, schemas, ref writeToc);
            BuildViews(content, header, schemas, ref writeToc);
            BuildRoutines(content, header, schemas, writeToc);

            return string.Concat(header.ToString(), content.ToString());
        }

        private void BuildRoutines(StringBuilder content, StringBuilder header, List<string> schemas, bool writeToc)
        {
            foreach (var schema in schemas)
            {
                if (!settings.CommentsMdRoutines)
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
                    var routineAnchor = result.Signature.ToLower().Replace("(", "").Replace(")", "").Replace(",", "").Replace(" ", "-");
                    header.AppendLine($"- {result.Type.First().ToString().ToUpper()}{result.Type[1..]} [`{schema}.{result.Signature}`](#{result.Type.ToLower()}-{schema.ToLower()}{routineAnchor})");
                    content.AppendLine();
                    content.AppendLine($"- Returns `{result.Returns}`");
                    content.AppendLine();
                    content.AppendLine($"- Language is `{result.Language}`");
                    content.AppendLine();
                    content.AppendLine(StartTag(result.Type, $"{schema}.{result.Signature.Replace(result.Name, $"\"{result.Name}\"")}"));
                    if (result.Comment != null)
                    {
                        content.AppendLine(result.Comment);
                    }

                    content.AppendLine(EndTag);
                    if (writeToc)
                    {
                        content.AppendLine();
                        content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
                    }
                    else
                    {
                        writeToc = true;
                    }
                }
            }
        }

        private void BuildViews(StringBuilder content, StringBuilder header, List<string> schemas, ref bool writeToc)
        {
            var anyViews = false;

            foreach (var schema in schemas)
            {
                if (!settings.CommentsMdViews)
                {
                    break;
                }
                var viewsHeader = false;
                foreach (var result in connection.GetTableComments(settings, schema, isTable: false))
                {
                    if (!viewsHeader)
                    {
                        content.AppendLine();
                        content.AppendLine("## Views");
                        viewsHeader = true;
                    }

                    if (result.Column == null)
                    {
                        content.AppendLine();

                        if (writeToc)
                        {
                            content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
                            content.AppendLine();
                        }
                        else
                        {
                            writeToc = true;
                        }
                        if (!anyViews)
                        {
                            anyViews = true;
                        }

                        content.AppendLine($"### View `{schema}.{result.Table}`");
                        header.AppendLine($"- View [`{schema}.{result.Table}`](#view-{schema.ToLower()}{result.Table.ToLower()})");
                        content.AppendLine();
                        content.AppendLine(StartTag("view", $"{schema}.\"{result.Table}\""));
                        if (result.Comment != null)
                        {
                            content.AppendLine(result.Comment);
                        }

                        content.AppendLine(EndTag);
                        content.AppendLine();
                        content.AppendLine("| Column | Type | Comment |");
                        content.AppendLine("| ------ | ---- | --------|");
                    }
                    else
                    {
                        content.AppendLine(
                            $"| `{result.Column}` " +
                            $"| `{result.ColumnType}` " +
                            $"| {StartTag("column", $"{schema}.\"{result.Table}\".\"{result.Column}\"")}{result.Comment}{EndTag} |");
                    }
                }
            }

            if (anyViews)
            {
                content.AppendLine();
                content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
            }
        }

        private void BuildTables(StringBuilder content, StringBuilder header, List<string> schemas, ref bool writeToc)
        {
            var anyTables = false;
            var tablesHeader = false;
            foreach (var schema in schemas)
            {
                foreach (var result in connection.GetTableComments(settings, schema, isTable: true))
                {
                    if (!tablesHeader)
                    {
                        content.AppendLine("## Tables");
                        tablesHeader = true;
                    }

                    if (result.Column == null)
                    {
                        content.AppendLine();

                        if (writeToc)
                        {
                            content.AppendLine("<a href=\"#table-of-contents\" title=\"Table of Contents\">&#8673;</a>");
                            content.AppendLine();
                        }
                        else
                        {
                            writeToc = true;
                        }

                        content.AppendLine($"### Table `{schema}.{result.Table}`");
                        header.AppendLine($"- Table [`{schema}.{result.Table}`](#table-{schema.ToLower()}{result.Table.ToLower()})");
                        content.AppendLine();
                        content.AppendLine(StartTag("table", $"{schema}.\"{result.Table}\""));
                        if (result.Comment != null)
                        {
                            content.AppendLine(result.Comment);
                        }

                        if (!anyTables)
                        {
                            anyTables = true;
                        }
                        content.AppendLine(EndTag);
                        content.AppendLine();
                        content.AppendLine("| Column |             | Type | Nullable | Default | Comment |");
                        content.AppendLine("| ------ | ----------- | -----| -------- | ------- | ------- |");
                    }
                    else
                    {
                        var name = $"{schema.ToLower()}-{result.Table.ToLower()}-{result.Column.ToLower()}";
                        content.AppendLine(
                            $"| {Hashtag(name)}`{result.Column}` " +
                            $"| {result.ConstraintMarkup} " +
                            $"| `{result.ColumnType}` " +
                            $"| {result.Nullable} " +
                            $"| {result.DefaultMarkup} " +
                            $"| {StartTag("column", $"{schema}.\"{result.Table}\".\"{result.Column}\"")}{result.Comment}{EndTag} |");
                    }
                }
            }

            if (anyTables)
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
            header.AppendLine();
            header.AppendLine("## Table of Contents");
            header.AppendLine();
        }
    }
}
