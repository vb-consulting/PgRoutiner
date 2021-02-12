using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using System.Reflection;
using System.Text;

namespace PgRoutiner
{
    public record Table(string Schema, string Name);
    public record Routine(string Schema, string Name, string Params);
    public class Statements
    {
        public StringBuilder Drop { get; } = new();
        public StringBuilder Unique { get; } = new();
        public StringBuilder Create { get; } = new();
        public StringBuilder AlterIndexes { get; } = new();
        public StringBuilder TableComments { get; } = new();
        public StringBuilder TableGrants { get; } = new();
    }

    public partial class PgDiffBuilder : CodeHelpers
    {
        private readonly NpgsqlConnection target;
        private readonly PgDumpBuilder sourceBuilder;
        private readonly string title;
        private readonly PgDumpBuilder targetBuilder;

        private readonly Dictionary<Table, PgItem> sourceTables;
        private readonly Dictionary<Table, PgItem> sourceViews;
        private readonly Dictionary<Routine, PgRoutineGroup> sourceRoutines;

        private readonly Dictionary<Table, PgItem> targetTables;
        private readonly Dictionary<Table, PgItem> targetViews;
        private readonly Dictionary<Routine, PgRoutineGroup> targetRoutines;

        public PgDiffBuilder(
            Settings settings, 
            NpgsqlConnection source, 
            NpgsqlConnection target, 
            PgDumpBuilder sourceBuilder, 
            PgDumpBuilder targetBuilder,
            string title) : base(settings)
        {
            this.target = target;
            this.sourceBuilder = sourceBuilder;
            this.title = title;
            this.targetBuilder = targetBuilder;

            var ste = source.GetTables(new Settings { Schema = settings.Schema });
            this.sourceTables = ste
                .Where(t => t.Type == PgType.Table)
                .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
            this.sourceViews = ste
                .Where(t => t.Type == PgType.View)
                .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
            this.sourceRoutines = source
                .GetRoutineGroups(new Settings { Schema = settings.Schema })
                .SelectMany(g => g)
                .ToDictionary(r => new Routine(r.SpecificSchema, 
                    r.RoutineName, 
                $"({string.Join(", ", r.Parameters.Select(p => $"{p.Name} {p.DataType}{(p.Array ? "[]" : "")}"))})"), 
                    r => r);

            var tte = target.GetTables(new Settings { Schema = settings.Schema });
            this.targetTables = tte
                .Where(t => t.Type == PgType.Table)
                .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
            this.targetViews = tte
                .Where(t => t.Type == PgType.View)
                .ToDictionary(t => new Table(t.Schema, t.Name), t => t);
            this.targetRoutines = target
                .GetRoutineGroups(new Settings { Schema = settings.Schema })
                .SelectMany(g => g)
                .ToDictionary(r => new Routine(r.SpecificSchema,
                    r.RoutineName,
                $"({string.Join(", ", r.Parameters.Select(p => $"{p.Name} {p.DataType}{(p.Array ? "[]" : "")}"))})"),
                    r => r);
        }

        public string Build(Action<string, int, int> stage = null)
        {
            StringBuilder sb = new();
            Statements statements = new();
            if (stage == null)
            {
                stage = (_, _, _) => { };
            }
            var total = 7;
            stage("scanning routines not in source to drop...", 1, total);
            BuildDropRoutinesNotInSource(sb);
            stage("scanning views not in source to drop...", 2, total);
            BuildDropViewsNotInSource(sb);
            stage("scanning tables not in target to create...", 3, total);
            BuildCreateTablesNotInTarget(sb, statements);
            stage("scanning tables not in source to drop...", 4, total);
            var dropTables = GetDropTablesNotInSource(statements);
            stage("scanning tables in source different from target to alter...", 5, total);
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

            //stage("scanning views in source different from target to replace...", 6, total);

            //stage("scanning routines in source different from target to replace...", 7, total);

            stage("scanning views not in target to create...", 6, total);
            BuildCreateViewsNotInTarget(sb);
            stage("scanning routines no in target to create...", 97 total);
            BuildCreateRoutinesNotInTarget(sb);

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
}
