using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Npgsql;
using System.Reflection;
using System.Text;

namespace PgRoutiner
{
    public partial class PgDiffBuilder
    {
        private (string dropConstraints, string dropTables) GetDropTablesNotInSource()
        {
            StringBuilder dropConstraints = new();
            StringBuilder dropTables = new();
            var tablesToDrop = targetTables.Keys.Where(k => !sourceTables.Keys.Contains(k));
            var foreignKeys = this.target.GetConstraintNames(tablesToDrop.Select(t => (t.Schema, t.Name)).ToArray(), PgConstraint.ForeignKey);

            foreach (var fk in foreignKeys)
            {
                dropConstraints.AppendLine($"ALTER TABLE ONLY {fk.Schema}.\"{fk.Table}\" DROP CONSTRAINT \"{fk.Name}\";");
            }
            foreach (var tableKey in tablesToDrop)
            {
                dropTables.AppendLine($"DROP TABLE {tableKey.Schema}.\"{tableKey.Name}\";");
            }
            return (dropConstraints.ToString(), dropTables.ToString());
        }

        private (string dropConstraints, string addConstraints, string alterTables) GetAlterTargetTables()
        {
            StringBuilder dropConstraints = new();
            StringBuilder addConstraints = new();
            StringBuilder alterTables = new();
            foreach (var tableKey in targetTables.Keys.Where(k => sourceTables.Keys.Contains(k)))
            {
                var tableValue = targetTables[tableKey];
                var sourceTransformer = new TableDumpTransformer(tableValue, sourceBuilder.GetRawTableDumpLines(tableValue, true)).BuildLines();
                var targetTransformer = new TableDumpTransformer(tableValue, targetBuilder.GetRawTableDumpLines(tableValue, true)).BuildLines();
                if (targetTransformer.Equals(sourceTransformer))
                {
                    continue;
                }
                var (constraints, fields) = targetTransformer.ToDiff(sourceTransformer);
                foreach(var c in constraints)
                {
                    dropConstraints.AppendLine($"ALTER TABLE ONLY {tableKey.Schema}.\"{tableKey.Name}\" DROP CONSTRAINT \"{c.Key}\";");
                    // TODO primary keys and uniques first!!!
                    addConstraints.AppendLine($"ALTER TABLE ONLY {tableKey.Schema}.\"{tableKey.Name}\" ADD CONSTRAINT \"{c.Key}\" {c.Value};");
                }
                foreach (var f in fields)
                {
                    alterTables.AppendLine($"ALTER TABLE ONLY {tableKey.Schema}.\"{tableKey.Name}\" {f.Value};");
                }
            }
            return (dropConstraints.ToString(), addConstraints.ToString(), alterTables.ToString());
        }

        private void BuildCreateTablesNotInTarget(StringBuilder sb)
        {
            StringBuilder first = new();
            StringBuilder appends = new();
            var header = false;
            foreach (var tableKey in sourceTables.Keys.Where(k => !targetTables.Keys.Contains(k)))
            {
                var tableValue = sourceTables[tableKey];
                if (!header)
                {
                    AddComment(sb, "#region CREATE TABLES");
                    header = true;
                }
                var transformer = new TableDumpTransformer(tableValue, sourceBuilder.GetRawTableDumpLines(tableValue, true)).BuildLines();
                foreach(var line in transformer.Create)
                {
                    sb.AppendLine(line);
                }
                foreach (var line in transformer.Append)
                {
                    if (line.Contains("PRIMARY KEY") || line.Contains("UNIQUE"))
                    {
                        first.AppendLine(line);
                    }
                    else
                    {
                        appends.AppendLine(line);
                    }
                }
            }
            if (first.Length > 0)
            {
                sb.Append(first);
            }
            if (appends.Length > 0)
            {
                sb.Append(appends);
            }
            if (header)
            {
                AddComment(sb, "#endregion CREATE TABLES");
            }
        }
    }
}
