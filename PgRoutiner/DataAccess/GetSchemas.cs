using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static IEnumerable<string> GetSchemas(this NpgsqlConnection connection, Settings settings) =>
            connection.Read<string>(@"

                select
                    schema_name
                from
                    information_schema.schemata
                where
                    (   @schema is not null and schema_name similar to @schema   )
                    or
                    (   schema_name not like 'pg_%' and schema_name <> 'information_schema' )
            ",
                ("schema", settings.Schema, DbType.AnsiString));
    }
}
