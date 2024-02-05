using System.Data;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<(string Schema, string Table, string Expression)> GetPartitionTables(
        this NpgsqlConnection connection, PgItem table)
    {
        return connection
            .Read<(string Schema, string Table, string Expression)>(
            [
                (table.Schema, DbType.AnsiString, null), 
                (table.Name, DbType.AnsiString, null)
            ], @$"
            select
                nmsp_child.nspname,
                child.relname,
                pg_get_expr(child.relpartbound, child.oid, true)
            from 
                pg_inherits
                inner join pg_class parent on pg_inherits.inhparent = parent.oid
                inner join pg_class child on pg_inherits.inhrelid = child.oid
                inner join pg_namespace nmsp_parent on nmsp_parent.oid = parent.relnamespace
                inner join pg_namespace nmsp_child on nmsp_child.oid = child.relnamespace
            where
                nmsp_parent.nspname = $1 and parent.relname = $2;

        ", r => (r.Val<string>(0), r.Val<string>(1), r.Val<string>(2)));

        //return connection
        //    .WithParameters(
        //        (table.Schema, DbType.AnsiString),
        //        (table.Name, DbType.AnsiString))
        //    .Read<(string Schema, string Table, string Expression)>(@$"
        //    select
        //        nmsp_child.nspname,
        //        child.relname,
        //        pg_get_expr(child.relpartbound, child.oid, true)
        //    from 
        //        pg_inherits
        //        inner join pg_class parent on pg_inherits.inhparent = parent.oid
        //        inner join pg_class child on pg_inherits.inhrelid = child.oid
        //        inner join pg_namespace nmsp_parent on nmsp_parent.oid = parent.relnamespace
        //        inner join pg_namespace nmsp_child on nmsp_child.oid = child.relnamespace
        //    where
        //        nmsp_parent.nspname = $1 and parent.relname = $2;

        //");
    }

    public static IEnumerable<(string Schema, string Table)> GetAllPartitionTables(this NpgsqlConnection connection)
    {

        return connection
            .Read<(string Schema, string Table)>(
            [
            ], @$"
            select
                nmsp_child.nspname,
                child.relname
            from 
                pg_inherits
                inner join pg_class parent on pg_inherits.inhparent = parent.oid
                inner join pg_class child on pg_inherits.inhrelid = child.oid
                inner join pg_namespace nmsp_parent on nmsp_parent.oid = parent.relnamespace
                inner join pg_namespace nmsp_child on nmsp_child.oid = child.relnamespace
            where
                pg_get_expr(child.relpartbound, child.oid, true) is not null;


        ", r => (r.Val<string>(0), r.Val<string>(1)));

        //return connection.Read<(string Schema, string Table)>(@$"

        //    select
        //        nmsp_child.nspname,
        //        child.relname
        //    from 
        //        pg_inherits
        //        inner join pg_class parent on pg_inherits.inhparent = parent.oid
        //        inner join pg_class child on pg_inherits.inhrelid = child.oid
        //        inner join pg_namespace nmsp_parent on nmsp_parent.oid = parent.relnamespace
        //        inner join pg_namespace nmsp_child on nmsp_child.oid = child.relnamespace
        //    where
        //        pg_get_expr(child.relpartbound, child.oid, true) is not null;

        //");
    }
}
