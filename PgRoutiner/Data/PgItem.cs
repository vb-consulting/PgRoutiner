
namespace PgRoutiner
{
    public enum PgType
    {
        Table,
        View,
        Function,
        Procedure,
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
}
