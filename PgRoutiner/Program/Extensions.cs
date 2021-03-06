﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;

namespace PgRoutiner
{
    public static class Extensions
    {
        public static string ToUpperCamelCase(this string value)
        {
            if (value is null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return string.Empty;
            }
            return value.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => string.Concat(char.ToUpperInvariant(s[0]), s[1..]))
                .Aggregate(string.Empty, string.Concat);
        }

        public static string ToCamelCase(this string value)
        {
            if (value is null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return string.Empty;
            }
            var result = value.ToUpperCamelCase();
            return string.Concat(result.First().ToString().ToLowerInvariant(), result[1..]);
        }

        public static string PathToNamespace(this string value)
        {
            if (value is null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return string.Empty;
            }
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

        public static string FirstWordAfter(this string value, string word, char? character = ' ')
        {
            if (value is null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return string.Empty;
            }
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
            if (character.HasValue)
            {
                var lastindex = value.IndexOf(character.Value, index);
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
            return value.Substring(index).Trim();
        }

        public static string Between(this string value, char start, char end)
        {
            if (value is null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return string.Empty;
            }
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

        public static string ToKebabCase(this string value)
        {
            if (value is null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new();
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (char.IsLower(ch) || ch == '-')
                {
                    builder.Append(ch);
                }
                else if (i == 0) 
                {
                    builder.Append(char.ToLower(ch));
                }
                else if (char.IsDigit(ch) && !char.IsDigit(value[i - 1]))
                {
                    builder.Append('-');
                    builder.Append(ch);
                }
                else if (char.IsDigit(ch))
                {
                    builder.Append(ch);
                }
                else if (char.IsLower(value[i - 1]))
                {
                    builder.Append('-');
                    builder.Append(char.ToLower(ch));
                }
                else if (i + 1 == value.Length || char.IsUpper(value[i + 1]))
                {
                    builder.Append(char.ToLower(ch));
                }
                else
                {
                    builder.Append('-');
                    builder.Append(char.ToLower(ch));
                }
            }
            return builder.ToString();
        }

        public static string SanitazeName(this string name, string allowed = "_", string replacement = "_")
        {
            StringBuilder sb = new();
            foreach(var ch in name)
            {
                if (char.IsLetterOrDigit(ch) || allowed.Contains(ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append(replacement);
                }
            }
            return sb.ToString();
        }

        public static string SanitazePath(this string name, string replacement = "_")
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidRegStr, replacement);
        }

        public static string GetRelativePath(this string path)
        {
            return Path.GetRelativePath(Program.CurrentDir, path).Replace("\\", "/");
        }

        public static bool IsUniqueStatemnt(this string value)
        {
            return 
                value.Contains("PRIMARY") || 
                value.Contains("UNIQUE") ||
                value.Contains("primary") ||
                value.Contains("unique");
        }

        public static string GetSequence(this string value, string sequence = "$")
        {
            var index = value.IndexOf(sequence);
            if (index > -1)
            {
                var endIndex = value.IndexOf(sequence, index + 1);
                if (endIndex > -1)
                {
                    return value.Substring(index, endIndex - index + 1);
                }
                return value.Substring(index);
            }
            return null;
        }

        public static string ToPsqlFormatString(this NpgsqlConnection connection)
        {
            var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
            return $"postgresql://{connection.UserName}:{password}@{connection.Host}:{connection.Port}/{connection.Database}";
        }

        public static bool PathEquals(this string path1, string path2)
        {
            return Path
                .GetFullPath(path1).TrimEnd('/').TrimEnd('\\')
                .Equals(Path.GetFullPath(path2).TrimEnd('/').TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase);
            ;
        }
    }
}
