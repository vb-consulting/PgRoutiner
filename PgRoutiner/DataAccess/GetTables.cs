using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetTables(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
    {
        return connection.Read<(string Schema, string Name, string Type)>(@$"

            select 
                table_schema, table_name, table_type
            from 
                information_schema.tables
            where
                (   @schema is null or (table_schema similar to @schema)   )
                and (   {GetSchemaExpression("table_schema")}  )
                and (   @skipSimilar is null or (table_name not similar to @skipSimilar)   )

            ", ("schema", settings.Schema, DbType.AnsiString), ("skipSimilar", skipSimilar, DbType.AnsiString)).Select(t => new PgItem
        {
            Schema = t.Schema,
            Name = t.Name,
            TypeName = t.Type,
            Type = t.Type switch
            {
                "BASE TABLE" => PgType.Table,
                "VIEW" => PgType.View,
                _ => PgType.Unknown
            }
        });
    }
}
