using NpgsqlTypes;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<string> GetTablesThatDontExist(this NpgsqlConnection connection, IEnumerable<string> tableNames)
    {
        return connection.Read<string>([(tableNames.ToList(), null, NpgsqlDbType.Varchar | NpgsqlDbType.Array)], @"
        select 
            n
        from 
            unnest($1) n
            left outer join information_schema.tables t 
            on 
                t.table_name = n 
                or '""' || t.table_name || '""' = n
                or t.table_schema || '.' || t.table_name = n
                or t.table_schema || '.' || '""' || t.table_name || '""' = n
        where
                t.table_name is null
        ", r => r.Val<string>(0));
        /*
        return connection
            .WithParameters((tableNames.ToList(), NpgsqlDbType.Varchar | NpgsqlDbType.Array))
            .Read<string>(@$"

        select 
            n
        from 
            unnest($1) n
            left outer join information_schema.tables t 
            on 
                t.table_name = n 
                or '""' || t.table_name || '""' = n
                or t.table_schema || '.' || t.table_name = n
                or t.table_schema || '.' || '""' || t.table_name || '""' = n
        where
                t.table_name is null

        ");
        */
    }
}
