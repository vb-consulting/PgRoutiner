using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<IGrouping<(string Schema, string Name), PgColumnGroup>>
        GetTableDefintions(this NpgsqlConnection connection, Current settings, string tableExpr)
    {
        return connection
            .WithParameters(
                (settings.SchemaSimilarTo, DbType.AnsiString),
                (settings.SchemaNotSimilarTo, DbType.AnsiString),
                (tableExpr, DbType.AnsiString))
            .Read<(
                string Schema,
                string Table,
                string Name,
                int Ord,
                bool IsGeneration,
                string Default,
                string IsNullable,
                string DataType,
                string TypeUdtName,
                string IsIdentity,
                string[] ConstraintTypes)>(@$"

            select 
                t.table_schema, 
                t.table_name, 
                c.column_name,
                c.ordinal_position,

                c.generation_expression is not null as is_generation_expression,
                c.column_default as column_default,

                c.is_nullable,
                c.data_type,
                --c.udt_name,
                regexp_replace(c.udt_name, '^[_]', '') as udt_name,
                c.is_identity,
                array_agg(tc.constraint_type) as constraint_types
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
                and (t.table_name similar to $3 or t.table_schema || '.' || t.table_name similar to $3)
                and
                (   $1 is null or (t.table_schema similar to $1)   )
                and (   $2 is null or (t.table_schema not similar to $2)   )
                and (   {GetSchemaExpression("t.table_schema")}  )
            group by 
                t.table_schema, 
                t.table_name, 
                c.column_name,
                c.ordinal_position,
                c.identity_generation,
                c.is_generated,
                c.generation_expression,
                c.column_default,
                c.is_nullable,
                c.data_type,
                c.udt_name,
                c.is_identity
            order by 
                t.table_schema, 
                t.table_name, 
                c.ordinal_position

            ")
            .Select(t => new PgColumnGroup
            {
                Schema = t.Schema,
                Table = t.Table,
                Name = t.Name,
                Ordinal = t.Ord,
                IsGeneration = t.IsGeneration,
                Default = t.Default,
                HasDefault = !string.IsNullOrEmpty(t.Default),
                IsNullable = string.Equals(t.IsNullable, "YES"),
                DataType = t.DataType,
                Type = t.TypeUdtName,
                IsArray = t.DataType == "ARRAY",//t.ConstraintTypes.Contains("ARRAY"),
                IsIdentity = string.Equals(t.IsIdentity, "YES"),
                IsPk = t.ConstraintTypes.Contains("PRIMARY KEY"),
            })
            .GroupBy(i => (i.Schema, i.Table));
    }
}
