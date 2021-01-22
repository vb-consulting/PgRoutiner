using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static IEnumerable<PgItem> GetRoutines(this NpgsqlConnection connection, Settings settings) =>
            connection.Read<(string Schema, string Name, string Type)>(@"

                select 
                    distinct
                    r.routine_schema,
                    r.routine_name,
                    r.routine_type
                from
                    information_schema.routines r
                where
                    r.external_language <> 'INTERNAL'
                    and
                    (
                        (   @schema is not null and r.specific_schema similar to @schema   )
                        or
                        (   r.specific_schema not like 'pg_%' and r.specific_schema <> 'information_schema' )
                    )

            ", ("schema", settings.Schema, DbType.AnsiString))
            .Select(t => new PgItem
            {
                Schema = t.Schema,
                Name = t.Name,
                TypeName = t.Type,
                Type = t.Type switch
                {
                    "FUNCTION" => PgType.Function,
                    "PROCEDURE" => PgType.Procedure,
                    _ => PgType.Unknown
                }
            });
    }
}
