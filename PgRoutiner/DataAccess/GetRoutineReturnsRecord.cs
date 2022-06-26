﻿using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgReturns> GetRoutineReturnsRecord(this NpgsqlConnection connection, PgRoutineGroup routine) =>
        connection
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
                true as nullable
            from 
                information_schema.parameters p
            where 
                p.ordinal_position is not null 
                and (p.parameter_mode = 'OUT' or p.parameter_mode = 'INOUT')
                and p.specific_name = $1 and p.specific_schema = $2
            order by 
                p.ordinal_position 

            ");
}
