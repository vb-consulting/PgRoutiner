using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.Builder.DiffBuilder;

public partial class PgDiffBuilder : CodeBuilders.Code
{
    private readonly NpgsqlConnection target;
    private readonly NpgsqlConnection source;
    private readonly Dump.PgDumpBuilder sourceBuilder;
    private readonly Dump.PgDumpBuilder targetBuilder;
    private readonly string title;

    private readonly Dictionary<Table, PgItem> sourceTables;
    private readonly Dictionary<Table, PgItem> sourceViews;
    private readonly Dictionary<Routine, PgRoutineGroup> sourceRoutines;
    private readonly Dictionary<Domain, PgItem> sourceDomains;
    private readonly Dictionary<Type, PgItem> sourceTypes;
    private readonly HashSet<string> sourceSchemas;
    private readonly Dictionary<Seq, PgItem> sourceSeqs;

    private readonly Dictionary<Table, PgItem> targetTables;
    private readonly Dictionary<Table, PgItem> targetViews;
    private readonly Dictionary<Routine, PgRoutineGroup> targetRoutines;
    private readonly Dictionary<Domain, PgItem> targetDomains;
    private readonly Dictionary<Type, PgItem> targetTypes;
    private readonly HashSet<string> targetSchemas;
    private readonly Dictionary<Seq, PgItem> targetSeqs;

    private List<string> _sourceLines = null;
    private List<string> _targetLines = null;
    private List<string> SourceLines => _sourceLines;
    private List<string> TargetLines => _targetLines;


    public PgDiffBuilder(
        Current settings,
        NpgsqlConnection source,
        NpgsqlConnection target,
        Dump.PgDumpBuilder sourceBuilder,
        Dump.PgDumpBuilder targetBuilder,
        string title) : base(settings, null)
    {
        this.target = target;
        this.source = source;
        this.sourceBuilder = sourceBuilder;
        this.title = title;
        this.targetBuilder = targetBuilder;

        var diffSettings = new Current { SchemaSimilarTo = settings.SchemaSimilarTo, SchemaNotSimilarTo = settings.SchemaNotSimilarTo };
        var ste = source.GetTables(diffSettings, skipSimilar: settings.DiffSkipSimilarTo);
        this.sourceTables = ste
            .Where(t => t.Type == PgType.Table)
            .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
        this.sourceViews = ste
            .Where(t => t.Type == PgType.View)
            .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
        this.sourceRoutines = source
            .GetRoutineGroups(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .SelectMany(g => g)
            .ToDictionary(r => new Routine(r.SpecificSchema,
                r.RoutineName,
            $"({string.Join(", ", r.Parameters.Select(p => $"{p.Name} {p.DataType}{(p.IsArray ? "[]" : "")}"))})"),
                r => r);
        this.sourceDomains = source.GetDomains(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .ToDictionary(t => new Domain(t.Schema, t.Name), t => t);
        this._sourceLines = sourceBuilder.GetRawRoutinesDumpLines(settings.DiffPrivileges, out var sourceTypes);
        this.sourceTypes = source.FilterTypes(sourceTypes, diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .ToDictionary(t => new Type(t.Schema, t.Name), t => t);
        this.sourceSchemas = source.GetSchemas(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .Where(s => !string.Equals(s, "public"))
            .ToHashSet();
        this.sourceSeqs = source.GetSequences(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .ToDictionary(t => new Seq(t.Schema, t.Name), t => t);

        var tte = target.GetTables(diffSettings, skipSimilar: settings.DiffSkipSimilarTo);
        this.targetTables = tte
            .Where(t => t.Type == PgType.Table)
            .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
        this.targetViews = tte
            .Where(t => t.Type == PgType.View)
            .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
        this.targetRoutines = target
            .GetRoutineGroups(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .SelectMany(g => g)
            .ToDictionary(r => new Routine(r.SpecificSchema,
                r.RoutineName,
            $"({string.Join(", ", r.Parameters.Select(p => $"{p.Name} {p.DataType}{(p.IsArray ? "[]" : "")}"))})"),
                r => r);
        this.targetDomains = target.GetDomains(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .ToDictionary(t => new Domain(t.Schema, t.Name), t => t);
        this._targetLines = targetBuilder.GetRawRoutinesDumpLines(settings.DiffPrivileges, out var targetTypes);
        this.targetTypes = target.FilterTypes(targetTypes, diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .ToDictionary(t => new Type(t.Schema, t.Name), t => t);
        this.targetSchemas = target.GetSchemas(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .Where(s => !string.Equals(s, "public"))
            .ToHashSet();
        this.targetSeqs = target.GetSequences(diffSettings, skipSimilar: settings.DiffSkipSimilarTo)
            .ToDictionary(t => new Seq(t.Schema, t.Name), t => t);
    }

    public string Build(Action<string, int, int> stage = null)
    {
        StringBuilder sb = new();
        Statements statements = new();
        if (stage == null)
        {
            stage = (_, _, _) => { };
        }
        var total = 16;
        var current = 1;

        stage("scanning new schemas...", current++, total);
        BuildCreateSchemasNotInTarget(sb);
        stage("scanning routines to drop...", current++, total);
        BuildDropRoutinesNotInSource(sb);
        stage("scanning types to drop...", current++, total);
        BuildDropTypesNotInSource(sb);
        stage("scanning views to drop...", current++, total);
        BuildDropViews(sb);

        stage("scanning sequences not in target to create...", current++, total);
        BuildCreateSeqsNotInTarget(sb);

        stage("scanning domains not in target to create...", current++, total);
        BuildCreateDomainsNotInTarget(sb);

        stage("scanning domains to alter...", current++, total);
        BuildAlterDomains(sb);
        stage("scanning tables not in target to create...", current++, total);
        BuildCreateTablesNotInTarget(sb, statements);
        stage("scanning tables not in source to drop...", current++, total);
        var dropTables = GetDropTablesNotInSource(statements);
        stage("scanning tables in source different from target to alter...", current++, total);
        var alters = GetAlterTargetTables(statements);

        if (statements.Drop.Length > 0)
        {
            AddComment(sb, "#region DROP ARTIFACTS");
            sb.Append(statements.Drop);
            AddComment(sb, "#endregion DROP ARTIFACTS");
        }
        if (!string.IsNullOrEmpty(alters))
        {
            AddComment(sb, "#region ALTER TABLES");
            sb.Append(alters);
            AddComment(sb, "#endregion ALTER TABLES");
        }
        if (!string.IsNullOrEmpty(dropTables))
        {
            AddComment(sb, "#region DROP TABLES");
            sb.Append(dropTables);
            AddComment(sb, "#endregion DROP TABLES");
        }
        if (statements.Unique.Length > 0 || statements.Create.Length > 0)
        {
            AddComment(sb, "#region CREATE TABLE ARTIFACTS");
            if (statements.Unique.Length > 0)
            {
                sb.Append(statements.Unique);
            }
            if (statements.Create.Length > 0)
            {
                sb.Append(statements.Create);
            }
            AddComment(sb, "#endregion CREATE TABLE ARTIFACTS");
        }
        if (statements.AlterIndexes.Length > 0)
        {
            AddComment(sb, "#region ALTER INDEXES");
            sb.Append(statements.AlterIndexes);
            AddComment(sb, "#endregion ALTER INDEXES");
        }
        if (statements.TableGrants.Length > 0)
        {
            AddComment(sb, "#region TABLE PRIVILEGES");
            sb.Append(statements.TableGrants);
            AddComment(sb, "#endregion TABLE PRIVILEGES");
        }
        if (statements.TableComments.Length > 0)
        {
            AddComment(sb, "#region TABLE COMMENTS");
            sb.Append(statements.TableComments);
            AddComment(sb, "#endregion TABLE COMMENTS");
        }

        stage("scanning sequences to drop...", current++, total);
        BuildDropSeqsNotInSource(sb);

        stage("scanning domains to drop...", current++, total);
        BuildDropDomainsNotInSource(sb);

        stage("scanning views to create...", current++, total);
        BuildCreateViews(sb);
        stage("scanning types not in target to create...", current++, total);
        BuildCreateTypesNotInTarget(sb);
        stage("scanning routines not in target to create...", current++, total);
        BuildCreateRoutinesNotInTarget(sb);

        if (statements.CreateTriggers.Length > 0 || statements.DropTriggers.Length > 0)
        {
            AddComment(sb, "#region TRIGGERS");
            if (statements.DropTriggers.Length > 0)
            {
                sb.Append(statements.DropTriggers);
            }
            if (statements.CreateTriggers.Length > 0)
            {
                sb.Append(statements.CreateTriggers);
            }
            AddComment(sb, "#endregion TRIGGERS");
        }
        stage("scanning schemas to drop...", current++, total);
        BuildDropSchemasNotInSource(sb);

        if (sb.Length == 0)
        {
            return null;
        }
        sb.Insert(0, $"DO ${title}${NL}BEGIN{NL}{NL}");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("--ROLLBACK; /* uncomment this line to test this script */");
        sb.AppendLine("END");
        sb.AppendLine($"${title}$");
        sb.AppendLine("LANGUAGE plpgsql;");
        return sb.ToString();
    }

    private static void AddComment(StringBuilder sb, string comment)
    {
        sb.AppendLine();
        sb.AppendLine($"/* {comment} */");
        sb.AppendLine();
    }
}
