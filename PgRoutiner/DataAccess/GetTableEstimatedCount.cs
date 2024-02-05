using NpgsqlTypes;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static long GetTableEstimatedCount(this NpgsqlConnection connection, string schema, string table)
    {
        return connection.Read<long>(
        [
            (table, null, NpgsqlDbType.Text),
            (schema, null, NpgsqlDbType.Text)
        ], @"
            select reltuples::bigint
            from 
                pg_class a
                inner join pg_namespace b on a.relnamespace = b.oid
            where
                relname::text = $1 and nspname::text = $2
        ", r => r.Val<long>(0))
            .FirstOrDefault();
        /*
        return connection
            .WithParameters(new
            {
                table = (table, NpgsqlDbType.Text),
                schema = (schema, NpgsqlDbType.Text),
            })
            .Read<long>(@$"

            select reltuples::bigint
            from 
                pg_class a
                inner join pg_namespace b on a.relnamespace = b.oid
            where
                relname::text = @table and nspname::text = @schema

            ").FirstOrDefault();
        */
    }
}
