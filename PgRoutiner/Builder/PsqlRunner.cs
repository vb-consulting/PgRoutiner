﻿using System.Diagnostics;
using Npgsql;
using System.Reflection;

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

        public void RunFromTerminal()
        {
            using var process = new Process();
            process.StartInfo.FileName = settings.PsqlTerminal;
            process.StartInfo.Arguments = $"{settings.PsqlCommand} {baseArg} {(settings.PsqlOptions ?? "")}";
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            process.Start();
        }
    }
}
