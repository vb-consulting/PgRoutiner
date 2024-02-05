using System.Data;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<PgItem> GetExtensions(this NpgsqlConnection connection)
    {
        return connection
            .Read<string>([],"select extname from pg_extension", r => r.Val<string>(0))
            .Select(s => new PgItem
            {
                Schema = null,
                Name = s,
                TypeName = "EXTENSION",
                Type = PgType.Extension
            });

        //return connection
        //    .Read<string>("select extname from pg_extension")
        //    .Select(s => new PgItem
        //    {
        //        Schema = null,
        //        Name = s,
        //        TypeName = "EXTENSION",
        //        Type = PgType.Extension
        //    });
    }
}
