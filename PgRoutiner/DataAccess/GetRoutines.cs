using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetRoutines(this NpgsqlConnection connection, Settings settings)
    {
        return connection.Read<(string Schema, string Name, string Type)>(@$"

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
                    (   @schema is null or (r.specific_schema similar to @schema)   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )

            ", ("schema", settings.Schema, DbType.AnsiString)).Select(t => new PgItem
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
