using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace PgRoutiner
{
    public record Return(string PgName, string Name, bool IsVoid, bool IsInstance);

    public class Param
    {
        public string DbType { get; init; }
        public string Name => PgName.ToCamelCase();
        public string ClassName => PgName.ToUpperCamelCase();
        public string PgName { get; init; }
        public string PgType { get; init; }
        public string Type { get; init; }
    }

    public record Method(string Name, string Namespace, List<Param> Params, Return Returns, string ActualReturns, bool Sync);
}
