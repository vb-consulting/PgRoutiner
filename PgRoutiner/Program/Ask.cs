using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRoutiner
{
    static partial class Program
    {
        public static ConsoleKey Ask(string message, params ConsoleKey[] answers)
        {
            WriteLine(ConsoleColor.Yellow, message);
            ConsoleKey answer;
            do
            {
                answer = Console.ReadKey(false).Key;
            }
            while (!answers.Contains(answer));
            WriteLine("");
            return answer;
        }
    }
}
