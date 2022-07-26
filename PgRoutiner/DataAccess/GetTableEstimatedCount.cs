using System.Data;
using Norm;
using NpgsqlTypes;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static long GetTableEstimatedCount(this NpgsqlConnection connection, string schema, string table)
    {
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
    }
}
