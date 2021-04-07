using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PgRoutiner
{
    static partial class Program
    {
        public static string[] ParseArgs(string[] args)
        {
            List<string> result = new();
            var allowed = new HashSet<string>();

            foreach (var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.Name.EndsWith("Args"))
                {
                    continue;
                }
                var arg = (Arg)field.GetValue(null);
                allowed.Add(arg.Alias.ToLower());
                allowed.Add(string.Concat("--", arg.Original.ToLower()));
            }
            foreach(var prop in typeof(Settings).GetProperties())
            {
                allowed.Add(string.Concat("--", prop.Name.ToLower()));
            }

            foreach(var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    string name = arg;
                    string[] split = null;
                    if (name.Contains("="))
                    {
                        name = name.Split("=").First().ToLower();
                    }
                    if (name.Contains(":"))
                    {
                        split = name.Split(":");
                        name = split.First().ToLower();
                    }
                    string prefix = "";
                    if (name.StartsWith("--"))
                    {
                        prefix = "--";
                    }
                    else if (name.StartsWith("-"))
                    {
                        prefix = "-";
                    }
                    name = string.Concat(prefix, name.Replace("-", ""));
                    if (!allowed.Contains(name))
                    {
                        DumpError($"Argument {arg} not recognized!");
                        WriteLine("Try \"pgroutiner --help\" for more information.");
                        return null;
                    }

                    if (split != null)
                    {
                        name = $"{name}:{split[1]}";
                    }
                    var to = arg.IndexOfAny(new[] { '=' });
                    if (to == -1)
                    {
                        result.Add(name);
                    }
                    else
                    {
                        result.Add(name);
                        result.Add(arg.Substring(to + 1));
                    }

                }
                else
                {
                    result.Add(arg);
                }
            }
            return result.ToArray();
        }
    }
}
