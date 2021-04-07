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
        public static void GetTableDefintions(this NpgsqlConnection connection, Settings settings)
        {
            connection.Read<(string Schema, string Name)>(@$"

            select 
                t.table_schema, 
                t.table_name, 
                c.column_name,
                c.ordinal_position,
                c.column_default,
                c.is_nullable,
                c.data_type,
                c.udt_name,
                c.is_identity,
                c.identity_generation,
                c.is_generated,
                c.is_updatable,
                tc.constraint_type
            from 
                information_schema.tables t
                inner join information_schema.columns c 
                on t.table_name = c.table_name and t.table_schema = c.table_schema
    
                left outer join information_schema.key_column_usage kcu 
                on t.table_name = kcu.table_name and t.table_schema = kcu.table_schema and c.column_name = kcu.column_name
    
                left outer join information_schema.table_constraints tc
                on t.table_name = tc.table_name and t.table_schema = tc.table_schema and tc.constraint_name = kcu.constraint_name
            where 
                table_type = 'BASE TABLE'
                and
                (
                    (   @schema is not null and t.table_schema similar to @schema   )
                    or
                    (   {GetSchemaExpression("t.table_schema")}  )
                )
            order by 
                t.table_schema, 
                t.table_name, 
                c.ordinal_position

            ", ("schema", settings.Schema, DbType.AnsiString))
            .Select(t => new PgItem
            {
                Schema = t.Schema,
                Name = t.Name,
                Type = PgType.Sequence
            });
        }
    }
}
