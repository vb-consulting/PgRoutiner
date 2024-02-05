using System.Data;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;


public static partial class DataAccessConnectionExtensions
{

    public static IEnumerable<PgItem> FilterTypes(this NpgsqlConnection connection, List<PgItem> types, Current settings, string skipSimilar = null)
    {
        if (!types.Any())
        {
            return Enumerable.Empty<PgItem>();
        }

        return connection.Read<(string Schema, string Name)>(
            [
                (settings.SchemaSimilarTo, DbType.AnsiString, null), 
                (settings.SchemaNotSimilarTo, DbType.AnsiString, null), 
                (skipSimilar, DbType.AnsiString, null)
            ],
            @$"

            select 
                schema, name
            from
            (
                {string.Join(" union all ", types.Select(t => $"select '{t.Schema}' as schema, '{t.Name}' as name"))}
            ) sub

            where
                (   $1 is null or (sub.schema similar to $1)   )
                and (   $2 is null or sub.schema not similar to $2   )
                and (   {GetSchemaExpression("sub.schema")}  )
                and (   $3 is null or (sub.name not similar to $3)   )
                
            ", r => (r.Val<string>(0), r.Val<string>(1)) 
        ).Select(t => new PgItem
        {
            Schema = t.Schema,
            Name = t.Name,
            TypeName = "TYPE",
            Type = PgType.Type
        });

        //return connection
        //    .WithParameters(
        //        (settings.SchemaSimilarTo, DbType.AnsiString), 
        //        (settings.SchemaNotSimilarTo, DbType.AnsiString), 
        //        (skipSimilar, DbType.AnsiString))
        //    .Read<(string Schema, string Name)>(@$"

        //        select 
        //            schema, name
        //        from
        //        (
        //            {string.Join(" union all ", types.Select(t => $"select '{t.Schema}' as schema, '{t.Name}' as name"))}
        //        ) sub

        //        where
        //            (   $1 is null or (sub.schema similar to $1)   )
        //            and (   $2 is null or sub.schema not similar to $2   )
        //            and (   {GetSchemaExpression("sub.schema")}  )
        //            and (   $3 is null or (sub.name not similar to $3)   )

        //        ")
        //        .Select(t => new PgItem
        //        {
        //            Schema = t.Schema,
        //            Name = t.Name,
        //            TypeName = "TYPE",
        //            Type = PgType.Type
        //        });
    }
}
