using System.Data;
using Norm;
using NpgsqlTypes;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetRoutines(this NpgsqlConnection connection, Settings settings)
    {
        return connection
            .WithParameters(
                (settings.SchemaSimilarTo, DbType.AnsiString),
                (settings.SchemaNotSimilarTo, DbType.AnsiString),
                (settings.RoutinesLanguages, NpgsqlDbType.Array | NpgsqlDbType.Text))
            .Read<(string Schema, string Name, string Type)>(@$"

                select 
                    distinct
                    r.routine_schema,
                    r.routine_name,
                    r.routine_type
                from
                    information_schema.routines r
                where
                    lower(r.external_language) = any($3)
                    and
                    (   $1 is null or (r.specific_schema similar to $1)   )
                    and (   $2 is null or r.specific_schema not similar to $2   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )


            ")
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
