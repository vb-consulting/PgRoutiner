using System.Data;
using Norm;
using Newtonsoft.Json;
using PgRoutiner.DataAccess.Models;
using NpgsqlTypes;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<IGrouping<(string Schema, string Name), PgRoutineGroup>> GetRoutineGroups(
        this NpgsqlConnection connection, Current settings, bool all = true, string skipSimilar = null, 
        string schemaSimilarTo = null, string schemaNotSimilarTo = null)
    {
        return connection
            .WithParameters(
                (schemaSimilarTo ?? settings.SchemaSimilarTo, DbType.AnsiString),
                (schemaNotSimilarTo ?? settings.SchemaNotSimilarTo, DbType.AnsiString),
                (settings.RoutinesNotSimilarTo, DbType.AnsiString),
                (settings.RoutinesSimilarTo, DbType.AnsiString),
                (all, DbType.Boolean),
                (skipSimilar, DbType.AnsiString),
                (settings.RoutinesLanguages, NpgsqlDbType.Array | NpgsqlDbType.Text))
            .Read<(
                uint Oid,
                string SpecificSchema,
                string SpecificName,
                string RoutineName,
                string Description,
                string Language,
                string RoutineType,
                string TypeUdtName,
                string DataType,
                string Parameters)>(@$"

                select 
                    proc.oid,
                    r.specific_schema,
                    r.specific_name,
                    r.routine_name,
                    pgdesc.description,
                    lower(r.external_language) as language,
                    lower(r.routine_type) as routine_type,

                    regexp_replace(r.type_udt_name, '^[_]', '') as type_udt_name,
    
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
                                'default', p.parameter_default
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
                    and ($5 is true or (r.type_udt_name <> 'trigger' and r.type_udt_name <> 'refcursor'))
                    and (   $6 is null or (r.routine_name not similar to $6)   )
                group by
                    proc.oid,
                    r.specific_schema,
                    r.specific_name,
                    r.routine_name,
                    pgdesc.description,
                    r.external_language,
                    r.routine_type,
                    r.type_udt_name,
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
                Description = t.Description,
                Language = t.Language,
                RoutineType = t.RoutineType,
                TypeUdtName = t.TypeUdtName,
                DataType = t.DataType,
                Parameters = JsonConvert.DeserializeObject<List<PgParameter>>(t.Parameters)
            })
            .GroupBy(i => (i.SpecificSchema, i.RoutineName));
    }
}
