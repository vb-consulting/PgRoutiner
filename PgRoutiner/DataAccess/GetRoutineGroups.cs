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
        public static IEnumerable<IGrouping<(string Schema, string Name), PgRoutineGroup>> GetRoutineGroups(this NpgsqlConnection connection, Settings settings, bool all = true)
        {
            return connection.Read<(
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
                    r.data_type,

                    coalesce (
                        json_agg(
                            json_build_object(
                                'ordinal', p.ordinal_position,
                                'name', p.parameter_name,
                                'type', regexp_replace(p.udt_name, '^[_]', ''),
                                'dataType', p.data_type,
                                'isArray', p.data_type = 'ARRAY'
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
                    r.external_language <> 'INTERNAL'
                    and
                    (   @schema is null or (r.specific_schema similar to @schema)   )
                    and (   {GetSchemaExpression("r.specific_schema")}  )
                    and (@notSimilarTo is null or r.routine_name not similar to @notSimilarTo)
                    and (@similarTo is null or r.routine_name similar to @similarTo)
                    and (@all is true or (r.type_udt_name <> 'trigger' and r.type_udt_name <> 'refcursor'))
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
                    r.routine_name

            ", 
            ("schema", settings.Schema, DbType.AnsiString),
            ("notSimilarTo", settings.NotSimilarTo, DbType.AnsiString),
            ("similarTo", settings.SimilarTo, DbType.AnsiString),
            ("all", all, DbType.Boolean)).Select(t => new PgRoutineGroup
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
                Parameters = JsonConvert.DeserializeObject<IList<PgParameter>>(t.Parameters)
            }).GroupBy(i => (i.SpecificSchema, i.RoutineName));
        }
    }
}
