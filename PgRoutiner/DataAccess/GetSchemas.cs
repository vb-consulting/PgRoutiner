using System.Data;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<string> GetSchemas(this NpgsqlConnection connection, 
        Current settings, string skipSimilar = null, string schemaSimilarTo = null, string schemaNotSimilarTo = null)
    {
        return connection.Read<string>([
            (schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString, null),
            (schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString, null),
            (skipSimilar, DbType.AnsiString, null)], @$"
            select
                schema_name
            from
                information_schema.schemata
            where
                (   $1 is null or (schema_name similar to $1)   )
                and (   $2 is null or schema_name not similar to $2   )
                and (   {GetSchemaExpression("schema_name")}  )
                and (   $3 is null or (schema_name not similar to $3)   )", r => r.Val<string>(0));
        /*
        return connection
            .WithParameters(
                (schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString),
                (schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString),
                (skipSimilar, DbType.AnsiString))
            .Read<string>(@$"

            select
                schema_name
            from
                information_schema.schemata
            where
                (   $1 is null or (schema_name similar to $1)   )
                and (   $2 is null or schema_name not similar to $2   )
                and (   {GetSchemaExpression("schema_name")}  )
                and (   $3 is null or (schema_name not similar to $3)   )
        ");
        */
    }
}
