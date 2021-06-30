using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Norm;
using Npgsql;

namespace PgRoutiner
{
    public static partial class DataAccess
    {
        public static IEnumerable<PgItem> GetSequences(this NpgsqlConnection connection, Settings settings, string skipSimilar = null)
        {
            return connection.Read<(string Schema, string Name)>(@$"

                select
                    s.sequence_schema,
                    s.sequence_name
                from
                    information_schema.sequences s
                where
                    (   @schema is null or (s.sequence_schema similar to @schema)   )
                    and (   {GetSchemaExpression("s.sequence_schema")}  )
                    and (   @skipSimilar is null or (sequence_name not similar to @skipSimilar)   )

            ", ("schema", settings.Schema, DbType.AnsiString), ("skipSimilar", skipSimilar, DbType.AnsiString)).Select(t => new PgItem
            {
                Schema = t.Schema,
                Name = t.Name,
                Type = PgType.Sequence
            });
        }
    }
}
