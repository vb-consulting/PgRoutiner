using System.Data;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgReturns> GetRoutineReturnsRecord(this NpgsqlConnection connection, PgRoutineGroup routine)
    {
        return connection.Read<PgReturns>(
            [
                (routine.SpecificName, DbType.AnsiString, null),
                (routine.SpecificSchema, DbType.AnsiString, null)
            ], @"

            select 
                p.ordinal_position as ordinal,
                p.parameter_name as name,
                regexp_replace(p.udt_name, '^[_]', '') as type,
                p.data_type,
                p.data_type = 'ARRAY' as array,
                true as nullable,

                case    when p.data_type = 'USER-DEFINED' 
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
                end as data_type_formatted

            from 
                information_schema.parameters p
            where 
                p.ordinal_position is not null 
                and (p.parameter_mode = 'OUT' or p.parameter_mode = 'INOUT')
                and p.specific_name = $1 and p.specific_schema = $2
            order by 
                p.ordinal_position 

            ", r => new PgReturns
        {
            Ordinal = r.Val<int>("ordinal"),
            Name = r.Val<string>("name"),
            Type = r.Val<string>("type"),
            DataType = r.Val<string>("data_type"),
            Array = r.Val<bool>("array"),
            Nullable = r.Val<bool>("nullable"),
            DataTypeFormatted = r.Val<string>("data_type_formatted")
        });
        /*
        return connection
        .WithParameters(
            (routine.SpecificName, DbType.AnsiString),
            (routine.SpecificSchema, DbType.AnsiString))
        .Read<PgReturns>(@"

            select 
                p.ordinal_position as ordinal,
                p.parameter_name as name,
                regexp_replace(p.udt_name, '^[_]', '') as type,
                p.data_type,
                p.data_type = 'ARRAY' as array,
                true as nullable,

                case    when p.data_type = 'USER-DEFINED' 
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
                end as data_type_formatted

            from 
                information_schema.parameters p
            where 
                p.ordinal_position is not null 
                and (p.parameter_mode = 'OUT' or p.parameter_mode = 'INOUT')
                and p.specific_name = $1 and p.specific_schema = $2
            order by 
                p.ordinal_position 

            ");
        */
    }
}
