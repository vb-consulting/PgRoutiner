namespace PgRoutiner.Builder
{
    static class Process
    {
        public static void Run(string file, string args = null, string dir = null, bool writeCommand = true)
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = file;
            if (args != null)
            {
                process.StartInfo.Arguments = args;
            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WorkingDirectory = dir ?? Program.CurrentDir;
            process.OutputDataReceived += (sender, data) => Program.WriteLine(data.Data);
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (sender, data) =>
            {
                if (!string.IsNullOrEmpty(data.Data))
                {
                    Program.WriteLine(ConsoleColor.Red, data.Data);
                    Console.ResetColor();
                }
            };
            if (writeCommand)
            {
                Program.WriteLine($"{file} {args ?? ""}");
            }
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();
        }
    }
}
