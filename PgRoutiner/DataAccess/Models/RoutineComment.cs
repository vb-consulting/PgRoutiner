﻿namespace PgRoutiner.DataAccess.Models;

public class RoutineComment
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string SpecificName { get; set; }
    public string Returns { get; set; }
    public string Language { get; set; }
    public string Comment { get; set; }
    public bool IsSet { get; set; }
    public string Definition { get; set; }
    public string[] Parameters { get; set; }
    public string Signature { get; set; }
}
