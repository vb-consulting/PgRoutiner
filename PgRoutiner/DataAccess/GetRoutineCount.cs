using System.Linq;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static long GetRoutineCount(this NpgsqlConnection connection, Settings settings) => 
            connection.Read<long>(@$"

                select 
                    count(*)
                from
                    information_schema.routines r
                where
                    r.external_language <> 'INTERNAL'
                    and
                    (   @schema is null or (r.specific_schema similar to @schema)   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )

                    and (@notSimilarTo is null or r.routine_name not similar to @notSimilarTo)
                    and (@similarTo is null or r.routine_name similar to @similarTo)
            ",
                ("schema", settings.Schema, DbType.AnsiString),
                ("notSimilarTo", settings.NotSimilarTo, DbType.AnsiString),
                ("similarTo", settings.SimilarTo, DbType.AnsiString))
            .Single();
    }
}
