using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetSequences(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
    {
        return connection
            .WithParameters(new
            {
                schema = (settings.SchemaSimilarTo, DbType.AnsiString),
                not_schema = (settings.SchemaNotSimilarTo, DbType.AnsiString),
                skipSimilar = (skipSimilar, DbType.AnsiString),
            })
            .Read<(string Schema, string Name)>(@$"

            select
                s.sequence_schema,
                s.sequence_name
            from
                information_schema.sequences s
            where
                (   @schema is null or (s.sequence_schema similar to @schema)   )
                and (   @not_schema is null or s.sequence_schema not similar to @not_schema   )
                and (   {GetSchemaExpression("s.sequence_schema")}  )
                and (   @skipSimilar is null or (sequence_name not similar to @skipSimilar)   )

        ")
        .Select(t => new PgItem
        {
            Schema = t.Schema,
            Name = t.Name,
            Type = PgType.Sequence
        });
    }
}
