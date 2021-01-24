using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
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
