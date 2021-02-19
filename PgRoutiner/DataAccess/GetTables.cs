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
        public static IEnumerable<PgItem> GetTables(this NpgsqlConnection connection, Settings settings) => 
            connection.Read<(string Schema, string Name, string Type)>(@$"

            select 
                table_schema, table_name, table_type
            from 
                information_schema.tables
            where
                (   @schema is not null and table_schema similar to @schema   )
                or
                (   {GetSchemaExpression("table_schema")}  )

            ", ("schema", settings.Schema, DbType.AnsiString))
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
