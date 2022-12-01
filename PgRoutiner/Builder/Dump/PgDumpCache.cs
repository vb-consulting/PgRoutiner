using System.Diagnostics;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgRoutiner.Builder.Dump;

public static class PgDumpCache
{
    private static Dictionary<string, (List<string> content, string error)> cache = new();

    public static IEnumerable<string> GetLines(NpgsqlConnection connection, string args, string pgDumpCmd)
    {
        var key = GetKey(connection, args, pgDumpCmd);
        if (cache.TryGetValue(key, out var value))
        {
            foreach (var line in value.content)
            {
                yield return line;
            }
        } 
        else
        {
            var result = GetDumpContent(connection, args, pgDumpCmd);
            cache.Add(key, result);

            foreach (var line in result.content)
            {
                yield return line;
            }
        }
    }

    public static string GetError(NpgsqlConnection connection, string args, string pgDumpCmd)
    {
        var key = GetKey(connection, args, pgDumpCmd);
        if (cache.TryGetValue(key, out var value))
        {
            return value.error;
        }

        var result = GetDumpContent(connection, args, pgDumpCmd);
        cache.Add(key, result);

        return result.error;
    }

    private static string GetKey(NpgsqlConnection connection, string args, string pgDumpCmd) => 
        $"{connection.ConnectionString}{pgDumpCmd}{args}".Replace(" ", "");

    private static (List<string> content, string error) GetDumpContent(NpgsqlConnection connection, string args, string pgDumpCmd)
    {
        //if (!string.Equals(args, "--version"))
        //{
        //    return (File.ReadAllLines("C:\\vb-consulting\\PgRoutiner\\PgRoutiner\\Test\\cpims.sql")
        //        .Where(l => !string.IsNullOrWhiteSpace(l))
        //        .ToList(), 
        //        "");
        //}
        
        var password = connection.ExtractPassword();
        Environment.SetEnvironmentVariable("PGPASSWORD", password);

        List<string> content = new();
        var error = "";

        using var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = pgDumpCmd;
        if (string.Equals(args, "--version"))
        {
            process.StartInfo.Arguments = args;
        }
        else
        {
            process.StartInfo.Arguments = string.Concat(args, " ", connection.Database);
        }
        if (Current.Value.DumpPgCommands)
        {
            Program.WriteLine(ConsoleColor.White, $"{pgDumpCmd} {process.StartInfo.Arguments}");
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.OutputDataReceived += (_, data) =>
        {
            if (!string.IsNullOrEmpty(data.Data))
            {
                content.Add(data.Data);
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

        return (content, error);
    }
}
