using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static IEnumerable<RoutineComment> GetRoutineComments(this NpgsqlConnection connection, Settings settings, string schema) =>
            connection.Read<(string Type, string Name, string Signature, string Returns, string Language, string Comment)>(@"

                select
                    lower(r.routine_type) as type,
                    r.routine_name,

                    r.routine_name || 
                        '(' || 
                        array_to_string(
                            array_agg(
                                case    when p.parameter_mode = 'IN' 
                                        then '' else lower(p.parameter_mode) || ' ' 
                                end || coalesce(case when p.data_type = 'ARRAY' then regexp_replace(p.udt_name, '^[_]', '')  || '[]' else p.data_type end, '')
                                order by p.ordinal_position
                            ), 
                            ', '
                        ) ||
                        ')' as signature,
                        
                    case    when    r.data_type = 'USER-DEFINED' and 
                                    r.type_udt_catalog is not null and 
                                    r.type_udt_schema is not null and 
                                    r.type_udt_name is not null 
                            then 'setof ' || r.type_udt_name
                            else r.data_type
                    end as returns_type,
                    
                    lower(r.external_language) as language,
                    
                    pgdesc.description

                from 
                    information_schema.routines r
                    left outer join information_schema.parameters p 
                    on r.specific_name = p.specific_name and r.specific_schema = p.specific_schema

                    inner join pg_catalog.pg_proc proc on r.specific_name = proc.proname || '_' || proc.oid
                    left outer join pg_catalog.pg_description pgdesc on proc.oid = pgdesc.objoid
                where
                    r.specific_schema = @schema
                    and r.external_language <> 'INTERNAL'

                    and (@notSimilarTo is null or r.routine_name not similar to @notSimilarTo)
                    and (@similarTo is null or r.routine_name similar to @similarTo)

                group by
                    r.specific_name, r.routine_type, r.external_language, r.routine_name, 
                    r.data_type, r.type_udt_catalog, r.type_udt_schema, r.type_udt_name,
                    pgdesc.description
            ",
                ("schema", schema, DbType.AnsiString),
                ("notSimilarTo", settings.MdNotSimilarTo, DbType.AnsiString),
                ("similarTo", settings.MdSimilarTo, DbType.AnsiString))

            .Select(t => new RoutineComment
            {
                Comment = t.Comment,
                Language = t.Language,
                Name = t.Name,
                Returns = t.Returns,
                Signature = t.Signature,
                Type = t.Type
            });
    }
}
