namespace PgRoutiner.DataAccess.Models;

public record PgRoutineGroup
{
    public uint Oid { get; set; }
    public string SpecificSchema { get; set; }
    public string SpecificName { get; set; }
    public string RoutineName { get; set; }
    public string Description { get; set; }
    public string Language { get; set; }
    public string RoutineType { get; set; }
    public string TypeUdtName { get; set; }
    public string DataType { get; set; }
    public List<PgParameter> Parameters { get; set; }
    //public List<string> ModelNames { get; set; } = new();
    public List<(string type, string name)> ModelItems { get; set; } = new();
}
