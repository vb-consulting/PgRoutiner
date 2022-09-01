using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetSequences(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
    {
        return connection
            .WithParameters(
                (settings.SchemaSimilarTo, DbType.AnsiString),
                (settings.SchemaNotSimilarTo, DbType.AnsiString),
                (skipSimilar, DbType.AnsiString))
            .Read<(string Schema, string Name)>(@$"

            select
                s.sequence_schema,
                s.sequence_name
            from
                information_schema.sequences s
            where
                (   $1 is null or (s.sequence_schema similar to $1)   )
                and (   $2 is null or s.sequence_schema not similar to $2   )
                and (   {GetSchemaExpression("s.sequence_schema")}  )
                and (   $3 is null or (sequence_name not similar to $3)   )

        ")
        .Select(t => new PgItem
        {
            Schema = t.Schema,
            Name = t.Name,
            TypeName = "SEQ",
            Type = PgType.Sequence
        });
    }
}
