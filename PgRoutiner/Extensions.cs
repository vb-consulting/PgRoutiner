using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public static class Extensions
    {
        public static string ToUpperCamelCase(this string value) =>
            value.Split(new[] {"_"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => string.Concat(char.ToUpperInvariant(s[0]), s[1..]))
                .Aggregate(string.Empty, string.Concat);

        public static string ToCamelCase(this string value)
        {
            var result = value.ToUpperCamelCase();
            return string.Concat(result.First().ToString().ToLowerInvariant(), result.Substring(1));
        }
    }
}
