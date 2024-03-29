﻿using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using NpgsqlTypes;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

[JsonSerializable(typeof(PgParameter[]))]
internal partial class PgParameterSerializerContext : JsonSerializerContext;

public static partial class DataAccessConnectionExtensions
{
    private static PgParameterSerializerContext jsonCtx = new(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    public static IEnumerable<IGrouping<(string Schema, string Name), PgRoutineGroup>> GetRoutineGroups(
        this NpgsqlConnection connection, Current settings, bool all = true, string skipSimilar = null, 
        string schemaSimilarTo = null, string schemaNotSimilarTo = null)
    {
        return connection.Read<(
                uint Oid,
                string SpecificSchema,
                string SpecificName,
                string RoutineName,
                string RoutineDefinition,
                string FullRoutineDefinition,
                string Description,
                string Language,
                string RoutineType,
                string TypeUdtName,
                bool IsSet,
                string DataType,
                string Parameters)>([
                    (schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString, null),
                    (schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString, null),
                    (settings.RoutinesNotSimilarTo, DbType.AnsiString, null),
                    (settings.RoutinesSimilarTo, DbType.AnsiString, null),
                    (all, DbType.Boolean, null),
                    (skipSimilar, DbType.AnsiString, null),
                    (settings.RoutinesLanguages.ToList(), null, NpgsqlDbType.Array | NpgsqlDbType.Text)
                    ], @$"

                select 
                    proc.oid,
                    r.specific_schema,
                    r.specific_name,
                    r.routine_name,
                    
                    r.routine_definition,
                    pg_get_functiondef(proc.oid) as full_routine_definition,
                    
                    pgdesc.description,
                    lower(r.external_language) as language,
                    lower(r.routine_type) as routine_type,

                    regexp_replace(r.type_udt_name, '^[_]', '') as type_udt_name,
                    proc.proretset,

                    case 
                        when array_length((array_agg(p.ordinal_position) filter (where p.ordinal_position is not null and p.parameter_mode = 'OUT')), 1) > 0
                        then 'record'
                        else r.data_type
                    end as data_type,

                    coalesce (
                        json_agg(
                            json_build_object(
                                'ordinal', p.ordinal_position,
                                'name', p.parameter_name,
                                'type', regexp_replace(p.udt_name, '^[_]', ''),
                                'dataType', p.data_type,
                                'isArray', p.data_type = 'ARRAY',
                                'default', p.parameter_default,
                                'dataTypeFormatted', case    when p.data_type = 'USER-DEFINED' 
                                                                then p.udt_name 
                                                                else case   when p.udt_schema <> 'pg_catalog' 
                                                                            then p.udt_schema || '.' 
                                                                            else '' 
                                                                end || p.udt_name::regtype 
                                                        end || 
                                                        case    when p.data_type <> 'integer' and p.data_type <> 'bigint' and p.data_type <> 'smallint' 
                                                                then
                                                                    case    when p.character_maximum_length is not null 
                                                                            then '(' || cast(p.character_maximum_length as varchar) || ')'
                                                                            else 
                                                                                case    when p.numeric_precision is not null 
                                                                                        then '(' || cast(p.numeric_precision as varchar) || ',' || cast(p.numeric_scale as varchar) || ')'
                                                                                else ''
                                                                                end
                                                                    end
                                                                else ''
                                                        end
                            ) 
                            order by 
                                p.ordinal_position
                        ) 
                        filter (
                            where 
                                p.ordinal_position is not null 
                                and (p.parameter_mode = 'IN' or  p.parameter_mode = 'INOUT')
                        ),
                        '[]'
                    ) as parameters

                from
                    information_schema.routines r
                    inner join pg_catalog.pg_proc proc on r.specific_name = proc.proname || '_' || proc.oid
                    left outer join pg_catalog.pg_description pgdesc on proc.oid = pgdesc.objoid

                    left outer join information_schema.parameters p
                    on r.specific_name = p.specific_name and r.specific_schema = p.specific_schema
    
                where
                    lower(r.external_language) = any($7)
                    and
                    (   $1 is null or (r.specific_schema similar to $1)   )
                    and (   $2 is null or r.specific_schema not similar to $2   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )

                    and ($3 is null or r.routine_name not similar to $3)
                    and ($4 is null or r.routine_name similar to $4)
                    
                    and (
                        ($5 is true)
                        or (r.type_udt_name is null and r.routine_type = 'PROCEDURE') 
                        or (r.type_udt_name is not null and r.type_udt_name <> 'trigger' and r.type_udt_name <> 'refcursor')
                    )
                    
                    and (   $6 is null or (r.routine_name not similar to $6)   )
                group by
                    proc.oid,
                    r.specific_schema,
                    r.specific_name,
                    r.routine_name,
                    r.routine_definition,
                    pgdesc.description,
                    r.external_language,
                    r.routine_type,
                    r.type_udt_name,
                    proc.proretset,
                    r.data_type
                order by 
                    r.specific_schema,
                    r.routine_name
            ", 
                r => (
                    r.Val<uint>(0),
                    r.Val<string>(1),
                    r.Val<string>(2),
                    r.Val<string>(3),
                    r.Val<string>(4),
                    r.Val<string>(5),
                    r.Val<string>(6),
                    r.Val<string>(7),
                    r.Val<string>(8),
                    r.Val<string>(9),
                    r.Val<bool>(10),
                    r.Val<string>(11),
                    r.Val<string>(12)
                    ))
            .Select(t =>
            {
                return new PgRoutineGroup
                {
                    Oid = t.Oid,
                    SpecificSchema = t.SpecificSchema,
                    SpecificName = t.SpecificName,
                    RoutineName = t.RoutineName,
                    Definition = t.RoutineDefinition,
                    FullDefinition = t.FullRoutineDefinition,
                    Description = t.Description,
                    Language = t.Language,
                    RoutineType = t.RoutineType,
                    TypeUdtName = t.TypeUdtName,
                    IsSet = t.IsSet,
                    DataType = t.DataType,
                    Parameters = JsonSerializer.Deserialize(t.Parameters, jsonCtx.PgParameterArray).ToList()
 
                };
            })
            .GroupBy(i => (i.SpecificSchema, i.RoutineName));


        /*
        return connection
            .WithParameters(
                (schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString),
                (schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString),
                (settings.RoutinesNotSimilarTo, DbType.AnsiString),
                (settings.RoutinesSimilarTo, DbType.AnsiString),
                (all, DbType.Boolean),
                (skipSimilar, DbType.AnsiString),
                (settings.RoutinesLanguages.ToList(), NpgsqlDbType.Array | NpgsqlDbType.Text))
            .Read<(
                uint Oid,
                string SpecificSchema,
                string SpecificName,
                string RoutineName,
                string RoutineDefinition,
                string FullRoutineDefinition,
                string Description,
                string Language,
                string RoutineType,
                string TypeUdtName,
                bool IsSet,
                string DataType,
                string Parameters)>(@$"

                select 
                    proc.oid,
                    r.specific_schema,
                    r.specific_name,
                    r.routine_name,
                    
                    r.routine_definition,
                    pg_get_functiondef(proc.oid) as full_routine_definition,
                    
                    pgdesc.description,
                    lower(r.external_language) as language,
                    lower(r.routine_type) as routine_type,

                    regexp_replace(r.type_udt_name, '^[_]', '') as type_udt_name,
                    proc.proretset,

                    case 
                        when array_length((array_agg(p.ordinal_position) filter (where p.ordinal_position is not null and p.parameter_mode = 'OUT')), 1) > 0
                        then 'record'
                        else r.data_type
                    end as data_type,

                    coalesce (
                        json_agg(
                            json_build_object(
                                'ordinal', p.ordinal_position,
                                'name', p.parameter_name,
                                'type', regexp_replace(p.udt_name, '^[_]', ''),
                                'dataType', p.data_type,
                                'isArray', p.data_type = 'ARRAY',
                                'default', p.parameter_default,
                                'dataTypeFormatted', case    when p.data_type = 'USER-DEFINED' 
                                                                then p.udt_name 
                                                                else case   when p.udt_schema <> 'pg_catalog' 
                                                                            then p.udt_schema || '.' 
                                                                            else '' 
                                                                end || p.udt_name::regtype 
                                                        end || 
                                                        case    when p.data_type <> 'integer' and p.data_type <> 'bigint' and p.data_type <> 'smallint' 
                                                                then
                                                                    case    when p.character_maximum_length is not null 
                                                                            then '(' || cast(p.character_maximum_length as varchar) || ')'
                                                                            else 
                                                                                case    when p.numeric_precision is not null 
                                                                                        then '(' || cast(p.numeric_precision as varchar) || ',' || cast(p.numeric_scale as varchar) || ')'
                                                                                else ''
                                                                                end
                                                                    end
                                                                else ''
                                                        end
                            ) 
                            order by 
                                p.ordinal_position
                        ) 
                        filter (
                            where 
                                p.ordinal_position is not null 
                                and (p.parameter_mode = 'IN' or  p.parameter_mode = 'INOUT')
                        ),
                        '[]'
                    ) as parameters

                from
                    information_schema.routines r
                    inner join pg_catalog.pg_proc proc on r.specific_name = proc.proname || '_' || proc.oid
                    left outer join pg_catalog.pg_description pgdesc on proc.oid = pgdesc.objoid

                    left outer join information_schema.parameters p
                    on r.specific_name = p.specific_name and r.specific_schema = p.specific_schema
    
                where
                    lower(r.external_language) = any($7)
                    and
                    (   $1 is null or (r.specific_schema similar to $1)   )
                    and (   $2 is null or r.specific_schema not similar to $2   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )

                    and ($3 is null or r.routine_name not similar to $3)
                    and ($4 is null or r.routine_name similar to $4)
                    
                    and (
                        ($5 is true)
                        or (r.type_udt_name is null and r.routine_type = 'PROCEDURE') 
                        or (r.type_udt_name is not null and r.type_udt_name <> 'trigger' and r.type_udt_name <> 'refcursor')
                    )
                    
                    and (   $6 is null or (r.routine_name not similar to $6)   )
                group by
                    proc.oid,
                    r.specific_schema,
                    r.specific_name,
                    r.routine_name,
                    r.routine_definition,
                    pgdesc.description,
                    r.external_language,
                    r.routine_type,
                    r.type_udt_name,
                    proc.proretset,
                    r.data_type
                order by 
                    r.specific_schema,
                    r.routine_name
            ")
            .Select(t => new PgRoutineGroup
            {
                Oid = t.Oid,
                SpecificSchema = t.SpecificSchema,
                SpecificName = t.SpecificName,
                RoutineName = t.RoutineName,
                Definition = t.RoutineDefinition,
                FullDefinition = t.FullRoutineDefinition,
                Description = t.Description,
                Language = t.Language,
                RoutineType = t.RoutineType,
                TypeUdtName = t.TypeUdtName,
                IsSet = t.IsSet,
                DataType = t.DataType,
                Parameters = JsonConvert.DeserializeObject<List<PgParameter>>(t.Parameters)
            })
            .GroupBy(i => (i.SpecificSchema, i.RoutineName));
        */
    }
}
