using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public partial class PgDiffBuilder
    {
        private void BuildDropTypesNotInSource(StringBuilder sb)
        {
            var header = false;
            foreach (var typeKey in targetTypes.Keys.Where(k => !sourceTypes.Keys.Contains(k)))
            {
                if (!header)
                {
                    AddComment(sb, "#region DROP NON EXISTING TYPES");
                    header = true;
                }
                sb.AppendLine($"DROP TYPE {typeKey.Schema}.{typeKey.Name};");
            }
            if (header)
            {
                AddComment(sb, "#endregion DROP NON EXISTING TYPES");
            }
        }

        private void BuildCreateTypesNotInTarget(StringBuilder sb)
        {
            var header = false;
            foreach (var typeKey in sourceTypes.Keys.Where(k => !targetTypes.Keys.Contains(k)))
            {
                if (!header)
                {
                    AddComment(sb, "#region CREATE NON EXISTING TYPES");
                    header = true;
                }
                var item = sourceTypes[typeKey];
                var content = new TypeDumpTransformer(item, SourceLines).BuildLines(ignorePrepend: true);
                sb.AppendLine(content.ToString());
            }
            if (header)
            {
                AddComment(sb, "#endregion CREATE NON EXISTING TYPES");
            }
        }
    }
}
