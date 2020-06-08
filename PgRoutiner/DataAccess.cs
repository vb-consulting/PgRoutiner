using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Norm.Extensions;
using Npgsql;

namespace PgRoutiner
{
    public class PgBaseType
    {
        public string Type { get; set; }
        public bool Array { get; set; }
    }

    public class PgType : PgBaseType
    {
        public int Ordinal { get; set; }
        public string Name { get; set; }
    }

    public class PgReturns : PgBaseType
    {
        public bool UserDefined { get; set; }
        public IEnumerable<PgType> Record { get; set; }
    }

    public class GetRoutinesResult
    {
        public string Name { get; set; }
        public IEnumerable<PgType> Parameters { get; set; }
        public PgReturns Returns { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string RoutineType { get; set; }
    }

    public static class DataAccess
    {
        public static IEnumerable<GetRoutinesResult> GetRoutines(this NpgsqlConnection connection, Settings settings) =>
            connection.Read<string, string, string, string, string, string>(@"

            select
                r.routine_name,
                
                coalesce (
                    json_agg(
                        json_build_object(
                            'ordinal', p.ordinal_position,
                            'name', p.parameter_name,
                            'type', regexp_replace(p.udt_name, '^[_]', ''),
                            'array', p.data_type = 'ARRAY'
                        ) 
                        order by p.ordinal_position
                    ) 
                    filter (where p.ordinal_position is not null and (p.parameter_mode = 'IN' or  p.parameter_mode = 'INOUT')),
                    '[]'
                ) as parameters,
                
                json_build_object(
                    'type', regexp_replace(r.type_udt_name, '^[_]', ''),
                    'array', r.data_type = 'ARRAY',
                    'userDefined', r.data_type = 'USER-DEFINED',
                    'record',
                        case 
                            when (r.type_udt_name = 'record' or (r.data_type <> 'USER-DEFINED')) then 
                                coalesce (
                                    json_agg(
                                        json_build_object(
                                            'ordinal', p.ordinal_position,
                                            'name', p.parameter_name,
                                            'type', regexp_replace(p.udt_name, '^[_]', ''),
                                            'array', p.data_type = 'ARRAY'
                                        )
                                        order by p.ordinal_position
                                    ) 
                                    filter (where p.ordinal_position is not null and (p.parameter_mode = 'OUT' or p.parameter_mode = 'INOUT')),
                                    '[]'
                                )
                            when r.data_type = 'USER-DEFINED' then (
                                select 
                                    coalesce (
                                        json_agg(
                                            json_build_object(
                                                'ordinal', c.ordinal_position,
                                                'name', c.column_name,
                                                'type', regexp_replace(c.udt_name, '^[_]', ''),
                                                'array', c.data_type = 'ARRAY'
                                            )
                                            order by c.ordinal_position
                                        ),
                                        '[]'
                                    )
                                from
                                    information_schema.columns c
                                where
                                    c.table_name = r.type_udt_name and c.table_schema = @schema
                            )
                            else null
                        end
                ) as returns,

                pgdesc.description,
                lower(r.external_language) as language,
                lower(r.routine_type) as routine_type


            from
                information_schema.routines r
                left outer join information_schema.parameters p
                on r.specific_name = p.specific_name and r.specific_schema = p.specific_schema

                inner join pg_catalog.pg_proc proc on r.routine_name = proc.proname
                left outer join pg_catalog.pg_description pgdesc on proc.oid = pgdesc.objoid
            where
                r.specific_schema = @schema
                and r.external_language <> 'INTERNAL'
                and (@notSimilarTo is null or r.routine_name not similar to @notSimilarTo)
                and (@similarTo is null or r.routine_name similar to @similarTo)

            group by
                r.routine_name, r.data_type, r.type_udt_name, pgdesc.description, r.external_language, r.routine_type

            ",
                ("schema", settings.Schema, DbType.AnsiString),
                ("notSimilarTo", settings.NotSimilarTo, DbType.AnsiString),
                ("similarTo", settings.SimilarTo, DbType.AnsiString)).Select(t => new GetRoutinesResult 
            {
                Name = t.Item1,
                Parameters = JsonConvert.DeserializeObject<IEnumerable<PgType>>(t.Item2),
                Returns = JsonConvert.DeserializeObject<PgReturns>(t.Item3),
                Description = t.Item4,
                Language = t.Item5,
                RoutineType = t.Item6
            });
    }
}
