using System.Data;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetDomains(this NpgsqlConnection connection, Current settings, string skipSimilar = null)
    {
        return connection.Read<(string Schema, string Name)>(
        [
            (settings.SchemaSimilarTo, DbType.AnsiString, null),
            (settings.SchemaNotSimilarTo, DbType.AnsiString, null),
            (skipSimilar, DbType.AnsiString, null)
        ],
        @$"
        select 
            distinct
            d.domain_schema,
            d.domain_name
        from
            information_schema.domains d
        where
            (   $1 is null or (d.domain_schema similar to $1)   )
            and (   $2 is null or d.domain_schema not similar to $2   )
            and (   {GetSchemaExpression("d.domain_schema")}  )

            and (   $3 is null or (d.domain_name not similar to $3)   )
                ", r => (r.Val<string>(0), r.Val<string>(1))
        )
        .Select(t => new PgItem
        {
            Schema = t.Schema,
            Name = t.Name,
            TypeName = "DOMAIN",
            Type = PgType.Domain
        });
   
        //return connection
        //    .WithParameters(
        //        (settings.SchemaSimilarTo, DbType.AnsiString),
        //        (settings.SchemaNotSimilarTo, DbType.AnsiString),
        //        (skipSimilar, DbType.AnsiString))
        //    .Read<(string Schema, string Name)>(@$"

        //        select 
        //            distinct
        //            d.domain_schema,
        //            d.domain_name
        //        from
        //            information_schema.domains d
        //        where
        //            (   $1 is null or (d.domain_schema similar to $1)   )
        //            and (   $2 is null or d.domain_schema not similar to $2   )
        //            and (   {GetSchemaExpression("d.domain_schema")}  )

        //            and (   $3 is null or (d.domain_name not similar to $3)   )

        //    ")
        //    .Select(t => new PgItem
        //    {
        //        Schema = t.Schema,
        //        Name = t.Name,
        //        TypeName = "DOMAIN",
        //        Type = PgType.Domain
        //    });
    }
}
