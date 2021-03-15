using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static IEnumerable<PgItem> FilterTypes(this NpgsqlConnection connection, List<PgItem> types, Settings settings)
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
                    (
                        (   @schema is not null and sub.schema similar to @schema   )
                        or
                        (   {GetSchemaExpression("sub.schema")}  )
                    )
                
                ", ("schema", settings.Schema, DbType.AnsiString))
                .Select(t => new PgItem
                {
                    Schema = t.Schema,
                    Name = t.Name,
                    Type = PgType.Type
                });
        }
    }
}
