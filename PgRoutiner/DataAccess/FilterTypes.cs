using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> FilterTypes(this NpgsqlConnection connection, List<PgItem> types, Settings settings, string skipSimilar = null)
    {
        if (!types.Any())
        {
            return Enumerable.Empty<PgItem>();
        }
        return connection.Read<(string Schema, string Name)>(@$"

                select 
                    schema, name
                from
                (
                    {string.Join(" union all ", types.Select(t => $"select '{t.Schema}' as schema, '{t.Name}' as name"))}
                ) sub

                where
                    (   @schema is null or (sub.schema similar to @schema)   )
                    and (   @not_schema is null or sub.schema not similar to @not_schema   )
                    and (   {GetSchemaExpression("sub.schema")}  )

                    and (   @skipSimilar is null or (sub.name not similar to @skipSimilar)   )
                
                ", 
                ("schema", settings.SchemaSimilarTo, DbType.AnsiString),
                ("not_schema", settings.SchemaNotSimilarTo, DbType.AnsiString),
                ("skipSimilar", skipSimilar, DbType.AnsiString)).Select(t => new PgItem
        {
            Schema = t.Schema,
            Name = t.Name,
            Type = PgType.Type
        });
    }
}
