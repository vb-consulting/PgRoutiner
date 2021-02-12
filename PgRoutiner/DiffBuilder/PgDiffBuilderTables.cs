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
        private string GetDropTablesNotInSource(Statements statements)
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
            statements.Drop.Append(dropConstraints);
            return dropTables.ToString();
        }

        private string GetAlterTargetTables(Statements statements)
        {
            StringBuilder dropConstraints = new();
            StringBuilder createConstraints = new();
            StringBuilder alters = new();
            foreach (var tableKey in targetTables.Keys.Where(k => sourceTables.Keys.Contains(k)))
            {
                var tableValue = targetTables[tableKey];
                var sourceTransformer = new TableDumpTransformer(tableValue, sourceBuilder.GetRawTableDumpLines(tableValue, settings.DiffPrivileges)).BuildLines();
                var targetTransformer = new TableDumpTransformer(tableValue, targetBuilder.GetRawTableDumpLines(tableValue, settings.DiffPrivileges)).BuildLines();
                if (targetTransformer.Equals(sourceTransformer))
                {
                    continue;
                }
                var tableAlters = targetTransformer.ToDiff(sourceTransformer, statements);
                if (tableAlters.Length > 0)
                {
                    alters.Append(tableAlters);
                }
            }
            return alters.ToString();
        }

        private void BuildCreateTablesNotInTarget(StringBuilder sb, Statements statements)
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
                foreach (var line in transformer.Create)
                {
                    sb.AppendLine(line);
                }
                foreach (var line in transformer.Append)
                {
                    if (line.IsUniqueStatemnt())
                    {
                        statements.Unique.AppendLine(line);
                    }
                    else
                    {
                        statements.Create.AppendLine(line);
                    }
                }
            }
            if (header)
            {
                AddComment(sb, "#endregion CREATE TABLES");
            }
        }
    }
}
