using System.Data;
using Norm;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<string> GetSchemas(this NpgsqlConnection connection, 
        Settings settings, string skipSimilar = null, string schemaSimilarTo = null, string schemaNotSimilarTo = null)
    {
        return connection.Read<string>(@$"

                select
                    schema_name
                from
                    information_schema.schemata
                where
                    (   @schema is null or (schema_name similar to @schema)   )
                    and (   @not_schema is null or schema_name not similar to @not_schema   )
                    and (   {GetSchemaExpression("schema_name")}  )
                    and (   @skipSimilar is null or (schema_name not similar to @skipSimilar)   )
            ",
        ("schema", schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString),
        ("not_schema", schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString),
        ("skipSimilar", skipSimilar, DbType.AnsiString));
    }
}
