using System.Reflection;

namespace PgRoutiner.Builder;

public class PsqlRunner
{
    private readonly Settings settings;
    private readonly NpgsqlConnection connection;
    private readonly string baseArg;
    private readonly string command;

    public PsqlRunner(Settings settings, NpgsqlConnection connection)
    {
        this.settings = settings;
        this.connection = connection;
        //baseArg = $"-d {connection.ToPsqlFormatString()}";
        baseArg = $"-h {connection.Host} -p {connection.Port} -U {connection.UserName} {connection.Database}";
        command = ParsePsqlCommand();
    }

    public void Run(string args)
    {
        Process.Run(command, $"{baseArg} {args ?? ""}", writeCommand: false);
    }

    public void TryRunFromTerminal()
    {
        if (command == null)
        {
            RunAsShellExecute();
            return;
        }

        var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
        Environment.SetEnvironmentVariable("PGPASSWORD", password);
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = settings.PsqlTerminal;
            process.StartInfo.Arguments = $"{command} {baseArg} {settings.PsqlOptions ?? ""}";
            if (settings.DumpPgCommands)
            {
                Program.WriteLine(ConsoleColor.White, $"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            }
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
        var password = typeof(NpgsqlConnection).GetProperty("Password", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection) as string;
        Environment.SetEnvironmentVariable("PGPASSWORD", password);
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = $"{baseArg} {settings.PsqlOptions ?? ""}";
            if (settings.DumpPgCommands)
            {
                Program.WriteLine(ConsoleColor.White, $"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            }
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            process.Start();
            if (!OperatingSystem.IsWindows())
            {
                process.WaitForExit();
            }
        }
        catch (Exception e)
        {
            Program.DumpError(e.Message);
        }
    }

    private string ParsePsqlCommand()
    {
        var command = settings.PsqlCommand;
        var version = GetPsqlVersion(command).Split(" ").Last().Split(".").First();
        var connVersion = connection.ServerVersion.Split(".").First();
        if (!string.Equals(connVersion, version))
        {
            command = string.Format(settings.PsqlFallback, connVersion);
            try
            {
                GetPsqlVersion(command);
                Program.WriteLine(ConsoleColor.Yellow, "",
                    $"WARNING: Using fall-back path for psql: {command}. To remove this warning set the Psql setting to point to this path.",
                    "");
            }
            catch
            {
                command = settings.PsqlCommand;
            }
        }
        return command;
    }

    private string GetPsqlVersion(string command)
    {
        var content = "";
        var error = "";
        using var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = "--version";
        if (settings.DumpPgCommands)
        {
            Program.WriteLine(ConsoleColor.White, $"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.OutputDataReceived += (_, data) =>
        {
            if (!string.IsNullOrEmpty(data.Data))
            {
                if (data.Data != null)
                {
                    content = string.Concat(content, data.Data, Environment.NewLine);
                }
            }
        };
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (_, data) =>
        {
            if (!string.IsNullOrEmpty(data.Data))
            {
                error = string.Concat(error, data.Data, Environment.NewLine);
            }
        };
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
        process.Close();
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }
        return content;
    }
}
