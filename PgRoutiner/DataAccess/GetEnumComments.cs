using System.Data;
using PgRoutiner.DataAccess.Models;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<EnumComment> GetEnumComments(this NpgsqlConnection connection, Current settings, string schema)
    {
        return connection.Read<EnumComment>(
        [
            (schema, DbType.AnsiString, null),
            (settings.MdNotSimilarTo, DbType.AnsiString, null),
            (settings.MdSimilarTo, DbType.AnsiString, null)
        ],
        @$"
        select
            t.typname as name,
            pgdesc.description as comment,
            string_agg('''' || e.enumlabel || '''', ', ' order by e.enumsortorder) as values
        from
            pg_type t
            inner join pg_enum e on t.oid = e.enumtypid
            inner join pg_namespace ns on t.typnamespace = ns.oid
            left outer join pg_catalog.pg_description pgdesc on t.oid = pgdesc.objoid
        where
            ns.nspname = $1
            and ($2 is null or t.typname not similar to $2)
            and ($3 is null or t.typname similar to $3)
        group by
            t.typname,
            pgdesc.description;
        ", r => new EnumComment
        {
            Name = r.Val<string>(0),
            Comment = r.Val<string>(1),
            Values = r.Val<string>(2)
        });


        //return connection
        //    .WithParameters(
        //        (schema, DbType.AnsiString),
        //        (settings.MdNotSimilarTo, DbType.AnsiString),
        //        (settings.MdSimilarTo, DbType.AnsiString))
        //    .Read<EnumComment>(@$"

        //        select
        //            t.typname as name,
        //            pgdesc.description as comment,
        //            string_agg('''' || e.enumlabel || '''', ', ' order by e.enumsortorder) as values
        //        from
        //            pg_type t
        //            inner join pg_enum e on t.oid = e.enumtypid
        //            inner join pg_namespace ns on t.typnamespace = ns.oid
        //            left outer join pg_catalog.pg_description pgdesc on t.oid = pgdesc.objoid
        //        where
        //            ns.nspname = $1
        //            and ($2 is null or t.typname not similar to $2)
        //            and ($3 is null or t.typname similar to $3)
        //        group by
        //            t.typname,
        //            pgdesc.description;

        //    ");
    }
}
