using System;
using System.IO;

namespace PgRoutiner
{
    partial class Builder
    {
        private static string GetModelDir()
        {
            var dir = Settings.Value.ModelDir;
            if (dir != null)
            {
                dir = Path.Combine(Program.CurrentDir, dir);
                if (!Directory.Exists(dir))
                {
                    Dump($"Creating dir: {dir}");
                    Directory.CreateDirectory(dir);
                }
            }
            return dir;
        }
    }
}
