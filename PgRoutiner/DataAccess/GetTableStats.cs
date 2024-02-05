using NpgsqlTypes;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static PgTableStats GetTableStats(this NpgsqlConnection connection, string schema, string table)
    {
        return connection.Read<PgTableStats>(
                   [
            (table, null, NpgsqlDbType.Text),
            (schema, null, NpgsqlDbType.Text)
        ], @"

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
            relname= $1 and schemaname = $2", r => new PgTableStats
        {
               SeqScanCount = r.Val<long>(0),
               SeqScanRows = r.Val<long>(1),
               IdxScanCount = r.Val<long>(2),
               IdxScanRows = r.Val<long>(3),
               RowsInserted = r.Val<long>(4),
               RowsUpdated = r.Val<long>(5),
               RowsDeleted = r.Val<long>(6),
               LiveRows = r.Val<long>(7),
               DeadRows = r.Val<long>(8),
               RowsModifiedSinceAnalyze = r.Val<long>(9),
               RowsInsertedSinceVacuum = r.Val<long>(10),
               LastVacuum = r.Val<DateTime?>(11),
               VacuumCount = r.Val<long>(12),
               LastAnalyze = r.Val<DateTime?>(13),
               AnalyzeCount = r.Val<long>(14),
               LastAutoanalyze = r.Val<DateTime?>(15),
               LastAutovacuum = r.Val<DateTime?>(16)
        }).FirstOrDefault();
        /*
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
        */
    }
}
