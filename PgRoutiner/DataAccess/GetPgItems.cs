using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetPgItems(
        this NpgsqlConnection connection, 
        string exp,
        List<PgItem> types)
    {
        var split = exp.Split('.');
        string schema = null;
        string name = null;
        if (split.Length == 1)
        {
            name = split[0];
        }
        else
        {
            schema = split[0];
            name = split[1];
        }

        return connection
            .WithParameters(
                (schema == "*" ? null : schema, DbType.AnsiString), 
                (name == "*" ? null : name, DbType.AnsiString))
            .Read<(string Schema, string Name, string Type)>(@$"

            -- tables
            select 
                t.table_schema as schema, 
                t.table_name as name, 
                t.table_type as type
            from 
                information_schema.tables t
            where
                (   $1 is null or t.table_schema = $1   ) and
                (   $2 is null or t.table_name = $2   ) and
                (   {GetSchemaExpression("t.table_schema")}  ) and
                not exists (

                    select
                        1
                    from 
                        pg_inherits
                        inner join pg_class parent on pg_inherits.inhparent = parent.oid
                        inner join pg_class child on pg_inherits.inhrelid = child.oid
                        inner join pg_namespace nmsp_parent on nmsp_parent.oid = parent.relnamespace
                        inner join pg_namespace nmsp_child on nmsp_child.oid = child.relnamespace
                    where
                         nmsp_child.nspname = t.table_schema and child.relname = t.table_name
                )

            union all

            -- function and procedures
            select 
                distinct
                r.routine_schema as schema, 
                r.routine_name as name, 
                r.routine_type as type
            from
                information_schema.routines r
            where
                (   $1 is null or r.routine_schema = $1   ) and
                (   $2 is null or r.routine_name = $2   ) and
                (   {GetSchemaExpression("r.routine_schema")}  )

            union all

            -- domains
            select 
                distinct
                d.domain_schema as schema, 
                d.domain_name  as name,
                'DOMAIN' as type
            from
                information_schema.domains d
            where
                (   $1 is null or d.domain_schema = $1   ) and
                (   $2 is null or d.domain_name = $2   ) and
                (   {GetSchemaExpression("d.domain_schema")}  )

            union all

            -- types
            select 
                sub.schema as schema, 
                sub.name as name, 
                'TYPE' as type
            from
            (
                {string.Join(" union all ", types.Select(t => $"select '{t.Schema}' as schema, '{t.Name}' as name"))}
            ) sub
            where
                (   $1 is null or sub.schema = $1   ) and
                (   $2 is null or sub.name = $2   ) and
                (   {GetSchemaExpression("sub.schema")}  )

            union all

            select
                sc.schema_name as schema, 
                null as name,
                'SCHEMA' as type
            from
                information_schema.schemata sc
            where
                (  ($1 is null and $2 is null) or sc.schema_name = $1 or sc.schema_name = $2 )

            union all

            select 
                null as schema, 
                e.extname as name,
                'EXTENSION' as type
            from pg_extension e
            where
                (   ($1 is null and $2 is null) or e.extname = $1 or e.extname = $2 )
            ")
            .Select(t => new PgItem
            {
                Schema = t.Schema,
                Name = t.Name,
                Type = t.Type switch 
                {
                    "BASE TABLE" => PgType.Table,
                    "VIEW" => PgType.View,
                    "FUNCTION" => PgType.Function,
                    "DOMAIN" => PgType.Domain,
                    "TYPE" => PgType.Type,
                    "SCHEMA" => PgType.Schema,
                    "EXTENSION" => PgType.Extension,
                    _ => PgType.Unknown
                },
                TypeName = t.Type.ToUpperInvariant(),
            });
}
}
