using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using System.Reflection;
using System.Text;

namespace PgRoutiner
{
    public partial class PgDiffBuilder : CodeHelpers
    {
        private readonly NpgsqlConnection target;
        private readonly PgDumpBuilder sourceBuilder;
        private readonly string title;
        private readonly PgDumpBuilder targetBuilder;

        private record Table(string Schema, string Name);
        private record Routine(string Schema, string Name, string Params);

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

        public string Build()
        {
            StringBuilder sb = new();
            
            BuildDropRoutinesNotInSource(sb);
            BuildDropViewsNotInSource(sb);

            var (dropConstraints1, dropTables) = GetDropTablesNotInSource();
            var (dropConstraints2, addConstraints, alterTables) = GetAlterTargetTables();

            if (!string.IsNullOrEmpty(dropConstraints1) || !string.IsNullOrEmpty(dropConstraints2))
            {
                AddComment(sb, "#region DROP CONSTRAINTS");
                if (!string.IsNullOrEmpty(dropConstraints1))
                {
                    sb.Append(dropConstraints1);
                }
                if (!string.IsNullOrEmpty(dropConstraints2))
                {
                    sb.Append(dropConstraints2);
                }
                AddComment(sb, "#endregion DROP CONSTRAINTS");
            }
            if (!string.IsNullOrEmpty(alterTables) )
            {
                AddComment(sb, "#region ALTER TABLES");
                sb.Append(alterTables);
                AddComment(sb, "#endregion ALTER TABLES");
            }
            if (!string.IsNullOrEmpty(dropTables))
            {
                AddComment(sb, "#region DROP TABLES");
                sb.Append(dropTables);
                AddComment(sb, "#endregion DROP TABLES");
            }
            if (!string.IsNullOrEmpty(addConstraints))
            {
                AddComment(sb, "#region ADD CONSTRAINTS");
                sb.Append(addConstraints);
                AddComment(sb, "#endregion ADD CONSTRAINTS");
            }

            BuildCreateTablesNotInTarget(sb);
            BuildCreateViewsNotInTarget(sb);
            BuildCreateRoutinesNotInTarget(sb);

            if (sb.Length == 0)
            {
                return null;
            }
            sb.Insert(0, $"DO ${title}${NL}BEGIN{NL}{NL}");
            sb.AppendLine();
            sb.AppendLine();
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
