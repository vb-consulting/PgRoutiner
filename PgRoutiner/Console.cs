using System;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace PgRoutiner;

static partial class Program
{
    public static void WriteLine(ConsoleColor? color, params string[] lines)
    {
        if (Current.Value.Silent)
        {
            return;
        }
        if (color.HasValue)
        {
            Console.ForegroundColor = color.Value;
        }
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        if (color.HasValue)
        {
            Console.ResetColor();
        }
    }

    public static void Write(ConsoleColor? color, string line)
    {
        if (Current.Value.Silent)
        {
            return;
        }
        if (color.HasValue)
        {
            Console.ForegroundColor = color.Value;
        }
        Console.Write(line);
        if (color.HasValue)
        {
            Console.ResetColor();
        }
    }

    public static void Write(string line)
    {
        if (Current.Value.Silent)
        {
            return;
        }
        Write(null, line);
    }

    public static void WriteLine(params string[] lines)
    {
        if (Current.Value.Silent)
        {
            return;
        }
        WriteLine(null, lines);
    }

    public static void DumpError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine($"ERROR: {msg}");
        Console.ResetColor();
    }

    public static ConsoleKey Ask(string message, params ConsoleKey[] answers)
    {
        if (!string.IsNullOrEmpty(message))
        {
            WriteLine(ConsoleColor.Yellow, message);
        }
        ConsoleKey answer;
        do
        {
            answer = Console.ReadKey(false).Key;
        }
        while (!answers.Contains(answer));
        WriteLine("");
        return answer;
    }

    private static List<FieldInfo> _fields = typeof(Current).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.Name.EndsWith("Args")).ToList();
    public static Dictionary<string, Arg> ArgsDict = _fields.Select(f => (Arg)f.GetValue(null)).ToDictionary(a => a.Original, a => a);

    public static Current BindConsole(string[] args)
    {
        var settings = new Current();
        new ConfigurationBuilder().AddCommandLine(args).Build().Bind(settings);
        var hashes = args.ToHashSet();
        foreach (var field in _fields)
        {
            var arg = (Arg)field.GetValue(null);
            if (hashes.Contains(arg.Alias))
            {
                var prop = settings.GetType().GetProperty(arg.Original);
                if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(settings, true);
                }
            }
        }
        return settings;
    }

    //[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_count")]
    //private static extern ref int GetCountField(Current settings);

    public static string[] ParseArgs(string[] rawArgs)
    {
        string[] args = new string[rawArgs.Length];
        int i = 0;
        foreach(var arg in rawArgs)
        {
            if (/*(i+1 % 2 == 1) && */Arg.ArgReplacements.TryGetValue(arg, out var replacement))
            {
                args[i++] = replacement;
            }
            else
            {
                args[i++] = arg;
            }
        }

        List<string> result = new();
        var allowed = new HashSet<string>();
        var props = typeof(Current).GetProperties();
        //var fields = _fields;
        var argsList = new List<Arg>();

        foreach (var field in _fields)
        {
            var arg = (Arg)field.GetValue(null);
            allowed.Add(arg.Alias.ToLower());
            allowed.Add(string.Concat("--", arg.Original.ToLower()));
            argsList.Add(arg);
        }
        
        foreach (var prop in props)
        {
            allowed.Add(string.Concat("--", prop.Name.ToLower()));
        }

        foreach (var (arg, index) in args.Select((a,i) => (a,i)))
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
                var propName = name.Replace("-", "").ToLowerInvariant();
                var prop = props.Where(p => string.Equals(p.Name.ToLowerInvariant(), propName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                bool isBool = false;
                if (prop != null)
                {
                    isBool = prop.PropertyType == typeof(bool);
                }
                else
                {
                    var argObj = argsList.Where(a => string.Equals(a.Alias, name)).FirstOrDefault();
                    if (argObj != null)
                    {
                        prop = props.Where(p => string.Equals(p.Name.ToLowerInvariant(), argObj.Original, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (prop != null)
                        {
                            isBool = prop.PropertyType == typeof(bool);
                        }
                    }
                }
                if (to == -1)
                {
                    if (!isBool)
                    {
                        result.Add(name);
                        var next = index + 1 < args.Length ? args[index + 1] : "-";
                        if (next.StartsWith("-"))
                        {
                            result.Add("");
                        }
                    }
                    else
                    {
                        string next = null;
                        if (index < args.Length - 1)
                        {
                            next = args[index+1];
                        }
                        if (next == null || next.StartsWith("-"))
                        {
                            result.Add(name);
                            result.Add("true");
                        }
                        else
                        {
                            result.Add(name);
                        }
                    }
                }
                else
                {
                    var v = arg.Substring(to + 1);
                    result.Add(name);
                    if (v == "0")
                    {
                        result.Add("false");
                    }
                    else if (v == "1")
                    {
                        result.Add("true");
                    }
                    else
                    {
                        result.Add(v);
                    }
                }

            }
            else
            {
                if (arg == "0")
                {
                    result.Add("false");
                }
                else if (arg == "1")
                {
                    result.Add("true");
                }
                else
                {
                    result.Add(arg);
                }
            }
        }
        return result.ToArray();
    }

    //
    // Write method to highlight text in console
    //
    public static void Highlight(string text, string search, ConsoleColor color = ConsoleColor.Cyan, ConsoleColor highlight = ConsoleColor.Yellow)
    {
        Console.ForegroundColor = color;
        while(true)
        {
            var index = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                Console.Write(text);
                break;
            }
            Console.Write(text.Substring(0, index));
            Console.ForegroundColor = highlight;
            Console.Write(text.Substring(index, search.Length));
            Console.ForegroundColor = color;
            text = text.Substring(index + search.Length);
        }
        Console.ResetColor();
    }
}
