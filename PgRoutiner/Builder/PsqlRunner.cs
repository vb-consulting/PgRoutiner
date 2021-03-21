using System.Diagnostics;
using Npgsql;
using System.Reflection;
using System;

namespace PgRoutiner
{
    public class PsqlRunner
    {
        private readonly Settings settings;
        private readonly string baseArg;

        public PsqlRunner(Settings settings, NpgsqlConnection connection)
        {
            this.settings = settings;
            baseArg = $"-d {connection.ToPsqlFormatString()}";
        }

        public void Run(string args)
        {
            Program.RunProcess(settings.PsqlCommand, $"{baseArg} {(args ?? "")}", writeCommand: false);
        }

        public void TryRunFromTerminal()
        {
            if (settings.PsqlCommand == null)
            {
                RunAsShellExecute();
                return;
            }

            try
            {
                using var process = new Process();
                process.StartInfo.FileName = settings.PsqlTerminal;
                process.StartInfo.Arguments = $"{settings.PsqlCommand} {baseArg} {(settings.PsqlOptions ?? "")}";
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.Start();
            }
            catch
            {
                RunAsShellExecute();
            }
        }

        public void RunAsShellExecute()
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = settings.PsqlCommand;
                process.StartInfo.Arguments = $"{baseArg} {(settings.PsqlOptions ?? "")}";
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.Start();
            }
            catch (Exception e)
            {
                Program.DumpError(e.Message);
            }
        }
    }
}
