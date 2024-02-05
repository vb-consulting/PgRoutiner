namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static IEnumerable<(string schema, string name, string[] values, string comment)> GetAllEnums(this NpgsqlConnection connection)
    {

        return connection.Read<(string schema, string name, string[] values, string comment)>(
        [
        ],
    @$"
            select
                ns.nspname,
                t.typname as name,
                array_agg(e.enumlabel order by e.enumsortorder) as values,
                pgdesc.description as comment
            from
                pg_type t
                inner join pg_enum e on t.oid = e.enumtypid
                inner join pg_namespace ns on t.typnamespace = ns.oid
                left outer join pg_catalog.pg_description pgdesc on t.oid = pgdesc.objoid
            group by
                ns.nspname,
                t.typname,
                pgdesc.description
            ",
    r => (r.Val<string>(0), r.Val<string>(1), r.Val<string[]>(2), r.Val<string>(3)));

        //return connection.Read<(string schema, string name, string[] values, string comment)>(@$"

        //        select
        //            ns.nspname,
        //            t.typname as name,
        //            array_agg(e.enumlabel order by e.enumsortorder) as values,
        //            pgdesc.description as comment
        //        from
        //            pg_type t
        //            inner join pg_enum e on t.oid = e.enumtypid
        //            inner join pg_namespace ns on t.typnamespace = ns.oid
        //            left outer join pg_catalog.pg_description pgdesc on t.oid = pgdesc.objoid
        //        group by
        //            ns.nspname,
        //            t.typname,
        //            pgdesc.description

        //    ");
    }
}
