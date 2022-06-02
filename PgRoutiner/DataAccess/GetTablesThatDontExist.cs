using Norm;
using NpgsqlTypes;
using PgRoutiner.Builder.DiffBuilder;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<string> GetTablesThatDontExist(this NpgsqlConnection connection, IEnumerable<string> tableNames)
    {
        return connection
            .WithParameters(new { tables = (tableNames, NpgsqlDbType.Varchar | NpgsqlDbType.Array) })
            .Read<string>(@$"

        select 
            n
        from 
            unnest(@tables) n
            left outer join information_schema.tables t 
            on 
                t.table_name = n 
                or '""' || t.table_name || '""' = n
                or t.table_schema || '.' || t.table_name = n
                or t.table_schema || '.' || '""' || t.table_name || '""' = n
        where
                t.table_name is null

        ");
    }
}
