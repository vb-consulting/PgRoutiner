using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public enum PgType { Table, View, Unknown }
    public record PgTable(string Schema, string Name, PgType Type);

    public static partial class DataAccess
    {
        public static IEnumerable<PgTable> GetTables(this NpgsqlConnection connection, Settings settings) => 
            connection.Read<(string Schema, string Name, string Type)>(@"

            select 
                table_schema, table_name, table_type
            from 
                information_schema.tables
            where
                (   @schema is not null and table_schema similar to @schema   )
                or
                (   table_schema not like 'pg_%' and table_schema <> 'information_schema' )

            ", ("schema", settings.Schema, DbType.AnsiString))
            .Select(t => new PgTable(t.Schema, t.Name, t.Type switch { "BASE TABLE" => PgType.Table, "VIEW" => PgType.View, _ => PgType.Unknown }  ));
    }
}
