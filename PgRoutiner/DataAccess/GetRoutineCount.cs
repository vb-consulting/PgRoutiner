using System.Data;
using Norm;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static long GetRoutineCount(this NpgsqlConnection connection, Settings settings, 
        string schemaSimilarTo = null, string schemaNotSimilarTo = null) => connection
        .WithParameters(
            (schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString),
            (schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString),
            (settings.RoutinesNotSimilarTo, DbType.AnsiString),
            (settings.RoutinesSimilarTo, DbType.AnsiString))
        .Read<long>(@$"

                select 
                    count(*)
                from
                    information_schema.routines r
                where
                    lower(r.external_language) = any('{{sql,plpgsql}}')
                    and
                    (   $1 is null or (r.specific_schema similar to $1)   )
                    and (   $2 is null or r.specific_schema not similar to $2   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )

                    and ($3 is null or r.routine_name not similar to $3)
                    and ($4 is null or r.routine_name similar to $4)
            ")
            .Single();
}
