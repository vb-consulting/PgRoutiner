using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Norm.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace PgRoutiner
{
    public class PgType
    {
        public int Ordinal { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Array { get; set; }
    }

    public class PgReturns
    {
        public string Type { get; set; }
        public IEnumerable<PgType> Record { get; set; }
    }

    public static class DataAccess
    {
        public static IEnumerable<(string name, IEnumerable<PgType> parameters, PgReturns returns, string description)> 
            GetRoutines(this NpgsqlConnection connection, Settings settings)
        {
            return connection.Read<string, string, string, string>(@"

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
                    'type', r.type_udt_name,
                    'record',
                        case 
                            when r.type_udt_name = 'record' then 
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

                pgdesc.description


            from
                information_schema.routines r
                left outer join information_schema.parameters p
                on r.specific_name = p.specific_name and r.specific_schema = p.specific_schema

                inner join pg_catalog.pg_proc proc on r.routine_name = proc.proname
                left outer join pg_catalog.pg_description pgdesc on proc.oid = pgdesc.objoid
            where
                r.specific_schema = @schema
                and r.external_language <> 'INTERNAL'
                and (@skipPattern is null or r.routine_name not similar to @skipPattern)
                

            group by
                r.routine_name,
                r.data_type,
                r.type_udt_name,
                pgdesc.description

            ",
                ("schema", settings.Schema, DbType.AnsiString),
                ("skipPattern", settings.SkipSimilarTo, DbType.AnsiString)).Select(t => 
            (
                t.Item1,
                JsonConvert.DeserializeObject<IEnumerable<PgType>>(t.Item2),
                JsonConvert.DeserializeObject<PgReturns>(t.Item3),
                t.Item4
            ));
        }
    }
}
