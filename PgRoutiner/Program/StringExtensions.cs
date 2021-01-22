using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PgRoutiner
{
    public static class StringExtensions
    {
        public static string ToUpperCamelCase(this string value) =>
            value.Split(new[] {"_"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => string.Concat(char.ToUpperInvariant(s[0]), s[1..]))
                .Aggregate(string.Empty, string.Concat);

        public static string ToCamelCase(this string value)
        {
            var result = value.ToUpperCamelCase();
            return string.Concat(result.First().ToString().ToLowerInvariant(), result[1..]);
        }

        public static string PathToNamespace(this string value)
        {
            if (value == "." || value == "..")
            {
                return "";
            }
            return string.Join(".", value
                .Replace("/", ".")
                .Replace("\\", ".")
                .Replace(":", ".")
                .Split(".")
                .Select(v => v.ToUpperCamelCase()))
                .TrimStart('.');
        }

        public static string FirstWordAfter(this string value, string word, char character = ' ')
        {
            var index = value.IndexOf(word);
            if (index == -1)
            {
                return null;
            }
            index = value.IndexOf(' ', index + word.Length);
            if (index == -1)
            {
                return null;
            }
            index++;
            var lastindex = value.IndexOf(character, index);
            int len;
            if (lastindex == -1)
            {
                len = value.Length - index;
            }
            else
            {
                len = lastindex - index;
            }
            return value.Substring(index, len).Trim();
        }

        public static string Between(this string value, char start, char end)
        {
            var index = value.IndexOf(start);
            if (index == -1)
            {
                return null;
            }
            index++;
            var lastindex = value.IndexOf(end, index);
            int len;
            if (lastindex == -1)
            {
                len = value.Length - index;
            }
            else
            {
                len = lastindex - index;
            }
            return value.Substring(index, len).Trim();
        }
    }
}
