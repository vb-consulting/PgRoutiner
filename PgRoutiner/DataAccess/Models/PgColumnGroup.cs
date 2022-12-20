namespace PgRoutiner.DataAccess.Models;

public class PgColumnGroup : PgParameter
{
    public string Schema { get; set; }
    public string Table { get; set; }
    public bool IsGeneration { get; set; }
    public bool HasDefault { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsPk { get; set; }

    public string _Type => $"{this.Type}{(this.IsArray ? "[]" : "")}";
}
