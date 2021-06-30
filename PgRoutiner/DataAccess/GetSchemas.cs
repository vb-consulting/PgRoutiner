using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static string GetSchemaExpression(string field)
        {
            return $"{field} not like 'pg_temp%' and {field} not like 'pg_toast%' and {field} <> 'information_schema' and {field} <> 'pg_catalog'";
        }

        public static IEnumerable<string> GetSchemas(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
        {
            return connection.Read<string>(@$"

                select
                    schema_name
                from
                    information_schema.schemata
                where
                    (   @schema is null or (schema_name similar to @schema)   )
                    and (   {GetSchemaExpression("schema_name")}  )
                    and (   @skipSimilar is null or (schema_name not similar to @skipSimilar)   )
            ", 
            ("schema", settings.Schema, DbType.AnsiString), ("skipSimilar", skipSimilar, DbType.AnsiString));
        }
    }
}
