using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace PgRoutiner
{
    public record Return(string PgName, string Name, bool IsVoid, bool IsInstance);
    
    public record Param(string PgName, string Name, string PgType, string Type, string DbType);
    
    public record Method(string Name, string Namespace, List<Param> Params, Return Returns, string ActualReturns, bool Sync);
}
