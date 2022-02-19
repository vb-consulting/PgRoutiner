using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetDomains(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
    {
        return connection.Read<(string Schema, string Name)>(@$"

                select 
                    distinct
                    d.domain_schema,
                    d.domain_name
                from
                    information_schema.domains d
                where
                    (   @schema is null or (d.domain_schema similar to @schema)   )
                    and (   @not_schema is null or d.domain_schema not similar to @not_schema   )
                    and (   {GetSchemaExpression("d.domain_schema")}  )

                    and (   @skipSimilar is null or (d.domain_name not similar to @skipSimilar)   )

            ", 
            new
            {
                schema = (settings.SchemaSimilarTo, DbType.AnsiString),
                not_schema = (settings.SchemaNotSimilarTo, DbType.AnsiString),
                skipSimilar = (skipSimilar, DbType.AnsiString)
            })
            .Select(t => new PgItem
            {
                Schema = t.Schema,
                Name = t.Name,
                Type = PgType.Domain
            });
    }
}
