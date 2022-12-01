namespace PgRoutiner.DataAccess.Models;

public class PgReturns
{
    public int Ordinal { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string DataType { get; set; }
    public bool Array { get; set; }
    public bool Nullable { get; set; }
    public string DataTypeFormatted { get; set; }
};
