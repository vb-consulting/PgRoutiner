using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PgRoutiner
{
    static partial class Program
    {
        public static string[] ParseArgs(string[] args)
        {
            List<string> result = new();
            List<string> list = new();
            void Add(Arg value)
            {
                list.Add(value.Alias.TrimStart('-'));
                list.Add(value.Original.ToLower());
            }
            foreach(var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.Name.EndsWith("Args"))
                {
                    continue;
                }
                Add((Arg)field.GetValue(null));
            }
            var allowed = list.ToHashSet();
            foreach(var prop in typeof(Settings).GetProperties())
            {
                allowed.Add(prop.Name.ToLower());
            }

            foreach(var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    string name = arg.TrimStart('-');
                    if (name.Contains("="))
                    {
                        name = name.Split("=").First().ToLower();
                    }
                    if (name.Contains(":"))
                    {
                        name = name.Split(":").First().ToLower();
                    }
                    name = name.Replace("-", "");
                    if (!allowed.Contains(name))
                    {
                        DumpError($"Argument {arg} not recognized!");
                        WriteLine("Try \"pgroutiner --help\" for more information.");
                        return null;
                    }
                    var from = arg.StartsWith("--") ? 2 : 1;
                    var to = arg.IndexOfAny(new[] { ':', '=' });
                    result.Add(string.Concat(arg.Substring(0, from), name, arg.Substring(to == -1 ? arg.Length : to)));
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
