using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public class ExtensionMethods
    {
        public List<Method> Methods { get; set; }
        public string Namespace { get; set; }
        public string ModelNamespace { get; set; }
        public string Name { get; set; }
    }

    public class Return
    {
        public bool IsEnumerable { get; init; }
        public bool IsVoid { get; init; }
        public string Name { get; init; }
        public string PgName { get; init; }
    }

    public class Param
    {
        private readonly Settings settings;

        public string DbType { get; init; }
        public string Name
        {
            get
            {
                var name = PgName.ToCamelCase();
                if (settings.MethodParameterNames.TryGetValue(name, out var result))
                {
                    return result;
                }
                return name;
            }
        }

        public string ClassName
        {
            get
            {
                var name = PgName.ToUpperCamelCase();
                if (settings.MethodParameterNames.TryGetValue(name, out var result))
                {
                    return result;
                }
                return name;
            }
        }

        public string PgName { get; init; }
        public string PgType { get; init; }
        public string Type { get; init; }
        public bool IsInstance { get; init; } = false;

        public Param(Settings settings)
        {
            this.settings = settings;
        }
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
