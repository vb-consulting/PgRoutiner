using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class Return
    {
        public bool IsInstance { get; init; }
        public bool IsVoid { get; init; }
        public string Name { get; init; }
        public string PgName { get; init; }
    }

    public class Param
    {
        public string DbType { get; init; }
        public string Name => PgName.ToCamelCase();
        public string ClassName => PgName.ToUpperCamelCase();
        public string PgName { get; init; }
        public string PgType { get; init; }
        public string Type { get; init; }
        public bool IsInstance { get; init; } = false;
    }

    public class Method
    {
        public string ActualReturns { get; init; }
        public string Name { get; init; }
        public string Namespace { get; init; }
        public List<Param> Params { get; init; }
        public Return Returns { get; init; }
        public bool Sync { get; init; }
    }
}
