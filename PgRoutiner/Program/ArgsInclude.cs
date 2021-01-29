using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
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
    }
}
