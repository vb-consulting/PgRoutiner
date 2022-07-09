using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<(string Schema, string Table, string Expression)> GetPartitionTables(
        this NpgsqlConnection connection, PgItem table)
    {
        return connection
            .WithParameters(
                (table.Schema, DbType.AnsiString),
                (table.Name, DbType.AnsiString))
            .Read<(string Schema, string Table, string Expression)>(@$"
            select
                nmsp_child.nspname,
                child.relname,
                pg_get_expr(child.relpartbound, child.oid, true)
            from 
                pg_inherits
                inner join pg_class parent on pg_inherits.inhparent = parent.oid
                inner join pg_class child on pg_inherits.inhrelid   = child.oid
                inner join pg_namespace nmsp_parent on nmsp_parent.oid  = parent.relnamespace
                inner join pg_namespace nmsp_child on nmsp_child.oid   = child.relnamespace
            where
                nmsp_parent.nspname = $1 and parent.relname = $2;

        ");
    }
}
