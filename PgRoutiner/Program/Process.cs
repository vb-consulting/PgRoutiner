using System.Diagnostics;

namespace PgRoutiner
{
    static partial class Program
    {
        public static void RunProcess(string file, string args = null, string dir = null)
        {
            using var process = new Process();
            process.StartInfo.FileName = file;
            if (args != null)
            {
                process.StartInfo.Arguments = args;
            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WorkingDirectory = dir ?? CurrentDir;
            process.OutputDataReceived += (sender, data) => WriteLine(data.Data);
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += (sender, data) => WriteLine(data.Data);
            WriteLine($"{file}{args ?? ""}");
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();
        }
    }
}
