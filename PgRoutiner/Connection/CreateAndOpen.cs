namespace PgRoutiner.Connection;

public partial class ConnectionManager
{
    private static readonly IEnumerable<string> InfoLevels = new[] { "INFO", "NOTICE", "LOG" };
    private static readonly IEnumerable<string> ErrorLevels = new[] { "ERROR", "PANIC" };

    private NpgsqlConnection CreateAndOpen(string connectionStr)
    {
        var conn = new NpgsqlConnection(connectionStr);
        conn.Notice += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Notice.Severity))
            {
                Program.Write(ConsoleColor.Yellow, $"{args.Notice.Severity} ");
            }
            Program.WriteLine(ConsoleColor.Cyan, args.Notice.MessageText);
        };
        conn.Open();

        var keyValue = typeof(Settings).GetProperty(connectionKey).GetValue(Settings.Value) as string;
        if (!string.IsNullOrEmpty(keyValue))
        {
            Program.WriteLine("");
            Program.Write($"Using {name1} ");
            Program.Write(ConsoleColor.Yellow, keyValue);
            Program.Write(":");
            Program.WriteLine("");
            Program.WriteLine(ConsoleColor.Cyan, " " + conn.ConnectionString);
        }
        else
        {
            Program.WriteLine("");
            Program.Write($"Using {name1}:");
            Program.WriteLine(ConsoleColor.Cyan, " " + conn.ConnectionString);
        }
        return conn;
    }
}
