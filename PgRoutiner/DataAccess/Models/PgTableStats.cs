using System.Collections.Generic;

namespace PgRoutiner.DataAccess.Models;

public class PgTableStats
{
    public long? SeqScanCount { get; set; }
    public long? SeqScanRows { get; set; }
    public long? IdxScanCount { get; set; }
    public long? IdxScanRows { get; set; }
    public long? RowsInserted { get; set; }
    public long? RowsUpdated { get; set; }
    public long? RowsDeleted { get; set; }
    public long? LiveRows { get; set; }
    public long? DeadRows { get; set; }
    public long? RowsModifiedSinceAnalyze { get; set; }
    public long? RowsInsertedSinceVacuum { get; set; }
    public DateTime? LastVacuum { get; set; }
    public long? VacuumCount { get; set; }
    public DateTime? LastAnalyze { get; set; }
    public long? AnalyzeCount { get; set; }
    public DateTime? LastAutoanalyze { get; set; }
    public DateTime? LastAutovacuum { get; set; }
}
