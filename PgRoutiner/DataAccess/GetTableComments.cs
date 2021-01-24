using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;
using Newtonsoft.Json;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static IEnumerable<TableComment> GetTableComments(this NpgsqlConnection connection, Settings settings, string schema, bool isTable = true) =>
            connection.Read<(string Table, string Column, string ConstraintMarkup, string ColumnType, string Nullable, string DefaultMarkup, string Comment)>(@"

                with table_constraints as (
                    
                    select 
                        sub.table_name,
                        sub.column_name,
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
                            case    when tc.constraint_type = 'PRIMARY KEY' 
                                    then '**PK**'
                                    when tc.constraint_type = 'FOREIGN KEY' 
                                    then '**FK [➝](#' || lower(ccu.table_schema || '-' || ccu.table_name || '-' || ccu.column_name) || ') `' ||
                                        case    when tc.constraint_schema = ccu.table_schema 
                                                then ''
                                                else ccu.table_schema || '.'
                                        end 
                                        || ccu.table_name || '.' || ccu.column_name || '`**'
                                else tc.constraint_type
                            end as description_markup

                        from
                            information_schema.table_constraints tc
                            inner join information_schema.constraint_column_usage ccu 
                            on tc.constraint_schema = ccu.constraint_schema and tc.constraint_name = ccu.constraint_name
                            left outer join information_schema.key_column_usage kcu 
                            on tc.constraint_type = 'FOREIGN KEY' and tc.constraint_schema = kcu.constraint_schema and ccu.constraint_name = kcu.constraint_name
                        where
                            tc.constraint_schema = @schema

                        union all 
                        
                        select 
                            i.relname as table_name, 
                            a.attname as column_name,
                            '**IDX**' as description_markup
                        from 
                            pg_stat_all_indexes i
                            inner join pg_attribute a on i.indexrelid = a.attrelid
                            left outer join information_schema.table_constraints tc on i.indexrelname = tc.constraint_name
                        where tc.table_name is null and i.schemaname = @schema

                        order by
                            description_markup

                    ) sub
                    group by 
                        sub.table_name, sub.column_name

                )
                select 
                    table_name_id as table_name,
                    c.column_name,
                    tc.description_markup,
                    c.data_type || 

                        case    when c.data_type <> 'integer' and c.data_type <> 'bigint' and c.data_type <> 'smallint' 
                                then
                                    case    when c.character_maximum_length is not null 
                                            then '(' || cast(c.character_maximum_length as varchar) || ')'
                                            else 
                                                case    when c.numeric_precision is not null 
                                                        then '(' || cast(c.numeric_precision as varchar) || ',' || cast(c.numeric_precision_radix as varchar) || ')' || 
                                                            case    when coalesce(c.numeric_scale, 0) = 0 
                                                                    then '' 
                                                                    else ',' || cast(c.numeric_scale as varchar) || ')'
                                                            end
                                                else ''
                                                end
                                    end
                                else ''
                        end as data_type,

                    case when c.is_nullable = 'NO' then '**NO**' else c.is_nullable end as nullableMarkup,
                    
                    case    when c.column_default like 'next%' or c.identity_generation = 'ALWAYS' 
                            then '*auto increment*' 
                            else '`' || c.column_default || '`' 
                    end as defaultMarkup,
                    
                    pgdesc.description
                    
                from (
                        select t1.table_name as table_name_id, t1.table_name
                        from information_schema.tables t1
                        where t1.table_schema = @schema and t1.table_type = @type
                        union all
                        select t2.table_name as table_name_id, null as table_name
                        from information_schema.tables t2
                        where t2.table_schema = @schema and t2.table_type = @type
                        order by table_name_id, table_name nulls first
                    ) t
                    
                    left outer join information_schema.columns c 
                    on t.table_name = c.table_name and c.table_schema = @schema 
                    
                    left outer join pg_catalog.pg_statio_user_tables pgtbl
                    on t.table_name_id = pgtbl.relname and pgtbl.schemaname = @schema
                    
                    left outer join pg_catalog.pg_description pgdesc
                    on pgtbl.relid = pgdesc.objoid and coalesce(c.ordinal_position, 0) = pgdesc.objsubid

                    left outer join table_constraints tc
                    on t.table_name = tc.table_name and c.column_name = tc.column_name
           
                where
                    (@notSimilarTo is null or table_name_id not similar to @notSimilarTo)
                    and (@similarTo is null or table_name_id similar to @similarTo)

                order by 
                    t.table_name_id, 
                    t.table_name nulls first, 
                    c.ordinal_position
            ",
                ("schema", schema, DbType.AnsiString),
                ("notSimilarTo", settings.CommentsMdNotSimilarTo, DbType.AnsiString),
                ("similarTo", settings.CommentsMdSimilarTo, DbType.AnsiString),
                ("type", isTable ? "BASE TABLE" : "VIEW", DbType.AnsiString))

            .Select(t => new TableComment
            {
                Column = t.Column,
                ColumnType = t.ColumnType,
                Comment = t.Comment,
                ConstraintMarkup = t.ConstraintMarkup,
                DefaultMarkup = t.DefaultMarkup,
                Nullable = t.Nullable,
                Table = t.Table
            });
    }
}
