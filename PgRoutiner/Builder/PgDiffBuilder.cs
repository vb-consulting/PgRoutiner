using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using System.Reflection;
using System.Text;

namespace PgRoutiner
{
    public class PgDiffBuilder
    {
        private readonly PgDumpBuilder sourceBuilder;
        private readonly PgDumpBuilder targetBuilder;

        private record Routine(string Schema, string Name, string Params);

        private readonly Dictionary<string, PgItem> sourceTables;
        private readonly Dictionary<string, PgItem> sourceViews;
        private readonly Dictionary<Routine, PgRoutineGroup> sourceRoutines;

        private readonly Dictionary<string, PgItem> targetTables;
        private readonly Dictionary<string, PgItem> targetViews;
        private readonly Dictionary<Routine, PgRoutineGroup> targetRoutines;

        public PgDiffBuilder(
            Settings settings, 
            NpgsqlConnection source, 
            NpgsqlConnection target, 
            PgDumpBuilder sourceBuilder, 
            PgDumpBuilder targetBuilder)
        {
            this.sourceBuilder = sourceBuilder;
            this.targetBuilder = targetBuilder;

            var ste = source.GetTables(new Settings { Schema = settings.Schema });
            this.sourceTables = ste
                .Where(t => t.Type == PgType.Table)
                .ToDictionary(t => $"{t.Schema}.{t.Name}", t => t);
            this.sourceViews = ste
                .Where(t => t.Type == PgType.View)
                .ToDictionary(t => $"{t.Schema}.{t.Name}", t => t);
            this.sourceRoutines = source
                .GetRoutineGroups(new Settings { Schema = settings.Schema })
                .SelectMany(g => g)
                .ToDictionary(r => new Routine(r.SpecificSchema, 
                        r.RoutineName, 
                        string.Join(", ", r.Parameters.Select(p => $"{p.Name} {p.DataType}{(p.Array ? "[]" : "")}"))), 
                    r => r);

            var tte = target.GetTables(new Settings { Schema = settings.Schema });
            this.targetTables = tte
                .Where(t => t.Type == PgType.Table)
                .ToDictionary(t => $"{t.Schema}.{t.Name}", t => t);
            this.targetViews = tte
                .Where(t => t.Type == PgType.View)
                .ToDictionary(t => $"{t.Schema}.{t.Name}", t => t);
            this.targetRoutines = target
                .GetRoutineGroups(new Settings { Schema = settings.Schema })
                .SelectMany(g => g)
                .ToDictionary(r => new Routine(r.SpecificSchema,
                        r.RoutineName,
                        string.Join(", ", r.Parameters.Select(p => $"{p.Name} {p.DataType}{(p.Array ? "[]" : "")}"))),
                    r => r);
        }

        public string Build()
        {
            StringBuilder sb = new();
            return sb.ToString();
        }
    }
}
