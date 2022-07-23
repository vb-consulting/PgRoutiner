namespace PgRoutiner.DataAccess.Models;

public class TableComment
{
    public string Table { get; set; }
    public bool HasPartitions { get; set; }
    public string Column { get; set; }
    public bool? IsPk { get; set; }
    public string ConstraintMarkup { get; set; }
    public string ColumnType { get; set; }
    public bool? IsUdt { get; set; }
    public string Nullable { get; set; }
    public string DefaultMarkup { get; set; }
    public string Comment { get; set; }
}
