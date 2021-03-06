﻿using System;
using System.Linq;

namespace PgRoutiner
{
    static partial class Program
    {
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

        public static string ReadLine(string prompt, params string[] messages)
        {
            foreach(var message in messages)
            {
                WriteLine(ConsoleColor.Yellow, message);
            }
            if (prompt != null)
            {
                Write(ConsoleColor.Yellow, prompt);
            }
            var result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            return result;
        }
    }
}
