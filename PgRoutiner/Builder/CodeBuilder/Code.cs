using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public abstract class Code
    {
        protected string I1 => string.Join("", Enumerable.Repeat(" ", settings.Ident));
        protected string I2 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 2));
        protected string I3 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 3));
        protected string I4 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 4));
        protected string I5 => string.Join("", Enumerable.Repeat(" ", settings.Ident * 5));
        protected readonly string NL = Environment.NewLine;
        protected readonly Settings settings;

        public string Name { get; }
        public Dictionary<string, StringBuilder> Models { get; private set; } = new();
        public HashSet<string> UserDefinedModels { get; private set; } = new();
        public Dictionary<string, StringBuilder> ModelContent { get; private set; } = new();
        public StringBuilder Class { get; } = new();
        public List<Method> Methods { get; } = new();
        public string ModuleNamespace { get; private set; }

        public Code(Settings settings, string name)
        {
            this.settings = settings;
            Name = name;
        }

        protected bool TryGetParamMapping(PgParameter p, out string value)
        {
            if (settings.Mapping.TryGetValue(p.Type, out value))
            {
                return true;
            }
            return settings.Mapping.TryGetValue(p.DataType, out value);
        }

        protected string GetParamType(PgParameter p)
        {
            if (TryGetParamMapping(p, out var result))
            {
                if (p.IsArray)
                {
                    return $"{result}[]";
                }
                if (result != "string")
                {
                    return $"{result}?";
                }
                return result;
            }
            throw new ArgumentException($"Could not find mapping \"{p.DataType}\" for parameter of routine  \"{this.Name}\"");
        }

        protected string GetParamDbType(PgParameter p)
        {
            var type = "NpgsqlDbType.";
            if (ParamTypeMapping.TryGetValue(p.Type, out var map))
            {
                type = string.Concat(type, map.Name);
                if (p.IsArray)
                {
                    type = string.Concat("NpgsqlDbType.Array | ", type);
                }
                if (map.IsRange)
                {
                    type = string.Concat("NpgsqlDbType.Range | ", type);
                }
            }
            else
            {
                type = string.Concat(type, "Unknown");
            }
            return type;
        }

        protected static readonly Dictionary<string, (string Name, bool IsRange)> ParamTypeMapping = new()
        {
            { "refcursor", ("Refcursor", false) },
            { "tsvector", ("TsVector", false) },
            { "cidr", ("Cidr", false) },
            { "timestamptz", ("TimestampTz", false) },
            { "name", ("Name", false) },
            { "inet", ("Inet", false) },
            { "lseg", ("Lseg", false) },
            { "int8", ("Bigint", false) },
            { "_char", ("Char", false) },
            { "unknown", ("Unknown", false) },
            { "tsquery", ("TsQuery", false) },
            { "float4", ("Real", false) },
            { "timestamp", ("Timestamp", false) },
            { "gtsvector", ("TsVector", false) },
            { "circle", ("Circle", false) },
            { "numeric", ("Numeric", false) },
            { "pg_type", ("Regtype", false) },
            { "regconfig", ("Regconfig", false) },
            { "timetz", ("TimeTZ", false) },
            { "daterange", ("Date", true) },
            { "box", ("Box", false) },
            { "_float4", ("Real", false) },
            { "int4range", ("Integer", true) },
            { "cid", ("Cid", false) },
            { "_regtype", ("Regtype", false) },
            { "_varchar", ("Varchar", false) },
            { "_text", ("Text", false) },
            { "date", ("Date", false) },
            { "xid", ("Xid", false) },
            { "bool", ("Boolean", false) },
            { "_oid", ("Oid", false) },
            { "polygon", ("Polygon", false) },
            { "time", ("Time", false) },
            { "int2vector", ("Int2Vector", false) },
            { "_int4", ("Integer", false) },
            { "int4", ("Integer", false) },
            { "_interval", ("Interval", false) },
            { "_int8", ("Bigint", false) },
            { "int8range", ("Bigint", true) },
            { "interval", ("Interval", false) },
            { "xml", ("Xml", false) },
            { "char", ("Char", false) },
            { "macaddr8", ("MacAddr8", false) },
            { "varchar", ("Varchar", false) },
            { "float8", ("Double", false) },
            { "json", ("Json", false) },
            { "_name", ("Name", false) },
            { "money", ("Money", false) },
            { "text", ("Text", false) },
            { "_float8", ("Double", false) },
            { "regtype", ("Regtype", false) },
            { "bit", ("Bit", false) },
            { "tid", ("Tid", false) },
            { "line", ("Line", false) },
            { "oidvector", ("Oidvector", false) },
            { "int2", ("Smallint", false) },
            { "uuid", ("Uuid", false) },
            { "path", ("Path", false) },
            { "jsonb", ("Jsonb", false) },
            { "bytea", ("Bytea", false) },
            { "_bool", ("Boolean", false) },
            { "macaddr", ("MacAddr", false) },
            { "point", ("Point", false) },
            { "varbit", ("Varbit", false) },
            { "oid", ("Oid", false) },
            { "_int2", ("Smallint", false) },
            { "character", ("Char", false) },
            { "bpchar", ("Char", false) }
        };
    }
}
