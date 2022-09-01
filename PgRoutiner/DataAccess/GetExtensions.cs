using System.Data;
using Norm;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetExtensions(this NpgsqlConnection connection)
    {
        return connection
            .Read<string>("select extname from pg_extension")
            .Select(s => new PgItem
            {
                Schema = null,
                Name = s,
                TypeName = "EXTENSION",
                Type = PgType.Extension
            });
    }
}
