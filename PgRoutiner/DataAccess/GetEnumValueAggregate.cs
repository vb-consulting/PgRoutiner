﻿using System.Data;
using Norm;

namespace PgRoutiner.DataAccess;

public static partial class DataAccessConnectionExtensions
{
    public static string GetEnumValueAggregate(this NpgsqlConnection connection, string schema, string name)
    {
        return connection
            .WithParameters(
                (schema, DbType.AnsiString),
                (name, DbType.AnsiString))
            .Read<string>(@$"

            select
                string_agg('''' || e.enumlabel || '''', ', ' order by e.enumsortorder)
            from
                pg_type t
                inner join pg_enum e on t.oid = e.enumtypid
                inner join pg_namespace ns on t.typnamespace = ns.oid
            where
                ns.nspname = $1 and t.typname = $2;

            ").FirstOrDefault();
    }
}
