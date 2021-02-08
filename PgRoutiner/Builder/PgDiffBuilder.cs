using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using System.Reflection;
using System.Text;

namespace PgRoutiner
{
    public class PgDiffBuilder : CodeHelpers
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
            (var dropConstraints, var dropTables) = GetDropTablesNotInSource(sb);
            AlterTargetTables(sb);

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

        private void BuildDropViewsNotInSource(StringBuilder sb)
        {
            var header = false;
            foreach (var viewKey in targetViews.Keys.Where(k => !sourceViews.Keys.Contains(k)))
            {
                if (!header)
                {
                    AddComment(sb, "#region DROP NON EXISTING VIEWS");
                    header = true;
                }

                sb.AppendLine($"DROP VIEW {viewKey.Schema}.\"{viewKey.Name}\";");
            }
            if (header)
            {
                AddComment(sb, "#endregion DROP NON EXISTING VIEWS");
            }
        }

        private (string dropConstraints, string dropTables) GetDropTablesNotInSource(StringBuilder sb)
        {
            StringBuilder dropConstraints = new();
            StringBuilder dropTables = new();
            var tablesToDrop = targetTables.Keys.Where(k => !sourceTables.Keys.Contains(k));
            var foreignKeys = this.target.GetConstraintNames(tablesToDrop.Select(t => (t.Schema, t.Name)).ToArray(), PgConstraint.ForeignKey);

            foreach (var fk in foreignKeys)
            {
                sb.AppendLine($"ALTER TABLE ONLY {fk.Schema}.\"{fk.Table}\" DROP CONSTRAINT \"{fk.Name}\";");
            }
            foreach (var tableKey in tablesToDrop)
            {
                sb.AppendLine($"DROP TABLE {tableKey.Schema}.\"{tableKey.Name}\";");
            }
            return (dropConstraints.ToString(), dropTables.ToString());
            /*
            var tablesToDrop = targetTables.Keys.Where(k => !sourceTables.Keys.Contains(k));
            var foreignKeys = this.target.GetConstraintNames(tablesToDrop.Select(t => (t.Schema, t.Name)).ToArray(), PgConstraint.ForeignKey);
            var header = false;
            foreach (var fk in foreignKeys)
            {
                if (!header)
                {
                    AddComment(sb, "#region DROP NON EXISTING TABLES");
                    header = true;
                }
                sb.AppendLine($"ALTER TABLE ONLY {fk.Schema}.\"{fk.Table}\" DROP CONSTRAINT \"{fk.Name}\";");
            }
            foreach (var tableKey in tablesToDrop)
            {
                if (!header)
                {
                    AddComment(sb, "#region DROP NON EXISTING TABLES");
                    header = true;
                }
                sb.AppendLine($"DROP TABLE {tableKey.Schema}.\"{tableKey.Name}\";");
            }
            if (header)
            {
                AddComment(sb, "#endregion DROP NON EXISTING TABLES");
            }
            */
        }

        private void AlterTargetTables(StringBuilder sb)
        {
            var header = false;
            foreach (var tableKey in targetTables.Keys.Where(k => sourceTables.Keys.Contains(k)))
            {
                var tableValue = targetTables[tableKey];
                var sourceTransformer = new TableDumpTransformer(tableValue, sourceBuilder.GetRawTableDumpLines(tableValue, true)).BuildLines();
                var targetTransformer = new TableDumpTransformer(tableValue, targetBuilder.GetRawTableDumpLines(tableValue, true)).BuildLines();
                if (targetTransformer.Equals(sourceTransformer))
                {
                    continue;
                }
                var diff = targetTransformer.ToDiffString(sourceTransformer);
                if (diff == null)
                {
                    continue;
                }
                if (!header)
                {
                    AddComment(sb, "#region ALTER TABLES");
                    header = true;
                }
                sb.Append(diff);
            }
            if (header)
            {
                AddComment(sb, "#endregion ALTER TABLES");
            }
        }

        private void AddComment(StringBuilder sb, string comment)
        {
            /*
            sb.AppendLine();
            sb.AppendLine("--");
            sb.AppendLine($"-- {comment}");
            sb.AppendLine("--");
            sb.AppendLine();
            */
            sb.AppendLine($"/* {comment} */");
        }
    }
}
