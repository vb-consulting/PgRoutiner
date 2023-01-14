using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<TableComment> GetTableComments(this NpgsqlConnection connection, 
            Current settings, 
            string schema) =>
        connection
        .WithParameters(
            (schema, DbType.AnsiString),
            (settings.MdNotSimilarTo, DbType.AnsiString),
            (settings.MdSimilarTo, DbType.AnsiString))
        .Read<(string Table, bool HasPartitions, string Column, bool? IsPk, string ConstraintMarkup, string ColumnType, bool? IsUdt, string Nullable, string DefaultMarkup, string Comment)>(@"

            with partitioned as (
    
                select 
                    nmsp_parent.nspname as parent_schema_name,
                    parent.relname as parent_table_name,
                    nmsp_child.nspname as child_schema_name,
                    child.relname as child_table_name
                from pg_inherits
                    inner join pg_class parent on pg_inherits.inhparent = parent.oid
                    inner join pg_class child on pg_inherits.inhrelid = child.oid
                    inner join pg_namespace nmsp_parent on nmsp_parent.oid = parent.relnamespace
                    inner join pg_namespace nmsp_child on nmsp_child.oid = child.relnamespace
                    inner join information_schema.tables t on parent.relname = t.table_name and nmsp_parent.nspname = $1
    
            ),
            table_constraints as (

                select 
                    sub.table_name,
                    sub.column_name,
                    sub.is_pk,
                    string_agg(
                        sub.description_markup, 
                        ', ' 
                        order by 
                            case 
                                when sub.description_markup = '**PK**' then ' ' 
                                else sub.description_markup 
                            end
                    ) as description_markup
                from (

                    select
                        csub.table_name,
                        csub.column_name,
                        max(csub.is_pk::int)::bool as is_pk,
                        string_agg(csub.description_markup, ', ' order by csub.is_pk desc) as description_markup
                    from (
                        select 
                            tc.table_name, 
                            coalesce(kcu.column_name, ccu.column_name) as column_name,
                            tc.constraint_type = 'PRIMARY KEY' as is_pk,
                            case    when tc.constraint_type = 'PRIMARY KEY' 
                                    then '**PK**'
                                    when tc.constraint_type = 'FOREIGN KEY' 
                                    then '**FK [➝](#' || lower(ccu.table_schema || '-' || ccu.table_name || '-' || ccu.column_name) || ') `' ||
                                        case    when tc.constraint_schema = ccu.table_schema 
                                                then ''
                                                else ccu.table_schema || '.'
                                        end 
                                        || case when ccu.table_schema = 'public' then '' else  ccu.table_schema || '.' end || ccu.table_name || '.' || ccu.column_name || '`**'
                                    when tc.constraint_type = 'CHECK' then (select '`' || pg_get_constraintdef((select oid from pg_constraint where conname = tc.constraint_name), true) || '`')
                                    else tc.constraint_type
                            end as description_markup

                        from
                            information_schema.table_constraints tc
                            inner join information_schema.constraint_column_usage ccu 
                            on tc.constraint_schema = ccu.constraint_schema and tc.constraint_name = ccu.constraint_name
                            left outer join information_schema.key_column_usage kcu 
                            on tc.constraint_type = 'FOREIGN KEY' and tc.constraint_schema = kcu.constraint_schema and ccu.constraint_name = kcu.constraint_name
                        where
                            tc.constraint_schema = $1
                            and tc.table_name = 'table_configs'
                        group by
                            tc.table_name, kcu.column_name, ccu.column_name, tc.constraint_name,
                            tc.constraint_type, ccu.table_schema, ccu.table_name, tc.constraint_schema
                    ) csub
                    group by
                        csub.table_name,
                        csub.column_name

                    union all 
                        
                    select 
                        i.relname as table_name, 
                        a.attname as column_name,
                        false as is_pk,
                        '**IDX**' as description_markup
                    from 
                        pg_stat_all_indexes i
                        inner join pg_attribute a on i.indexrelid = a.attrelid
                        left outer join information_schema.table_constraints tc on i.indexrelname = tc.constraint_name
                    where tc.table_name is null and i.schemaname = $1

                    order by
                        description_markup

                ) sub
                group by 
                    sub.table_name, sub.column_name, sub.is_pk

            )
            select 
                table_name_id as table_name,
                (
                    select exists (select 1 from partitioned where partitioned.parent_table_name = table_name_id)
                ) as has_partitions,
                c.column_name,
                tc.is_pk,
                tc.description_markup,

                case    when c.data_type = 'USER-DEFINED' 
                        then c.udt_name 
                        else case   when c.udt_schema <> 'pg_catalog' 
                                    then c.udt_schema || '.' 
                                    else '' 
                        end || c.udt_name::regtype 
                end || 
                case    when c.data_type <> 'integer' and c.data_type <> 'bigint' and c.data_type <> 'smallint' 
                        then
                            case    when c.character_maximum_length is not null 
                                    then '(' || cast(c.character_maximum_length as varchar) || ')'
                                    else 
                                        case    when c.numeric_precision is not null 
                                                then '(' || cast(c.numeric_precision as varchar) || ',' || cast(c.numeric_scale as varchar) || ')'
                                        else ''
                                        end
                            end
                        else ''
                end as data_type,

                c.data_type = 'USER-DEFINED' as is_udt,

                case when c.is_nullable = 'NO' then '**NO**' else c.is_nullable end as nullableMarkup,

                '`' || case
                    when c.identity_generation is not null then 'GENERATED ' || c.identity_generation || ' AS IDENTITY'
                    when c.is_generated <> 'NEVER' then 'GENERATED ' || c.is_generated || ' AS ' || c.generation_expression
                    else c.column_default
                    end
                || '`' as defaultMarkup,

                pgdesc.description
                    
            from (
                    select t1.table_name as table_name_id, t1.table_name
                    from information_schema.tables t1
                    where t1.table_schema = $1 and t1.table_type = 'BASE TABLE'
                    union all
                    select t2.table_name as table_name_id, null as table_name
                    from information_schema.tables t2
                    where t2.table_schema = $1 and t2.table_type = 'BASE TABLE'
                    order by table_name_id, table_name nulls first
                ) t
                    
                left outer join information_schema.columns c 
                on t.table_name = c.table_name and c.table_schema = $1 
                    
                left outer join pg_catalog.pg_stat_all_tables pgtbl
                on t.table_name_id = pgtbl.relname and pgtbl.schemaname = $1
                    
                left outer join pg_catalog.pg_description pgdesc
                on pgtbl.relid = pgdesc.objoid and coalesce(c.ordinal_position, 0) = pgdesc.objsubid

                left outer join table_constraints tc
                on t.table_name = tc.table_name and c.column_name = tc.column_name

                left outer join partitioned part
                on table_name_id = part.child_table_name and pgtbl.schemaname = part.child_schema_name

            where
                part.child_table_name is null
                and ($2 is null or table_name_id not similar to $2)
                and ($3 is null or table_name_id similar to $3)

            order by 
                t.table_name_id, 
                t.table_name nulls first, 
                c.ordinal_position
        ")
        .Select(t => new TableComment
        {
            Column = t.Column,
            IsPk = t.IsPk,
            ColumnType = t.ColumnType,
            IsUdt = t.IsUdt,
            Comment = t.Comment,
            ConstraintMarkup = t.ConstraintMarkup,
            DefaultMarkup = t.DefaultMarkup,
            Nullable = t.Nullable,
            Table = t.Table,
            HasPartitions = t.HasPartitions,
        });

    public static IEnumerable<TableComment> GetViewComments(this NpgsqlConnection connection,
        Current settings,
        string schema) =>
    connection
    .WithParameters(
        (schema, DbType.AnsiString),
        (settings.MdNotSimilarTo, DbType.AnsiString),
        (settings.MdSimilarTo, DbType.AnsiString))
    .Read<(string Table, bool HasPartitions, string Column, bool? IsPk, string ConstraintMarkup, string ColumnType, bool? IsUdt, string Nullable, string DefaultMarkup, string Comment)>(@"

            with table_constraints as (
                    
                select 
                    sub.table_name,
                    sub.column_name,
                    sub.is_pk,
                    string_agg(
                        sub.description_markup, 
                        ', ' 
                        order by 
                            case 
                                when sub.description_markup = '**PK**' then ' ' 
                                else sub.description_markup 
                            end
                    ) as description_markup
                from (
                    select 
                        tc.table_name, 
                        coalesce(kcu.column_name, ccu.column_name) as column_name,
                        tc.constraint_type = 'PRIMARY KEY' as is_pk,
                        case    when tc.constraint_type = 'PRIMARY KEY' 
                                then '**PK**'
                                when tc.constraint_type = 'FOREIGN KEY' 
                                then '**FK [➝](#' || lower(ccu.table_schema || '-' || ccu.table_name || '-' || ccu.column_name) || ') `' ||
                                    case    when tc.constraint_schema = ccu.table_schema 
                                            then ''
                                            else ccu.table_schema || '.'
                                    end 
                                    || ccu.table_name || '.' || ccu.column_name || '`**'
                                when tc.constraint_type = 'CHECK' then (select '`' || pg_get_constraintdef((select oid from pg_constraint where conname = tc.constraint_name), true) || '`')
                                else tc.constraint_type
                        end as description_markup

                    from
                        information_schema.table_constraints tc
                        inner join information_schema.constraint_column_usage ccu 
                        on tc.constraint_schema = ccu.constraint_schema and tc.constraint_name = ccu.constraint_name
                        left outer join information_schema.key_column_usage kcu 
                        on tc.constraint_type = 'FOREIGN KEY' and tc.constraint_schema = kcu.constraint_schema and ccu.constraint_name = kcu.constraint_name
                    where
                        tc.constraint_schema = $1
                    group by
                        tc.table_name, kcu.column_name, ccu.column_name, tc.constraint_name,
                        tc.constraint_type, ccu.table_schema, ccu.table_name, tc.constraint_schema

                    union all 
                        
                    select 
                        i.relname as table_name, 
                        a.attname as column_name,
                        false as is_pk,
                        '**IDX**' as description_markup
                    from 
                        pg_stat_all_indexes i
                        inner join pg_attribute a on i.indexrelid = a.attrelid
                        left outer join information_schema.table_constraints tc on i.indexrelname = tc.constraint_name
                    where tc.table_name is null and i.schemaname = $1

                    order by
                        description_markup

                ) sub
                group by 
                    sub.table_name, sub.column_name, sub.is_pk

            )
            select 
                table_name_id as table_name,
                false as has_partitions,
                c.column_name,
                tc.is_pk,
                tc.description_markup,

                case    when c.data_type = 'USER-DEFINED' 
                        then c.udt_name 
                        else case   when c.udt_schema <> 'pg_catalog' 
                                    then c.udt_schema || '.' 
                                    else '' 
                        end || c.udt_name::regtype 
                end || 
                case    when c.data_type <> 'integer' and c.data_type <> 'bigint' and c.data_type <> 'smallint' 
                        then
                            case    when c.character_maximum_length is not null 
                                    then '(' || cast(c.character_maximum_length as varchar) || ')'
                                    else 
                                        case    when c.numeric_precision is not null 
                                                then '(' || cast(c.numeric_precision as varchar) || ',' || cast(c.numeric_scale as varchar) || ')'
                                        else ''
                                        end
                            end
                        else ''
                end as data_type,

                c.data_type = 'USER-DEFINED' as is_udt,
                
                case when c.is_nullable = 'NO' then '**NO**' else c.is_nullable end as nullableMarkup,

                '`' || case
                    when c.identity_generation is not null then 'GENERATED ' || c.identity_generation || ' AS IDENTITY'
                    when c.is_generated <> 'NEVER' then 'GENERATED ' || c.is_generated || ' AS ' || c.generation_expression
                    else c.column_default
                    end
                || '`' as defaultMarkup,

                pgdesc.description
                    
            from (
                    select t1.table_name as table_name_id, t1.table_name
                    from information_schema.tables t1
                    where t1.table_schema = $1 and t1.table_type = 'VIEW'
                    union all
                    select t2.table_name as table_name_id, null as table_name
                    from information_schema.tables t2
                    where t2.table_schema = $1 and t2.table_type = 'VIEW'
                    order by table_name_id, table_name nulls first
                ) t
                    
                left outer join information_schema.columns c 
                on t.table_name = c.table_name and c.table_schema = $1 
                    
                left outer join pg_catalog.pg_statio_user_tables pgtbl
                on t.table_name_id = pgtbl.relname and pgtbl.schemaname = $1
                    
                left outer join pg_catalog.pg_description pgdesc
                on pgtbl.relid = pgdesc.objoid and coalesce(c.ordinal_position, 0) = pgdesc.objsubid

                left outer join table_constraints tc
                on t.table_name = tc.table_name and c.column_name = tc.column_name
           
            where
                ($2 is null or table_name_id not similar to $2)
                and ($3 is null or table_name_id similar to $3)

            order by 
                t.table_name_id, 
                t.table_name nulls first, 
                c.ordinal_position
        ")
    .Select(t => new TableComment
    {
        Column = t.Column,
        IsPk = t.IsPk,
        ColumnType = t.ColumnType,
        IsUdt = t.IsUdt,
        Comment = t.Comment,
        ConstraintMarkup = t.ConstraintMarkup,
        DefaultMarkup = t.DefaultMarkup,
        Nullable = t.Nullable,
        Table = t.Table,
        HasPartitions = t.HasPartitions,
    });

}
