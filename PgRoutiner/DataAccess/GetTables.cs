using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetTables(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
    {
        return connection
            .WithParameters(
                (settings.SchemaSimilarTo, DbType.AnsiString),
                (settings.SchemaNotSimilarTo, DbType.AnsiString),
                (skipSimilar, DbType.AnsiString))
            .Read<(string Schema, string Name, string Type)>(@$"

        select 
            t.table_schema, t.table_name, t.table_type
        from 
            information_schema.tables t
        where
            (   $1 is null or (table_schema similar to $1)   )
            and (   $2 is null or (table_schema not similar to $2)   )
            and (   {GetSchemaExpression("table_schema")}  )
            and (   $3 is null or (table_name not similar to $3)   )
            and not exists (

                select
                    1
                from 
                    pg_inherits
                    inner join pg_class parent on pg_inherits.inhparent = parent.oid
                    inner join pg_class child on pg_inherits.inhrelid = child.oid
                    inner join pg_namespace nmsp_parent on nmsp_parent.oid = parent.relnamespace
                    inner join pg_namespace nmsp_child on nmsp_child.oid = child.relnamespace
                where
                     nmsp_child.nspname = t.table_schema and child.relname = t.table_name
            )
        ")
        .Select(t => new PgItem
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
