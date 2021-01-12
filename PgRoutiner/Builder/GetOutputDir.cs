using System;
using System.IO;

namespace PgRoutiner
{
    partial class Builder
    {
        private static string GetOutputDir()
        {
            var dir = Path.Combine(Program.CurrentDir, Settings.Value.OutputDir);
            if (!Directory.Exists(dir))
            {
                Dump($"Creating dir: {dir}");
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
    }
}
