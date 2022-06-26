using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetTables(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
    {
        return connection
            .WithParameters(
                (settings.SchemaSimilarTo, DbType.AnsiString),
                (settings.SchemaNotSimilarTo, DbType.AnsiString),
                (skipSimilar, DbType.AnsiString))
            .Read<(string Schema, string Name, string Type)>(@$"

        select 
            table_schema, table_name, table_type
        from 
            information_schema.tables
        where
            (   $1 is null or (table_schema similar to $1)   )
            and (   $2 is null or (table_schema not similar to $2)   )
            and (   {GetSchemaExpression("table_schema")}  )
            and (   $3 is null or (table_name not similar to $3)   )

        ")
        .Select(t => new PgItem
        {
            Schema = t.Schema,
            Name = t.Name,
            TypeName = t.Type,
            Type = t.Type switch
            {
                "BASE TABLE" => PgType.Table,
                "VIEW" => PgType.View,
                _ => PgType.Unknown
            }
        });
    }
}
