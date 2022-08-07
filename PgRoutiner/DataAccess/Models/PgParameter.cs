namespace PgRoutiner.DataAccess.Models;

public class PgParameter
{
    public int Ordinal { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string DataType { get; set; }
    public bool IsArray { get; set; }
    public string Default { get; set; }
}
