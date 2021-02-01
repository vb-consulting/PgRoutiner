using System;
using System.IO;

namespace PgRoutiner
{
    partial class Builder
    {
        public static void WriteFile(string path, string content)
        {
            if (Settings.Value.Dump)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(content);
                Console.ResetColor();
                return;
            }
            try
            {
                File.WriteAllText(path, content);
            }
            catch(Exception e)
            {
                Program.DumpError($"File {path} could not be written: {e.Message}");
            }
        }
    }
}
