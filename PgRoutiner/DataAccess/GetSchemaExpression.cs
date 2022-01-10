namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static string GetSchemaExpression(string field)
    {
        return $"{field} not like 'pg_temp%' and {field} not like 'pg_toast%' and {field} <> 'information_schema' and {field} <> 'pg_catalog'";
    }
}
