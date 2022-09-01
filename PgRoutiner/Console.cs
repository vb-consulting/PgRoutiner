using System.Reflection;

namespace PgRoutiner;

static partial class Program
{
    public static void WriteLine(ConsoleColor? color, params string[] lines)
    {
        if (Settings.Value.Silent)
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
        if (Settings.Value.Silent)
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
        if (Settings.Value.Silent)
        {
            return;
        }
        Write(null, line);
    }

    public static void WriteLine(params string[] lines)
    {
        if (Settings.Value.Silent)
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

    public static bool ArgsInclude(string[] args, Arg value)
    {
        foreach (var arg in args)
        {
            var lower = arg.ToLower();
            if (lower.Contains("="))
            {
                var left = lower.Split('=', 2, StringSplitOptions.RemoveEmptyEntries).First();
                if (string.Equals(left, value.Alias) || string.Equals(left, value.Name))
                {
                    return true;
                }
            }
            else
            {
                if (string.Equals(lower, value.Alias) || string.Equals(lower, value.Name))
                {
                    return true;
                }
            }
        }
        return false;
    }

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
        foreach (var prop in typeof(Settings).GetProperties())
        {
            allowed.Add(string.Concat("--", prop.Name.ToLower()));
        }

        foreach (var arg in args)
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
