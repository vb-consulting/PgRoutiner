using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgReturns> GetRoutineReturnsTable(this NpgsqlConnection connection, PgRoutineGroup routine)
    {
        var result = connection.GetTableColumnsForRoutine(routine);
        if (result.Any())
        {
            return result;
        }
        return connection.GetTypeColumnsForRoutine(routine);
    }

    private static IEnumerable<PgReturns> GetTableColumnsForRoutine(this NpgsqlConnection connection, PgRoutineGroup routine) =>
        connection
        .WithParameters((routine.TypeUdtName, DbType.AnsiString))
        .Read<PgReturns>(@"

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
                c.table_name = $1
            order by
                c.ordinal_position

            ");

    private static IEnumerable<PgReturns> GetTypeColumnsForRoutine(this NpgsqlConnection connection, PgRoutineGroup routine) =>
        connection
        .WithParameters((routine.TypeUdtName, DbType.AnsiString))
        .Read<PgReturns>(@"

            select 
                (row_number() over ())::int as ordinal,
                a.attname as name, 
                regexp_replace(t.typname, '^[_]', '') as type, 
                null as data_type,
                t.typinput::text like 'array_%' as array, 
                not t.typnotnull as nullable

            from pg_class c 
            inner join pg_attribute a on c.oid = a.attrelid 
            inner join pg_type t on a.atttypid = t.oid
            where 
                c.relname = $1

            ");
}
