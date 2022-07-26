using System.Data;
using Norm;
using NpgsqlTypes;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static PgTableStats GetTableStats(this NpgsqlConnection connection, string schema, string table)
    {
        return connection
            .WithParameters(new 
            {
                table = (table, NpgsqlDbType.Text),
                schema = (schema, NpgsqlDbType.Text),
            })
            .Read<PgTableStats>(@$"

                select 
                    seq_scan as seq_scan_count,
                    seq_tup_read as seq_scan_rows,
                    idx_scan as idx_scan_count,
                    idx_tup_fetch as idx_scan_rows,
                    n_tup_ins as rows_inserted,
                    n_tup_upd as rows_updated,
                    n_tup_del as rows_deleted,
                    n_live_tup as live_rows,
                    n_dead_tup as dead_rows,
                    n_mod_since_analyze as rows_modified_since_analyze,
                    n_ins_since_vacuum as rows_inserted_since_vacuum,
                    last_vacuum, 
                    vacuum_count, 
                    last_analyze, 
                    analyze_count, 
                    last_autoanalyze, 
                    last_autovacuum
                from 
                    pg_stat_all_tables
                where
                    relname= @table and schemaname = @schema

            ").FirstOrDefault();
    }
}
