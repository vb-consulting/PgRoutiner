using System.Data;
using Norm;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static long GetRoutineCount(this NpgsqlConnection connection, Settings settings, 
        string schemaSimilarTo = null, string schemaNotSimilarTo = null) =>
        connection.Read<long>(@$"

                select 
                    count(*)
                from
                    information_schema.routines r
                where
                    lower(r.external_language) = any('{{sql,plpgsql}}')
                    and
                    (   @schema is null or (r.specific_schema similar to @schema)   )
                    and (   @not_schema is null or r.specific_schema not similar to @not_schema   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )

                    and (@notSimilarTo is null or r.routine_name not similar to @notSimilarTo)
                    and (@similarTo is null or r.routine_name similar to @similarTo)
            ",
            new
            {
                schema = (schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString),
                not_schema = (schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString),
                notSimilarTo = (settings.RoutinesNotSimilarTo, DbType.AnsiString),
                similarTo = (settings.RoutinesSimilarTo, DbType.AnsiString)
            })
            .Single();
}
