using System.Collections.Generic;

namespace PgRoutiner
{
    public class PgParameter
    {
        public int Ordinal { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string DataType { get; set; }
        public bool Array { get; set; }
    }

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

    public enum PgType
    {
        Table,
        View,
        Function,
        Procedure,
        Domain,
        Type,
        Schema,
        Sequence,
        Unknown
    }

    public class PgItem
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public PgType Type { get; set; }
    }

    public static class PgItemExt
    {
        public static string GetFileName(this PgItem item)
        {
            if (!string.Equals(item.Schema, "public"))
            {
                return $"{item.Schema}.{item.Name}.sql";
            }
            return $"{item.Name}.sql";
        }

        public static string GetTableArg(this PgItem item) => $"--table=\\\"{item.Schema}\\\".\\\"{item.Name}\\\"";
    }

    public class PgReturns 
    {
        public int Ordinal { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string DataType { get; set; }
        public bool Array { get; set; }
        public bool Nullable { get; set; }
    };

    public class RoutineComment
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Signature { get; set; }
        public string Returns { get; set; }
        public string Language { get; set; }
        public string Comment { get; set; }
    }

    public class TableComment
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string ConstraintMarkup { get; set; }
        public string ColumnType { get; set; }
        public string Nullable { get; set; }
        public string DefaultMarkup { get; set; }
        public string Comment { get; set; }
    }
}
