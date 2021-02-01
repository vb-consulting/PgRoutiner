using System.Diagnostics;
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
            var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
            baseArg = $"-d postgresql://{connection.UserName}:{password}@{connection.Host}:{connection.Port}/{connection.Database}";
        }

        public void Run()
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
