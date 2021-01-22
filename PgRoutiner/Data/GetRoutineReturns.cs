using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public record PgReturns(int Ordinal, string Name, string Type, string DataType, bool Array, bool Nullable);

    public static partial class DataAccess
    {
        public static IEnumerable<PgReturns> GetRoutineReturnsRecord(this NpgsqlConnection connection, PgRoutineGroup routine) =>
            connection.Read<PgReturns>(@"

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
                and p.specific_name = @specificName and p.specific_schema = @specificSchema
            order by 
                p.ordinal_position 

            ",
                ("specificName", routine.SpecificName, DbType.AnsiString),
                ("specificSchema", routine.SpecificSchema, DbType.AnsiString));

        public static IEnumerable<PgReturns> GetRoutineReturnsTable(this NpgsqlConnection connection, PgRoutineGroup routine) =>
            connection.Read<PgReturns>(@"

            select 
                c.ordinal_position as ordinal,
                c.column_name as name, 
                regexp_replace(c.udt_name, '^[_]', '') as type,
                c.data_type,
                c.data_type = 'ARRAY' as array,
                c.is_nullable = 'YES' as nullable

            from
                information_schema.columns c
            where
                c.table_name = @typeUdtName
            order by
                c.ordinal_position

            ",
                ("typeUdtName", routine.TypeUdtName, DbType.AnsiString));
    }
}
