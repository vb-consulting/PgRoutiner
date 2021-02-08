using System.Linq;
using System.Data;
using Norm;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;

namespace PgRoutiner
{
    public enum PgConstraint
    {
        ForeignKey,
        PrimaryKey,
        Check,
        Unique
    }

    public class ConstraintName
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Name { get; set; }
        public PgConstraint Type { get; set; }
    }

    public static partial class DataAccess
    {
        public static IEnumerable<ConstraintName> GetConstraintNames(this NpgsqlConnection connection, (string Schema, string Name)[] tables, PgConstraint type) =>
            connection.Read<(string Schema, string Table, string Name, string Type)>(@"

            select 
                table_schema, table_name, constraint_name, constraint_type
            from 
                information_schema.table_constraints
            where 
                table_schema || '.' || table_name = any(@tables)
                and constraint_type = @type

            ",
                ("tables", tables.Select(t => $"{t.Schema}.{t.Name}").ToArray(), NpgsqlDbType.Varchar | NpgsqlDbType.Array),
                ("type", type switch
                {
                    PgConstraint.ForeignKey => "FOREIGN KEY",
                    PgConstraint.PrimaryKey => "PRIMARY KEY",
                    PgConstraint.Check => "CHECK",
                    PgConstraint.Unique => "UNIQUE",
                    _ => throw new System.NotImplementedException()
                }, NpgsqlDbType.Varchar))
            .Select(t => new ConstraintName
            {
                Name = t.Name,
                Schema = t.Schema,
                Table = t.Table,
                Type = t.Type switch
                {
                    "FOREIGN KEY" => PgConstraint.ForeignKey,
                    "PRIMARY KEY" => PgConstraint.PrimaryKey,
                    "CHECK" => PgConstraint.Check,
                    "UNIQUE" => PgConstraint.Unique,
                    _ => throw new System.NotImplementedException()
                }
            });
    }
}
