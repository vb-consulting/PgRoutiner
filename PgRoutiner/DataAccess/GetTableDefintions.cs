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
        public static IEnumerable<IGrouping<(string Schema, string Name), PgColumnGroup>> 
            GetTableDefintions(this NpgsqlConnection connection, Settings settings)
        {
            return connection.Read<(
                string Schema, 
                string Table, 
                string Name, 
                int Ord, 
                string Default, 
                string IsNullable, 
                string DataType, 
                string TypeUdtName,
                string IsIdentity,
                string ConstraintType)>(@$"

            select 
                t.table_schema, 
                t.table_name, 
                c.column_name,
                c.ordinal_position,
                c.column_default,
                c.is_nullable,
                c.data_type,
                regexp_replace(c.udt_name, '^[_]', '') as udt_name,
                c.is_identity,
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
            .Select(t => new PgColumnGroup
            {
                Schema = t.Schema,
                Table = t.Table,
                Name = t.Name,
                Ord = t.Ord,
                Default = t.Default,
                IsNullable = string.Equals(t.IsNullable, "YES"),
                DataType = t.DataType,
                TypeUdtName = t.TypeUdtName,
                IsIdentity = string.Equals(t.IsIdentity, "YES"),
                IsPk = string.Equals(t.ConstraintType, "PRIMARY KEY"),
            })
            .GroupBy(i => (i.Schema, i.Table));
        }
    }
}
