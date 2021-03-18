using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class PgDiffBuilder
    {
        private void BuildDropSchemasNotInSource(StringBuilder sb)
        {
            var header = false;
            foreach (var schema in targetSchemas.Where(s => !sourceSchemas.Contains(s)))
            {
                if (!header)
                {
                    AddComment(sb, "#region DROP NON EXISTING SCHEMAS");
                    header = true;
                }
                sb.AppendLine($"DROP SCHEMA {schema} CASCADE;");
            }
            if (header)
            {
                AddComment(sb, "#endregion DROP NON EXISTING SCHEMAS");
            }
        }

        private void BuildCreateSchemasNotInTarget(StringBuilder sb)
        {
            var header = false;
            foreach (var schema in sourceSchemas.Where(s => !targetSchemas.Contains(s)))
            {
                if (!header)
                {
                    AddComment(sb, "#region CREATE NON EXISTING SCHEMAS");
                    header = true;
                }
                var content = new SchemaDumpTransformer(schema, SourceLines).BuildLines(ignorePrepend: true);
                sb.AppendLine(content.ToString());
            }
            if (header)
            {
                AddComment(sb, "#endregion CREATE NON EXISTING SCHEMAS");
            }
        }
    }
}
