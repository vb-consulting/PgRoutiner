using System.Collections.Generic;

namespace PgRoutiner.DataAccess.Models;

public class PgRoutineGroup
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
    public IList<PgParameter> Parameters { get; set; }
}
