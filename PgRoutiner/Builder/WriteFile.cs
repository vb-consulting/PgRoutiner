using System;
using System.IO;

namespace PgRoutiner
{
    partial class Builder
    {
        public static bool WriteFile(string path, string content)
        {
            if (Settings.Value.Dump)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(content);
                Console.ResetColor();
                return true;
            }
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(path, content);
                return true;
            }
            catch(Exception e)
            {
                Program.DumpError($"File {path} could not be written: {e.Message}");
                return false;
            }
        }
    }
}
