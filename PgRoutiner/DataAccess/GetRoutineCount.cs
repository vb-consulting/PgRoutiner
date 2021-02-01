using System.Linq;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static long GetRoutineCount(this NpgsqlConnection connection, Settings settings) => 
            connection.Read<long>(@"

                select 
                    count(*)
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
                    and (@notSimilarTo is null or r.routine_name not similar to @notSimilarTo)
                    and (@similarTo is null or r.routine_name similar to @similarTo)
            ",
                ("schema", settings.Schema, DbType.AnsiString),
                ("notSimilarTo", settings.NotSimilarTo, DbType.AnsiString),
                ("similarTo", settings.SimilarTo, DbType.AnsiString))
            .Single();
    }
}
