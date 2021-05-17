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
        public static IEnumerable<PgItem> GetDomains(this NpgsqlConnection connection, Settings settings) =>
            connection.Read<(string Schema, string Name)>(@$"

                select 
                    distinct
                    d.domain_schema,
                    d.domain_name
                from
                    information_schema.domains d
                where
                    (   @schema is null or (d.domain_schema similar to @schema)   )
                    and (   {GetSchemaExpression("d.domain_schema")}  )

            ", ("schema", settings.Schema, DbType.AnsiString))
            .Select(t => new PgItem
            {
                Schema = t.Schema,
                Name = t.Name,
                Type = PgType.Domain
            });
    }
}
