using System.Collections.Generic;

namespace PgRoutiner
{
    public partial class Settings
    {
        public static IDictionary<string, string> DefaultTypeMapping { get; set; } = new Dictionary<string, string>
        {
            {"text", "string"},
            {"character", "string"},
            {"xml", "string"},
            {"inet", "string"},
            {"daterange", "TimeSpan"},
            {"double precision", "double"},
            {"boolean", "bool"},
            {"smallint", "short"},
            {"timestamp with time zone", "DateTime"},
            {"timestamp without time zone", "DateTime"},
            {"bigint", "long"},
            {"time with time zone", "DateTime"},
            {"time without time zone", "DateTime"},
            {"char", "string"},
            {"date", "DateTime"},
            {"numeric", "decimal"},
            {"character varying", "string"},
            {"jsonb", "string"},
            {"real", "float"},
            {"json", "string"},
            {"integer", "int"}
        };
    }
}
