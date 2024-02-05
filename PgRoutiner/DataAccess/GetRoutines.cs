using System.Data;
using NpgsqlTypes;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetRoutines(this NpgsqlConnection connection, Current settings)
    {
        return connection.Read<(string Schema, string Name, string Type)>(
                       [
                (settings.SchemaSimilarTo, DbType.AnsiString, null),
                (settings.SchemaNotSimilarTo, DbType.AnsiString, null),
                (settings.RoutinesLanguages.ToList(), null, NpgsqlDbType.Array | NpgsqlDbType.Text)
            ],@$"
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

            ", r => (r.Val<string>(0), r.Val<string>(1), r.Val<string>(2))
                   )
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
        /*
        return connection
            .WithParameters(
                (settings.SchemaSimilarTo, DbType.AnsiString),
                (settings.SchemaNotSimilarTo, DbType.AnsiString),
                (settings.RoutinesLanguages.ToList(), NpgsqlDbType.Array | NpgsqlDbType.Text))
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
        */
    }
}
