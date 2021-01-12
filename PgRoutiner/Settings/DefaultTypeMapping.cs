using System.Collections.Generic;

namespace PgRoutiner
{
    public partial class Settings
    {
        public static IDictionary<string, string> DefaultTypeMapping { get; set; } = new Dictionary<string, string>
        {
            {"text", "string"},
            {"bpchar", "string"},
            {"xml", "string"},
            {"inet", "string"},
            {"daterange", "TimeSpan"},
            {"float8", "double"},
            {"bool", "bool"},
            {"int2", "short"},
            {"timestamptz", "DateTime"},
            {"int8", "long"},
            {"timetz", "DateTime"},
            {"time", "DateTime"},
            {"char", "string"},
            {"date", "DateTime"},
            {"numeric", "decimal"},
            {"varchar", "string"},
            {"jsonb", "string"},
            {"float4", "float"},
            {"json", "string"},
            {"int4", "int"}
        };
    }
}
