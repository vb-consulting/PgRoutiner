using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
        private static bool ArgsInclude(string[] args, params string[] values)
        {
            var lower = values.Select(v => v.ToLower()).ToList();
            var upper = values.Select(v => v.ToUpper()).ToList();
            foreach (var arg in args)
            {
                if (lower.Contains(arg))
                {
                    return true;
                }
                if (upper.Contains(arg))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
